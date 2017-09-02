'''
BMCHarddiskMap
'''
from Products.DataCollector.plugins.CollectorPlugin import (
    SnmpPlugin, GetTableMap,
    )
from DeviceDefine import BMCSTATUS, BMCPRESENCE


class BMCHarddiskMap(SnmpPlugin):
    '''
    BMCHarddiskMap
    '''

    relname = 'bmcharddisks'
    modname = 'ZenPacks.community.HuaweiServer.BMCHarddisk'

    snmpGetTableMaps = (
        GetTableMap(
            'hardDiskTable', '1.3.6.1.4.1.2011.2.235.1.1.18.50.1', {
                '.1': 'hardDiskIndex',
                '.2': 'hardDiskPresence',
                '.3': 'hardDiskStatus',
                '.4': 'hardDiskLocation',
                '.6': 'hardDiskDevicename',
                }
            ),
        )

    def process(self, device, results, log):
        '''
        process oid
        '''

        log = log
        device = device
        temp_sensors = results[1].get('hardDiskTable', {})

        relmap = self.relMap()
        for snmpindex, row in temp_sensors.items():
            name = 'HDD_' + str(row.get('hardDiskIndex'))
            if not name:
                log.warn('Skipping temperature sensor with no name')
                continue
            if 2 != int(row.get('hardDiskPresence')):
                continue
            relmap.append(self.objectMap({
                'id': self.prepId(name),
                'title': name,
                'snmpindex': snmpindex.strip('.'),
                'locator': row.get('hardDiskLocation'),
                'presence': BMCPRESENCE.get(row.get('hardDiskPresence'),
                                            'unknown'),
                'status': BMCSTATUS.get(row.get('hardDiskStatus'), 'unknown')
                }))

        return relmap
