'''
HMMSystemMap
'''
from Products.DataCollector.plugins.CollectorPlugin import (
    SnmpPlugin, GetMap
    )
from DeviceDefine import HMMPRESENCE, HMMHEALTH


class HMMManagementBoardMap(SnmpPlugin):
    '''
    HMMSystemMap
    '''
    relname = 'hmmmanagementBoards'
    modname = 'ZenPacks.community.HuaweiServer.HMMManagementBoard'
    snmpGetMap = GetMap({
        '.1.3.6.1.4.1.2011.2.82.1.82.3.1.0': 'softwareVersion',
        '.1.3.6.1.4.1.2011.2.82.1.82.3.8.0': 'smmPresence',
        '.1.3.6.1.4.1.2011.2.82.1.82.3.9.0': 'smmHealth',
        '.1.3.6.1.4.1.2011.2.82.1.82.3.49.0': 'smmHostname',
        '.1.3.6.1.4.1.2011.2.82.1.82.3.74.0': 'smmProductName',
        })

    def process(self, device, results, log):
        '''
        process oid
        '''

        log = log
        device = device
        getdata = results[0]

        verstr = getdata.get('softwareVersion')
        verstrlist = verstr.split('\r\n')
        softwareversion = ''
        ubootversion = ''
        cpldversion = ''
        fpgaversion = ''

        for idx in verstrlist:
            if idx.find('Uboot    Version :') != -1:
                ubootversion = idx[idx.find('Uboot    Version :')+18:]
            if idx.find('CPLD     Version :') != -1:
                cpldversion = idx[idx.find('CPLD     Version :')+18:]
            if idx.find('FPGA     Version :') != -1:
                fpgaversion = idx[idx.find('FPGA     Version :')+18:]
            if idx.find('Software Version :') != -1:
                softwareversion = idx[idx.find('Software Version :')+18:]
        relmap = self.relMap()
        relmap.append(self.objectMap({
            'id': self.prepId('SMM_1'),
            'title': 'SMM_1',
            'hsProductName': getdata.get('smmProductName'),
            'hsSoftwareVersion': softwareversion,
            'hsUbootVersion': ubootversion,
            'hsCPLDVersion': cpldversion,
            'hsFPGAVersion': fpgaversion,
            'hsPresence': HMMPRESENCE.get(getdata.get('smmPresence'),
                                          'unknown'),
            'hsHostname': getdata.get('smmHostname'),
            'hsHealth': HMMHEALTH.get(getdata.get('smmHealth'), 'unknown'),
            }))
        return relmap
