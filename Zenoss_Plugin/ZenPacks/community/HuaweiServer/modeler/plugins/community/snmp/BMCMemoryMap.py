'''
BMCMemoryMap
'''
from Products.DataCollector.plugins.CollectorPlugin import (
    SnmpPlugin, GetTableMap,
)
from DeviceDefine import BMCSTATUS


class BMCMemoryMap(SnmpPlugin):
    '''
    BMCMemoryMap
    '''

    relname = 'bmcmemorys'
    modname = 'ZenPacks.community.HuaweiServer.BMCMemory'

    snmpGetTableMaps = (
        GetTableMap(
            'memoryTable', '1.3.6.1.4.1.2011.2.235.1.1.16.50.1', {
                '.1': 'memoryDimmIndex',
                '.3': 'memoryManufacturer',
                '.4': 'memorySize',
                '.5': 'memoryClockRate',
                '.6': 'memoryStatus',
                '.8': 'memoryLocation',
                '.10': 'memoryDevicename',
                }
            ),
        )

    def process(self, device, results, log):
        '''
        process oid
        '''

        log = log
        device = device
        temp_sensors = results[1].get('memoryTable', {})

        relmap = self.relMap()
        for snmpindex, row in temp_sensors.items():
            bbit = (int(row.get('memoryDimmIndex'))-1) / 8
            mbit = (int(row.get('memoryDimmIndex'))-1) % 4
            sbit = (int(row.get('memoryDimmIndex'))-1) % 2

            name = 'DIMM_' + str(bbit) + str(mbit) + str(sbit)
            if not name:
                log.warn('Skipping temperature sensor with no name')
                continue

            status = int(row.get('memoryStatus'))
            if status == 5:
                continue

            relmap.append(self.objectMap({
                'id': self.prepId(name),
                'title': name,
                'snmpindex': snmpindex.strip('.'),
                'locator': row.get('memoryLocation'),
                'manufacturer': row.get('memoryManufacturer'),
                'size': row.get('memorySize'),
                'frequency': row.get('memoryClockRate'),
                'status': BMCSTATUS.get(row.get('memoryStatus'), 'unknown')
                }))

        return relmap
