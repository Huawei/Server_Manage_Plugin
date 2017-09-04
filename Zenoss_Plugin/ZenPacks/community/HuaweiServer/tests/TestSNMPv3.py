'''
###########################################################################
# Author:    tangtang
# Date:        Apri 28th 2017
# Modified:
#
# use example :
# need net-snmp python egg
# python -m testSNMPv3
###########################################################################
'''
import unittest
import netsnmp


class TestSNMPv3(unittest.TestCase):
    '''snmp v2 test suite'''

    sess = None
    ipstr = '10.0.2.2'
    community = 'public'
    snmpver = 3

    def setUp(self):
        self.sess = netsnmp.Session(Version=self.snmpver,
                                    DestHost=self.ipstr,
                                    SecLevel='authPriv',
                                    Context=self.community,
                                    SecName='simulator',
                                    PrivPass='privatus',
                                    AuthPass='auctoritas')

        self.sess.UseSprintValue = 1

    def tearDown(self):
        pass

    def testsnmpgetsmmoutlettemp(self):
        '''snmp get smmoutlettemp'''
        snmpvars = netsnmp.VarList(
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.3.2008.1.2.2.0'))
        vals, = self.sess.get(snmpvars)
        self.assertIsNotNone(vals)

    def testsnmpgetsmmlswtemp(self):
        '''snmp get smmlswtemp'''
        snmpvars = netsnmp.VarList(
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.3.2008.1.2.3.0'))
        vals, = self.sess.get(snmpvars)
        self.assertIsNotNone(vals)

    def testsnmpgetsmmambienttemp(self):
        '''snmp get smmambienttemp'''
        snmpvars = netsnmp.VarList(
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.3.2008.1.2.4.0'))
        vals, = self.sess.get(snmpvars)
        self.assertIsNotNone(vals)

    def testsnmpgetbmcfantable(self):
        '''snmp get bmcfantable'''
        snmpvars = netsnmp.VarList(
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.8.50.1.1.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.8.50.1.2.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.8.50.1.3.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.8.50.1.4.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.235.1.1.8.50.1.5.1'))
        vals = self.sess.get(snmpvars)
        print vals
        self.assertIsNotNone(vals)

    def testsnmpgethmmfantable(self):
        '''snmp get hmmfantable'''
        snmpvars = netsnmp.VarList(
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.5.2001.1.1.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.5.2001.1.2.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.5.2001.1.3.1'),
            netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.5.2001.1.7.1'))
        vals = self.sess.get(snmpvars)
        print vals
        self.assertIsNotNone(vals)

    def testsnmpsetbladebiosoption(self):
        '''snmp set bladebiosoption'''
        var = netsnmp.Varbind('.1.3.6.1.4.1.2011.2.82.1.82.4.32.32',
                              '0', '1')
        res = netsnmp.snmpset(var)
        print res
        self.assertEqual(0, res)

if __name__ == "__main__":
    unittest.main()
