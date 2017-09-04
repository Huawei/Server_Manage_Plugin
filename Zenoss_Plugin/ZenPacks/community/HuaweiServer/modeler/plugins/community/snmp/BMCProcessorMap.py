'''
BMCProcessorMap
'''
from Products.DataCollector.plugins.CollectorPlugin import (
    SnmpPlugin, GetTableMap,
    )
from DeviceDefine import BMCSTATUS


class BMCProcessorMap(SnmpPlugin):
    '''
    BMCProcessorMap
    '''

    relname = 'bmcprocessors'
    modname = 'ZenPacks.community.HuaweiServer.BMCProcessor'

    snmpGetTableMaps = (
        GetTableMap(
            'cpuTable', '1.3.6.1.4.1.2011.2.235.1.1.15.50.1', {
                '.1': 'cpuIndex',
                '.2': 'cpuManufacturer',
                '.3': 'cpuFamily',
                '.4': 'cpuType',
                '.5': 'cpuClockRate',
                '.6': 'cpuStatus',
                '.8': 'cpuLocation',
                '.10': 'cpuDevicename',
                '.12': 'cpuCoreCount',
                '.13': 'cpuThreadCount',
                }
            ),
        )

    def process(self, device, results, log):
        '''
        process oid
        '''

        log = log
        device = device
        temp_sensors = results[1].get('cpuTable', {})

        relmap = self.relMap()
        for snmpindex, row in temp_sensors.items():
            name = 'CPU_' + str(row.get('cpuIndex'))

            status = int(row.get('cpuStatus'))
            if status == 5:
                continue

            relmap.append(self.objectMap({
                'id': self.prepId(name),
                'title': name,
                'snmpindex': snmpindex.strip('.'),
                'locator': row.get('cpuLocation'),
                'manufacturer': row.get('cpuManufacturer'),
                'family': row.get('cpuFamily'),
                'type': row.get('cpuType'),
                'frequency': row.get('cpuClockRate'),
                'status': BMCSTATUS.get(row.get('cpuStatus'), 'unknown'),
                'coreCount': row.get('cpuCoreCount'),
                'threadCount': row.get('cpuThreadCount'),
                }))

        return relmap
