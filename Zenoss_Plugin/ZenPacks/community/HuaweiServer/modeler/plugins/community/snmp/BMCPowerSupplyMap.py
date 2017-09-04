'''
BMCPowerSupplyMap
'''
from Products.DataCollector.plugins.CollectorPlugin import (
    SnmpPlugin, GetTableMap,
    )
from DeviceDefine import BMCSTATUS, BMCPRESENCE


class BMCPowerSupplyMap(SnmpPlugin):
    '''
    BMCPowerSupplyMap
    '''

    relname = 'bmcpowerSupplys'
    modname = 'ZenPacks.community.HuaweiServer.BMCPowerSupply'

    snmpGetTableMaps = (
        GetTableMap(
            'powerSupplyTable', '1.3.6.1.4.1.2011.2.235.1.1.6.50.1', {
                '.1': 'powerSupplyIndex',
                '.2': 'powerSupplymanufacture',
                '.4': 'powerSupplyModel',
                '.6': 'powerSupplyPowerRating',
                '.7': 'powerSupplyStatus',
                '.8': 'powerSupplyInputPower',
                '.9': 'powerSupplyPresence',
                '.11': 'powerSupplyLocation',
                '.13': 'powerSupplyDevicename',
                }
            ),
        )

    def process(self, device, results, log):
        '''
        process oid
        '''

        log = log
        device = device
        temp_sensors = results[1].get('powerSupplyTable', {})

        relmap = self.relMap()
        for snmpindex, row in temp_sensors.items():
            name = 'PS_' + str(row.get('powerSupplyIndex'))
            if not name:
                log.warn('Skipping temperature sensor with no name')
                continue
            if 2 != int(row.get('powerSupplyPresence')):
                continue

            relmap.append(self.objectMap({
                'id': self.prepId(name),
                'title': name,
                'snmpindex': snmpindex.strip('.'),
                'locator': row.get('powerSupplyLocation'),
                'presence': BMCPRESENCE.get(row.get('powerSupplyPresence'),
                                            'unknown'),
                'status': BMCSTATUS.get(row.get('powerSupplyStatus'),
                                        'unknown'),
                'manufacturer': row.get('powerSupplymanufacture'),
                'model': row.get('powerSupplyModel'),
                'powerRating': str(row.get('powerSupplyPowerRating')) + '(Watts)',
                'inputPower': str(row.get('powerSupplyInputPower'))+'(Watts)',
                }))

        return relmap
