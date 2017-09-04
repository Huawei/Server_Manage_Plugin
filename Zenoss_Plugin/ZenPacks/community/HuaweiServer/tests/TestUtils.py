'''
###########################################################################
# Author:    kingkong
# Date:        May 12th 2017
# Modified:
#
# use example :
# just test for IPS
# python -m testUtils
###########################################################################
'''
import unittest


class TestUtils(unittest.TestCase):
    '''test utils suite'''

    def setUp(self):
        pass

    def tearDown(self):
        pass

    def extractips(self, ips, iplist):
        '''extractips'''
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

    def testipsplitblank(self):
        '''testIPSplitBlank'''
        ips = '10.0.0.1 '
        iplist = []
        self.extractips(ips, iplist)
        print iplist
        iplistdst = ['10.0.0.1']
        self.assertEqual(iplist, iplistdst, '')

    def testipsplitblankcomma(self):
        '''testIPSplitBlankComma'''
        ips = '10.0.0.1 ,'
        iplist = []
        self.extractips(ips, iplist)
        print iplist
        iplistdst = ['10.0.0.1']
        self.assertEqual(iplist, iplistdst, '')

    def testipsplitcommablank(self):
        '''testIPSplitCommaBlank'''
        ips = '10.0.0.1,'
        iplist = []
        self.extractips(ips, iplist)
        print iplist
        iplistdst = ['10.0.0.1']
        self.assertEqual(iplist, iplistdst, '')

    def testipsplitdash(self):
        '''testIPSplitDash'''
        ips = '10.0.0.1-25'
        iplist = []
        self.extractips(ips, iplist)
        print iplist
        iplistdst = ['10.0.0.1',
                     '10.0.0.2',
                     '10.0.0.3',
                     '10.0.0.4',
                     '10.0.0.5',
                     '10.0.0.6',
                     '10.0.0.7',
                     '10.0.0.8',
                     '10.0.0.9',
                     '10.0.0.10',
                     '10.0.0.11',
                     '10.0.0.12',
                     '10.0.0.13',
                     '10.0.0.14',
                     '10.0.0.15',
                     '10.0.0.16',
                     '10.0.0.17',
                     '10.0.0.18',
                     '10.0.0.19',
                     '10.0.0.20',
                     '10.0.0.21',
                     '10.0.0.22',
                     '10.0.0.23',
                     '10.0.0.24',
                     '10.0.0.25']
        self.assertEqual(iplist, iplistdst, '')

    def testipsplitcomma(self):
        '''testIPSplitComma'''
        ips = '10.0.0.1,10.0.0.25'
        iplist = []
        self.extractips(ips, iplist)
        print iplist
        iplistdst = ['10.0.0.1',
                     '10.0.0.25']
        self.assertEqual(iplist, iplistdst, '')

    def testipsplitcomplex(self):
        '''testIPSplitComplex'''
        ips = '10.0.0.1-25,10.0.0.35'
        iplist = []
        self.extractips(ips, iplist)
        print iplist
        iplistdst = ['10.0.0.1',
                     '10.0.0.2',
                     '10.0.0.3',
                     '10.0.0.4',
                     '10.0.0.5',
                     '10.0.0.6',
                     '10.0.0.7',
                     '10.0.0.8',
                     '10.0.0.9',
                     '10.0.0.10',
                     '10.0.0.11',
                     '10.0.0.12',
                     '10.0.0.13',
                     '10.0.0.14',
                     '10.0.0.15',
                     '10.0.0.16',
                     '10.0.0.17',
                     '10.0.0.18',
                     '10.0.0.19',
                     '10.0.0.20',
                     '10.0.0.21',
                     '10.0.0.22',
                     '10.0.0.23',
                     '10.0.0.24',
                     '10.0.0.25',
                     '10.0.0.35']
        self.assertEqual(iplist, iplistdst, '')

# BladeHMMVersionString split
    def testversionstringsplit(self):
        '''testVersionStringSplit'''
        verstr = '''Uboot    Version :(U54)3.03\r\n
        CPLD     Version :(U1082)108  161206\r\n\r
        PCB      Version :SMMA REV B\r\n\r
        BOM      Version :001\r\n
        FPGA     Version :(U1049)008  130605\r\n
        Software Version :(U54)6.03\r\n
        IPMI Module Built:Mar 17 2017 11:54:28'''
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
        print ubootversion, cpldversion, fpgaversion, softwareversion
        self.assertEqual(ubootversion, '(U54)3.03', '')

# BladeIPString split
    def testipstringsplit(self):
        '''testIPStringSplit'''
        ipstr = '''the Straight BMC IP: 172.180.0.211,
         the Straight BMC Mask: 255.255.0.0'''
        ipstrlist = ipstr.split(',')
        bnip = ''
        bnipmask = ''
        for idx in ipstrlist:
            if idx.find('IP:') != -1:
                bnip = idx[idx.find('IP:')+4:]
            if idx.find('Mask: ') != -1:
                bnipmask = idx[idx.find('Mask:')+6:]
        print bnip, bnipmask
        self.assertEqual(bnip, '172.180.0.211', '')

    def teststringsplit(self):
        strs = 'hwOEMEvent    Huawei OEM event    2    1.3.6.1.4.1.2011.2.235.1.1.500.10.1.1'
        a,b,c = [str(i) for i in strs.split('\t')]
        print a,b,c
        self.assertEqual(1, 1, '')
if __name__ == "__main__":
    unittest.main()
