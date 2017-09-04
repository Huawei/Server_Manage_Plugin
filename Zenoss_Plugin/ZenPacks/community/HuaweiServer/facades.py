'''# -*- coding: utf-8 -*-
web router facades
'''
import os
from zope.interface import implements
from Products.Zuul.facades import ZuulFacade
from Products.Zuul.utils import ZuulMessageFactory as _t
from .interfaces import IBMCFacade
from .interfaces import IHMMFacade
from Products.ZenUtils.Utils import executeCommand
import logging
log = logging.getLogger('.'.join(['', __name__]))


class BMCFacade(ZuulFacade):
    '''
    BMC Facade
    '''
    implements(IBMCFacade)

    def extractips(self, ips, iplist):
        '''
        extractips
        '''
        deviceip1 = ips.replace(' ', '')
        for ipstr in deviceip1.split(","):
            if ipstr == '':
                continue
            deviceip = ipstr.split('-')
            if len(deviceip) == 2:
                if deviceip[0].count('.') == 3:
                    rangestart = int(deviceip[0][deviceip[0].rfind('.') + 1:])
                    rangeend = int(deviceip[1])
                    part1 = deviceip[0][:deviceip[0].rfind('.') + 1]
                    for iptmp in range(rangestart, rangeend + 1):
                        iplist.append(part1 + str(iptmp))
            else:
                iplist.append(ipstr)

    # Note that the the facade function, myFacadeFunc has 3 parameters
    #  The object is passed in addition to the comment and rackSlot

    def bootsequencesingle(self, deviceip, bootsequence, allbmcdevice):
        '''
        bootsequencesingle
        '''
        deviceroot = self._dmd.getDmdRoot("Devices")
        device = deviceroot.findDevice(deviceip)
        if device is None:
            return [deviceip, "device Not found!"]
        log.debug("myFacadeFunc data %s %s %s %s", deviceip,
                  allbmcdevice, device.__class__, device.zSnmpVer)
    #         arg1 = deviceip
    #         arg2 = "-v2c"
    #         arg3 = "-cpublic"
    #         arg4 = ""
    #         arg5 = ""
    #         arg6 = ""
    #         arg7 = ""
    #         arg8 = ""
        arg1 = deviceip
        arg2 = "-" + device.zSnmpVer
        arg3 = "-c" + device.zSnmpCommunity
        arg4 = "-u" + device.zSnmpSecurityName
        arg5 = "-a" + device.zSnmpAuthType
        arg6 = "-A" + device.zSnmpAuthPassword
        arg7 = "-x" + device.zSnmpPrivType
        arg8 = "-X" + device.zSnmpPrivPassword
        bootsequencearg = str(bootsequence)
        arg9 = bootsequencearg

        libexec = os.path.join(os.path.dirname(__file__), 'libexec')
        predefinedcmd = []
        if arg2 == "-v2c":
            predefinedcmd = [
                libexec + '/bmcbootsequence.sh', arg1, arg2, arg3, arg4,
                arg5, arg6, arg7, arg8, arg9]
        else:
            predefinedcmd = [
                libexec + '/bmcbootsequence.sh', arg1, arg2, arg3, arg4,
                arg5, arg6, arg7, arg8, arg9]
        result = executeCommand(predefinedcmd, None, None)
        retstr = "Fail"
        if result == 0:
            retstr = "Success"
        log.info("executeCommand result %s ", result)
        return [deviceip, retstr]

    def bootsequencetype(self, deviceip, bootsequence, allbmcdevice):
        '''
        bootsequencetype
        '''
        deviceroot = self._dmd.getDmdRoot("Devices")
        device = deviceroot.findDevice(deviceip)
        if device is None:
            return [deviceip, "device Not found!"]
        log.debug("myFacadeFunc data %s %s %s %s", deviceip,
                  allbmcdevice, device.__class__, device.zSnmpVer)
    #         arg1 = deviceip
    #         arg2 = "-v2c"
    #         arg3 = "-cpublic"
    #         arg4 = ""
    #         arg5 = ""
    #         arg6 = ""
    #         arg7 = ""
    #         arg8 = ""
        arg1 = deviceip
        arg2 = "-" + device.zSnmpVer
        arg3 = "-c" + device.zSnmpCommunity
        arg4 = "-u" + device.zSnmpSecurityName
        arg5 = "-a" + device.zSnmpAuthType
        arg6 = "-A" + device.zSnmpAuthPassword
        arg7 = "-x" + device.zSnmpPrivType
        arg8 = "-X" + device.zSnmpPrivPassword
        arg9 = bootsequence

        if not bootsequence.isdigit():
            return [deviceip, 'option not in range']
        if int(bootsequence) > 20:
            return [deviceip, 'option not in range']

        libexec = os.path.join(os.path.dirname(__file__), 'libexec')
        predefinedcmd = []
        if arg2 == "-v2c":
            predefinedcmd = [
                libexec + '/bmcboottype.sh', arg1, arg2, arg3, arg4,
                arg5, arg6, arg7, arg8, arg9]
        else:
            predefinedcmd = [
                libexec + '/bmcboottype.sh', arg1, arg2, arg3, arg4,
                arg5, arg6, arg7, arg8, arg9]
        result = executeCommand(predefinedcmd, None, None)

        retstr = "Fail"
        if result == 0:
            retstr = "Success"
        log.info("executeCommand result %s ", result)
        return [deviceip, retstr]

    def bootsequence(self, devobj, deviceip, bootsequence, boottype):
        """ Modifies bootsequence and boottype attributes for a device """
        iplist = []
        self.extractips(deviceip, iplist)
        ipret = []
        for ipstr in iplist:
            if boottype == 1:
                self.bootsequencetype(ipstr, "1", boottype)
            elif boottype == 2:
                self.bootsequencetype(ipstr, "2", boottype)
            result = self.bootsequencesingle(ipstr, bootsequence, boottype)
            ipret.append(result)
        return True, _t("BMC Boot Sequence set device %s" % ipret)

    def frupowerctrlsingle(self, deviceip, frupowercontrol):
        '''
        frupowerctrlsingle
        '''
        deviceroot = self._dmd.getDmdRoot("Devices")
        device = deviceroot.findDevice(deviceip)
        if device is None:
            return [deviceip, "device Not found!"]
        arg1 = deviceip
        arg2 = "-" + device.zSnmpVer
        arg3 = "-c" + device.zSnmpCommunity
        arg4 = "-u" + device.zSnmpSecurityName
        arg5 = "-a" + device.zSnmpAuthType
        arg6 = "-A" + device.zSnmpAuthPassword
        arg7 = "-x" + device.zSnmpPrivType
        arg8 = "-X" + device.zSnmpPrivPassword
        frupowercontrolarg = str(frupowercontrol)
        arg9 = frupowercontrolarg

        if not frupowercontrolarg.isdigit():
            return [deviceip, 'option not in range']
        if int(frupowercontrolarg) > 10:
            return [deviceip, 'option not in range']

        libexec = os.path.join(os.path.dirname(__file__), 'libexec')
        predefinedcmd = []
        if arg2 == "-v2c":
            predefinedcmd = [
                libexec + '/bmcfrupowercontrol.sh', arg1, arg2, arg3, arg4,
                arg5, arg6, arg7, arg8, arg9]
        else:
            predefinedcmd = [
                libexec + '/bmcfrupowercontrol.sh', arg1, arg2, arg3, arg4,
                arg5, arg6, arg7, arg8, arg9]
        result = executeCommand(predefinedcmd, None, None)
        retstr = "Fail"
        if result == 0:
            retstr = "Success"
        log.info("executeCommand result %s ", result)
        return [deviceip, retstr]

    def frupowerctrl(self, devobj, deviceip, frunum, frupowercontrol):
        """ Modifies frunum and frupowercontrol attributes for a device """
        frunum = frunum
        devobj = devobj
        iplist = []
        self.extractips(deviceip, iplist)
        ipret = []
        for ipstr in iplist:
            ret = self.frupowerctrlsingle(ipstr, frupowercontrol)
            ipret.append(ret)
        return True, _t("BMC FRU Power Control set for device %s" % (ipret))


