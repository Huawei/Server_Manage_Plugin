'''
BMCFanMap
'''
from Products.DataCollector.plugins.CollectorPlugin import (
    SnmpPlugin, GetTableMap,
    )
from DeviceDefine import BMCSTATUS, BMCPRESENCE


class BMCFanMap(SnmpPlugin):
    '''
    BMCFanMap
    '''

    relname = 'bmcfans'
    modname = 'ZenPacks.community.HuaweiServer.BMCFan'

    snmpGetTableMaps = (
        GetTableMap(
            'fanTable', '1.3.6.1.4.1.2011.2.235.1.1.8.50.1', {
                '.1': 'fanIndex',
                '.2': 'fanSpeed',
                '.3': 'fanPresence',
                '.4': 'fanStatus',
                '.5': 'fanLocation',
                '.7': 'fanDevicename',
                }
            ),
        )

    def process(self, device, results, log):
        '''
        process oid
        '''

        log = log
        device = device

        temp_sensors = results[1].get('fanTable', {})

        relmap = self.relMap()
        for snmpindex, row in temp_sensors.items():
            name = 'Fan_' + str(row.get('fanIndex'))
            if not name:
                log.warn('Skipping temperature sensor with no name')
                continue
            if 2 != int(row.get('fanPresence')):
                continue
            relmap.append(self.objectMap({
                'id': self.prepId(name),
                'title': name,
                'snmpindex': snmpindex.strip('.'),
                'locator': row.get('fanLocation'),
                'status': BMCSTATUS.get(row.get('fanStatus'), 'unknown'),
                'speed': row.get('fanSpeed'),
                'presence': BMCPRESENCE.get(row.get('fanPresence'), 'unknown')
                }))

        return relmap
