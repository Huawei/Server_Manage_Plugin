'''
###########################################################################
# Author:    tangtang
# Date:        Apri 24th 2017
# Modified:
#
# use example :
# need net-snmp python egg
# python -m testSNMPv2
###########################################################################
'''
import unittest
import netsnmp


# snmpv2 test suites
class TestSNMPv2(unittest.TestCase):
    '''snmp v2 test suite'''

    sess = None
    ipstr = '10.0.2.2'
    community = 'public'
    snmpver = 2

    # init snmp enviroment
    def setUp(self):
        self.sess = netsnmp.Session(Version=self.snmpver,
                                    DestHost=self.ipstr,
                                    Community=self.community)

        self.sess.UseEnums = 1
        self.sess.UseLongNames = 1

    # uninit snmp enviroment
    def tearDown(self):
        pass

    # snmp get test
    def testsnmpget(self):
        '''snmp get oid2'''
        snmpvars = netsnmp.VarList(netsnmp.Varbind('.1.3.6.1.2.1.1.3.0'))
        vals, = self.sess.get(snmpvars)
        self.assertIsNotNone(vals)

    def testgetaveragepower(self):
        '''snmp get AveragePower'''

        snmpvars = netsnmp.VarList(netsnmp.Varbind(
            '.1.3.6.1.4.1.2011.2.235.1.1.20.3.0'))
        vals, = self.sess.get(snmpvars)
        self.assertIsNotNone(vals)

    # snmp get PowerConsumption
    def testsnmpgetpowerconsumption(self):
        '''snmp get powerconsumption'''
        snmpvars = netsnmp.VarList(netsnmp.Varbind(
            '.1.3.6.1.4.1.2011.2.235.1.1.20.4.0'))
        vals, = self.sess.get(snmpvars)
        self.assertIsNotNone(vals)

    # snmp get TemperatureIntlet
    def testsnmpgettemperatureinlet(self):
        '''snmp get temperatureinlet'''
        snmpvars = netsnmp.VarList(netsnmp.Varbind(
            '.1.3.6.1.4.1.2011.2.235.1.1.26.50.1.3.1'))
        vals, = self.sess.get(snmpvars)
        self.assertIsNotNone(vals)

    # snmp get SmmInletTemp
    def testsnmpgetsmminlettemp(self):
        '''snmp get smminlettemp'''
        snmpvars = netsnmp.VarList(netsnmp.Varbind(
            '.1.3.6.1.4.1.2011.2.82.1.82.3.2008.1.2.1.0'))
        vals, = self.sess.get(snmpvars)
        self.assertIsNotNone(vals)

    # snmp get HMMSystemTable
    def testsnmpgethmmsystemtable(self):
        '''snmp get hmmsystemtable'''
        snmpvars = netsnmp.VarList(
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.3.1.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.3.2.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.3.8.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.3.9.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.3.49.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.3.71.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.3.74.0'))
        vals = self.sess.get(snmpvars)
        print vals
        self.assertIsNotNone(vals)

    # snmp get BladeTable
    def testsnmpgethmmbladetable(self):
        '''snmp get hmmbladetable'''
        snmpvars = netsnmp.VarList(
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.4.1.8.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.4.1.20.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.4.1.32.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.4.1.57.0'))
        vals = self.sess.get(snmpvars)
        print vals
        self.assertIsNotNone(vals)

    # snmp get HMMPowerSupplyTable
    def testsnmpgethmmpowersupplytable(self):
        '''snmp get hmmpowersupplytable'''
        snmpvars = netsnmp.VarList(
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.6.2001.1.1.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.6.2001.1.2.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.6.2001.1.3.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.6.2001.1.4.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.6.2001.1.5.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.6.2001.1.8.1'))
        vals = self.sess.get(snmpvars)
        print vals
        self.assertIsNotNone(vals)

    # snmp get HMMChassisTable
    def testsnmpgethmmchassistable(self):
        '''snmp get hmmchassistable'''
        snmpvars = netsnmp.VarList(
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.2.4.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.2.5.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.2.8.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.2.13.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.2.14.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.2.15.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.2.18.0'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.2.2001.1.16.1.0'))
        vals = self.sess.get(snmpvars)
        print vals
        self.assertIsNotNone(vals)

    # snmp set FruControl
    def testsnmpsetfrucontrol(self):
        '''snmp set frucontrol'''
        var = netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.7.50.1.2', '1', '1')
        res = netsnmp.snmpset(var,
                              Version=self.snmpver,
                              DestHost=self.ipstr,
                              Community=self.community)
        print res
        self.assertEqual(0, res)
        # self.assertTrue(True)

    # snmp set BladeFruControl
    def testsnmpsetbladefrucontrol(self):
        '''snmp set bladefrucontrol'''
        var = netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.4.32.2002.1.9',
                              '1', '1')
        res = netsnmp.snmpset(var,
                              Version=self.snmpver,
                              DestHost=self.ipstr,
                              Community=self.community)
        print res
        self.assertEqual(0, res)
        # self.assertTrue(True)

if __name__ == "__main__":
    unittest.main()
