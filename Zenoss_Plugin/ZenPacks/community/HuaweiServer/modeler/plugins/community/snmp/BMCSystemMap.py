'''
BMCSystemMap
'''
from Products.DataCollector.plugins.CollectorPlugin import (
    SnmpPlugin, GetTableMap,
    )


class BMCSystemMap(SnmpPlugin):
    '''
    BMCSystemMap
    '''

    relname = 'bmcsystems'
    modname = 'ZenPacks.community.HuaweiServer.BMCSystem'

    snmpGetTableMaps = (
        GetTableMap(
            'firmwareTable', '1.3.6.1.4.1.2011.2.235.1.1.11.50.1', {
                '.1': 'fwIndex',
                '.4': 'fwVersion',
                '.7': 'fwBoard',
                }
            ),
        )

    def process(self, device, results, log):
        '''
        process oid
        '''

        log = log
        device = device
        temp_sensors = results[1].get('firmwareTable', {})

        relmap = self.relMap()
        for snmpindex, row in temp_sensors.items():
            name = str(row.get('fwIndex'))
            if not name:
                log.warn('Skipping firmware with no name')
                continue

            relmap.append(self.objectMap({
                'id': self.prepId(name),
                'fwVersion': row.get('fwVersion'),
                'fwBoard': row.get('fwBoard'),
                }))

        return relmap
