'''
BMCPowerConsumptionMap
'''
from Products.DataCollector.plugins.CollectorPlugin import (
    SnmpPlugin, GetMap
    )
from DeviceDefine import BMCPCESTATUS, BMCPCFA


class BMCPowerConsumptionMap(SnmpPlugin):
    '''
    BMCPowerConsumptionMap
    '''

    relname = 'bmcpowerConsumptions'
    modname = 'ZenPacks.community.HuaweiServer.BMCPowerConsumption'
    snmpGetMap = GetMap({
        '.1.3.6.1.4.1.2011.2.235.1.1.1.13.0': 'presentSystemPower',
        '.1.3.6.1.4.1.2011.2.235.1.1.20.1.0': 'peakPower',
        '.1.3.6.1.4.1.2011.2.235.1.1.20.3.0': 'averagePower',
        '.1.3.6.1.4.1.2011.2.235.1.1.20.4.0': 'powerConsumption',
        '.1.3.6.1.4.1.2011.2.235.1.1.23.1.0': 'powerCappingEnable',
        '.1.3.6.1.4.1.2011.2.235.1.1.23.2.0': 'powerCappingValue',
        '.1.3.6.1.4.1.2011.2.235.1.1.23.3.0': 'powerCappingFailureAction',
        })

    def process(self, device, results, log):
        '''
        process oid
        '''

        log = log
        device = device
        getdata = results[0]

        psp = str(getdata.get('presentSystemPower'))
        relmap = self.relMap()
        relmap.append(self.objectMap({
            'id': self.prepId('PC_1'),
            'title': '1',
            'presentSystemPower': psp + '(Watts)',
            'peakPower': str(getdata.get('peakPower'))+'(Watts)',
            'averagePower': str(getdata.get('averagePower'))+'(Watts)',
            'powerConsumption': str(getdata.get('powerConsumption'))+'(kWh)',
            'powerCappingEnable': BMCPCESTATUS.get(
                getdata.get('powerCappingEnable'), 'unknown'),
            'powerCappingValue': getdata.get('powerCappingValue'),
            'powerCappingFailureAction':
                BMCPCFA.get(getdata.get('powerCappingFailureAction'),
                            'unknown'),
            }))
        return relmap
