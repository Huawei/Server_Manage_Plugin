'''# -*- coding: utf-8 -*-
###########################################################################
# Author:    asusa
# Date:        Apri 22th 2017
# Modified:
#
# use example :
# need net-snmp python egg
# python -m testSNMPv1
# Notification Originator Application (TRAP)
###########################################################################
'''
from pysnmp.carrier.asynsock.dispatch import AsynsockDispatcher
from pysnmp.carrier.asynsock.dgram import udp
from pyasn1.codec.ber import encoder
from pysnmp.proto import api
from Crypto.PublicKey.DSA import oid
from Products.ZenEvents.Event import Event
from Products.ZenEvents.ZenEventClasses import Status_Ping
import unittest
import Globals
from Products.ZenUtils.ZCmdBase import ZCmdBase
zodb = ZCmdBase(noopts=True)
zem = zodb.dmd.ZenEventManager

# Protocol version to use
verID = api.protoVersion2c
pMod = api.protoModules[verID]

# Build PDU
trapPDU = pMod.TrapPDU()
pMod.apiTrapPDU.setDefaults(trapPDU)

trapdip = '192.168.1.108'
community = 'public'
val = [(1, 3, 6, 1, 4, 1, 2011, 2, 235, 1, 1, 4, 1, 0)]


class TestTrap(unittest.TestCase):

    def setUp(self):
        pass

    def tearDown(self):
        pass

    def teartrapevent(self):
        '''teartrapevent'''
        evt = Event()
        evt.device = "/Service/Huawei/BMC"
        evt.eventClass = 'hwOEMEvent'
        evt.summary = "Huawei OEM event"
        evt.severity = 2
        zem.sendEvent(evt)
        self.assertTrue(True)

    def testtrapv2(self):
        '''testtrapv2'''
        # Traps have quite different semantics among proto versions
        if verID == api.protoVersion2c:
            var = []
            oid = (1, 3, 6, 1, 4, 1, 2011, 2, 235, 1, 1, 4, 1, 0)
            # 1.3.6.1.4.1.2011.2.235.1.1.15.50.1.5(cpuClockRate).0
            # 1.3.6.1.4.1.2011.2.235.1.1.4.1.0  trapEnable
            val = pMod.Integer(1)
            var.append((oid, val))

            pMod.apiTrapPDU.setVarBinds(trapPDU, var)

        # Build message
        trapMsg = pMod.Message()
        pMod.apiMessage.setDefaults(trapMsg)
        pMod.apiMessage.setCommunity(trapMsg, community)
        pMod.apiMessage.setPDU(trapMsg, trapPDU)

        transportDispatcher = AsynsockDispatcher()
        transportDispatcher.registerTransport(
            udp.domainName, udp.UdpSocketTransport().openClientMode()
        )
        # 本机测试使用localhost，应为对应trap server 的IP地址。
        transportDispatcher.sendMessage(
            encoder.encode(trapMsg), udp.domainName, (trapdip, 162)
        )
        transportDispatcher.runDispatcher()
        transportDispatcher.closeDispatcher()

        self.assertTrue(True)

if __name__ == "__main__":
    # import sys;sys.argv = ['', 'Test.testName']
    unittest.main()
