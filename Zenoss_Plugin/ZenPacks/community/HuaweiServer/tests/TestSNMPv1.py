'''
###########################################################################
# Author:    tangtang
# Date:        Apri 20th 2017
# Modified:
#
# use example :
# need net-snmp python egg
# python -m testSNMPv1
###########################################################################
'''
import unittest
import netsnmp


class TestSNMPv1(unittest.TestCase):
    '''snmp v1 test suite'''

    sess = None
    ipstr = '10.0.2.2'
    community = 'public'
    snmpver = 1

    def setUp(self):
        pass

    def tearDown(self):
        pass

    def testsnmpget(self):
        '''snmp get oids'''
        snmpvar = netsnmp.Varbind('.1.3.6.1.2.1.1.1.0')
        res, = netsnmp.snmpget(snmpvar,
                               Version=self.snmpver,
                               DestHost=self.ipstr,
                               Community=self.community)
        self.assertIsNotNone(res)

    def testsnmpgetcpu(self):
        '''snmp get cpu'''
        snmpvar = netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.1.23.0')
        res, = netsnmp.snmpget(snmpvar,
                               Version=self.snmpver,
                               DestHost=self.ipstr,
                               Community=self.community)
        self.assertTrue(str(res).isdigit())

    def testsnmpgetpeakpower(self):
        '''snmp get peakpower'''
        snmpvar = netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.20.1.0')
        res, = netsnmp.snmpget(snmpvar,
                               Version=self.snmpver,
                               DestHost=self.ipstr,
                               Community=self.community)
        print res
        self.assertTrue(str(res).isdigit())

    def testsnmpgetpensentsystempower(self):
        '''snmp get pensentsystempower'''
        snmpvar = netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.1.13.0')
        res, = netsnmp.snmpget(snmpvar,
                               Version=self.snmpver,
                               DestHost=self.ipstr,
                               Community=self.community)
        print res
        self.assertTrue(str(res).isdigit())

    def testsnmpgetprocesstable(self):
        '''snmp get processtable'''
        snmpvars = netsnmp.VarList(
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.15.50.1.1.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.15.50.1.2.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.15.50.1.3.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.15.50.1.4.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.15.50.1.5.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.15.50.1.6.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.15.50.1.8.1'))
        vals = self.sess.get(snmpvars)
        print vals
        self.assertIsNotNone(vals)

    def testsnmpgetmemorytable(self):
        '''snmp get memorytable'''
        snmpvars = netsnmp.VarList(
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.16.50.1.1.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.16.50.1.3.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.16.50.1.4.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.16.50.1.5.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.16.50.1.6.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.16.50.1.8.1'))
        vals = self.sess.get(snmpvars)
        print vals
        self.assertIsNotNone(vals)

    def testsnmpgetharddisktable(self):
        '''snmp get harddisktable'''
        snmpvars = netsnmp.VarList(
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.18.50.1.1.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.18.50.1.2.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.18.50.1.3.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.18.50.1.4.1'))
        vals = self.sess.get(snmpvars)
        print vals
        self.assertIsNotNone(vals)

    def testsnmpgetpowersupplytable(self):
        '''snmp get powersupplytable'''
        snmpvars = netsnmp.VarList(
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.6.50.1.1.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.6.50.1.2.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.6.50.1.4.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.6.50.1.6.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.6.50.1.7.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.6.50.1.8.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.6.50.1.9.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.6.50.1.11.1'))
        vals = self.sess.get(snmpvars)
        print vals
        self.assertIsNotNone(vals)

    def testsnmpset(self):
        '''snmp set test'''
        var = netsnmp.Varbind('.1.3.6.1.2.1.1.6', '0', 'zenoss')
        res = netsnmp.snmpset(var,
                              Version=self.snmpver,
                              DestHost=self.ipstr,
                              Community=self.community)
        print res
        self.assertEqual(0, res)

    def testsnmpsetbootsequence(self):
        '''snmp set bootsequence'''
        var = netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.1.2.0', '1')
        res = netsnmp.snmpset(var,
                              Version=self.snmpver,
                              DestHost=self.ipstr,
                              Community=self.community)
        print res
        self.assertEqual(0, res)

if __name__ == "__main__":
    unittest.main()