class HMMFacade(ZuulFacade):
    '''
    HMM Facade
    '''
    implements(IHMMFacade)
    # deprecated class for can not link with URL router

    def extractips(self, ips, iplist):
        '''
        extractips
        '''
        deviceip1 = ips.replace(' ', '')
        for ipstr in deviceip1.split(","):
            if ipstr == '':
                continue
            deviceip = ipstr.split('-')
            if len(deviceip) == 2:
                if deviceip[0].count('.') == 3:
                    rangestart = int(deviceip[0][deviceip[0].rfind('.') + 1:])
                    rangeend = int(deviceip[1])
                    part1 = deviceip[0][:deviceip[0].rfind('.') + 1]
                    for iptmp in range(rangestart, rangeend + 1):
                        iplist.append(part1 + str(iptmp))
            else:
                iplist.append(ipstr)

    def biosbootoptionsingleblade(self, arg1, arg2, arg3, arg4, arg5,
                                  arg6, arg7, arg8, arg9, arg10):
        '''
        biosbootoptionsingleblade
        '''
        log.info("biosbootoptionsingleblade entry")
        libexec = os.path.join(os.path.dirname(__file__), 'libexec')
        predefinedcmd = []
        if arg2 == "-v2c":
            predefinedcmd = [
                libexec + '/hmmbladebiosbootoption.sh', arg1, arg2,
                arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10]
        else:
            predefinedcmd = [
                libexec + '/hmmbladebiosbootoption.sh', arg1, arg2,
                arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10]
        result = executeCommand(predefinedcmd, None, None)
        log.info("executeCommand result %s ", result)
        return result

    def biosbootoptionsingle(self, devobj, deviceip, hmmbladenum, hmmbbo,
                             hmmbotype):
        '''
        biosbootoptionsingle
        '''
        devobj = devobj
        deviceroot = self._dmd.getDmdRoot("Devices")
        device = deviceroot.findDevice(deviceip)
        if device is None:
            log.info("device Not found")
            return [deviceip, "device Not found!"]
        log.info("myFacadeFunc data %s %s %s", deviceip,
                 device.__class__, device.zSnmpVer)
        arg1 = deviceip
        arg2 = "-" + device.zSnmpVer
        arg3 = "-c" + device.zSnmpCommunity
        arg4 = "-u" + device.zSnmpSecurityName
        arg5 = "-a" + device.zSnmpAuthType
        arg6 = "-A" + device.zSnmpAuthPassword
        arg7 = "-x" + device.zSnmpPrivType
        arg8 = "-X" + device.zSnmpPrivPassword
        arg9 = hmmbladenum
        arg10 = str(hmmbbo)

        if hmmbotype == 0:
            arg10 = "disable"
        elif hmmbotype == 1:
            arg10 = "once,"+str(hmmbbo)
        else:
            arg10 = "persistent,"+str(hmmbbo)

        bladelist = []
        if hmmbladenum.isdigit():
            bladelist.append(hmmbladenum)
        elif ',' in hmmbladenum:
            bladelist = hmmbladenum.split(",")
        elif '-' in hmmbladenum:
            bladeliststr = hmmbladenum.split("-")
            if len(bladeliststr) == 2:
                rangestart = int(bladeliststr[0])
                rangeend = int(bladeliststr[1])
                if rangestart == rangeend:
                    bladelist.append(str(rangestart))
                else:
                    for iptmp in range(rangestart, rangeend + 1):
                        bladelist.append(str(iptmp))

        ipret = []
        for bladen in bladelist:
            if not bladen.isdigit():
                continue
            if int(bladen) < 1 or int(bladen) > 32:
                continue

            arg9 = str(bladen)
            retstr = "Fail"
            ret = self.biosbootoptionsingleblade(
                arg1, arg2, arg3, arg4, arg5,
                arg6, arg7, arg8, arg9, arg10)
            if ret == 0:
                retstr = "Success"
            ipret.append([arg9, retstr])

        return [deviceip, ipret]

    def biosbootoption(self, devobj, deviceip, hmmbladenum, hmmbbo,
                       hmmbotype):
        """ Modifies bladenum and biosbootoption attributes for a device """
        iplist = []
        ipret = []
        self.extractips(deviceip, iplist)
        for ipstr in iplist:
            ipret.append(self.biosbootoptionsingle(devobj, ipstr,
                                                   hmmbladenum, hmmbbo,
                                                   hmmbotype))

        return True, _t("HMM Bios Boot Option set device %s" % ipret)

    def frucontrolsingleblade(self, arg1, arg2, arg3, arg4, arg5,
                              arg6, arg7, arg8, arg9, arg10):
        '''
        frucontrolsingleblade
        '''
        libexec = os.path.join(os.path.dirname(__file__), 'libexec')
        predefinedcmd = []
        if arg2 == "-v2c":
            predefinedcmd = [
                libexec + '/hmmbladefrucontrol.sh', arg1, arg2,
                arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10]
        else:
            predefinedcmd = [
                libexec + '/hmmbladefrucontrol.sh', arg1, arg2,
                arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10]
        result = executeCommand(predefinedcmd, None, None)
        log.info("executeCommand result %s ", result)
        return result

    def frucontrolsingle(self, devobj, deviceip, hmmbladenum, hmmfrucontrol):
        '''
        frucontrolsingle
        '''
        devobj = devobj
        deviceroot = self._dmd.getDmdRoot("Devices")
        device = deviceroot.findDevice(deviceip)
        if device is None:
            return [deviceip, "device Not found!"]
        arg1 = deviceip
        arg2 = "-" + device.zSnmpVer
        arg3 = "-c" + device.zSnmpCommunity
        arg4 = "-u" + device.zSnmpSecurityName
        arg5 = "-a" + device.zSnmpAuthType
        arg6 = "-A" + device.zSnmpAuthPassword
        arg7 = "-x" + device.zSnmpPrivType
        arg8 = "-X" + device.zSnmpPrivPassword
        arg9 = hmmbladenum
        hmmfrucontrol = str(hmmfrucontrol)
        arg10 = hmmfrucontrol

        if not hmmfrucontrol.isdigit():
            return 'option not in range'
        if int(hmmfrucontrol) > 10:
            return 'option not in range'

        bladelist = []
        if hmmbladenum.isdigit():
            bladelist.append(hmmbladenum)
        elif ',' in hmmbladenum:
            bladelist = hmmbladenum.split(",")
        elif '-' in hmmbladenum:
            bladeliststr = hmmbladenum.split("-")
            if len(bladeliststr) == 2:
                rangestart = int(bladeliststr[0])
                rangeend = int(bladeliststr[1])
                if rangestart == rangeend:
                    bladelist.append(str(rangestart))
                else:
                    for iptmp in range(rangestart, rangeend + 1):
                        bladelist.append(str(iptmp))

        ipret = []
        for bladen in bladelist:
            if not bladen.isdigit():
                continue
            if int(bladen) < 1 or int(bladen) > 32:
                continue

            arg9 = str(bladen)
            retstr = "Fail"
            ret = self.frucontrolsingleblade(arg1, arg2, arg3, arg4,
                                             arg5, arg6, arg7, arg8,
                                             arg9, arg10)
            if ret == 0:
                retstr = "Success"
            ipret.append([arg9, retstr])

        return [deviceip, ipret]

    def frucontrol(self, devobj, deviceip, hmmbladenum, hmmfrunum,
                   hmmfrucontrol, hmmallblade):
        """ Modifies frunum and frupowercontrol attributes for a device """
        hmmfrunum = hmmfrunum
        hmmallblade = hmmallblade
        iplist = []
        ipret = []
        self.extractips(deviceip, iplist)
        for ipstr in iplist:
            ipret.append(self.frucontrolsingle(
                devobj, ipstr, hmmbladenum, hmmfrucontrol))

        return True, _t("HMM FRU Control set for device %s" % (ipret))
