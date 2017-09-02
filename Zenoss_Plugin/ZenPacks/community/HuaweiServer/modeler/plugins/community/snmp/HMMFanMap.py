'''
HMMFanMap
'''
from Products.DataCollector.plugins.CollectorPlugin import (
    SnmpPlugin, GetTableMap,
    )
from DeviceDefine import HMMSTATUS, HMMPRESENCE


class HMMFanMap(SnmpPlugin):
    '''
    HMMFanMap
    '''

    relname = 'hmmfans'
    modname = 'ZenPacks.community.HuaweiServer.HMMFan'

    snmpGetTableMaps = (
        GetTableMap(
            'hmmFanTable', '1.3.6.1.4.1.2011.2.82.1.82.5.2001.1', {
                '.1': 'fanIndex',
                '.2': 'fanPresence',
                '.3': 'fanSpeed',
                '.7': 'fanStatus',
                }
            ),
        )

    def process(self, device, results, log):
        '''
        process oid
        '''

        log = log
        device = device
        temp_sensors = results[1].get('hmmFanTable', {})

        relmap = self.relMap()
        for snmpindex, row in temp_sensors.items():
            name = 'Fan_'+str(row.get('fanIndex'))
            if not name:
                log.warn('Skipping temperature sensor with no name')
                continue
            if 1 != int(row.get('fanPresence')):
                continue

            relmap.append(self.objectMap({
                'id': self.prepId(name),
                'title': name,
                'snmpindex': snmpindex.strip('.'),
                'hfpresence': HMMPRESENCE.get(row.get('fanPresence'),
                                              'unknown'),
                'hfspeed': row.get('fanSpeed'),
                'hfstatus': HMMSTATUS.get(row.get('fanStatus'), 'unknown'),
                }))

        return relmap
