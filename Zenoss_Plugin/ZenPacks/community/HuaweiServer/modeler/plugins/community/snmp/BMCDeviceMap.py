'''
BMCDeviceMap
'''
from Products.DataCollector.plugins.CollectorPlugin import (
    SnmpPlugin, GetTableMap, GetMap
    )
from DeviceDefine import BMCBOOTSTR, BMCFPCSTR


class BMCDeviceMap(SnmpPlugin):
    '''
    BMCDeviceMap
    '''

    snmpGetTableMap = (
        GetTableMap(
            'fruBoardTable', '1.3.6.1.4.1.2011.2.235.1.1.19.50.1', {
                '.1': 'fruId',
                '.4': 'fruBoardSerialNumber'
            }
        )
    )
    snmpGetMap = GetMap({
        '.1.3.6.1.4.1.2011.2.235.1.1.19.50.1.4.1': 'fruBoardSerialNumber',
        '.1.3.6.1.4.1.2011.2.235.1.1.1.6.0': 'deviceName',
        '.1.3.6.1.4.1.2011.2.235.1.1.1.9.0': 'hostName',
        '.1.3.6.1.4.1.2011.2.235.1.1.1.2.0': 'systemBootsequence',
        '.1.3.6.1.4.1.2011.2.235.1.1.1.12.0': 'fruPowerControl',
        '.1.3.6.1.4.1.2011.2.235.1.1.10.50.1.4.9.77.97.105.110.98.111.97.114.100': 'boardId',
        })

    def process(self, device, results, log):
        '''
        process oid
        '''

        log = log
        device = device
        getdata = results[0]

        return self.objectMap({
            'bmcDeviceName': getdata.get('deviceName'),
            'bmcHostName': getdata.get('hostName'),
            'bmcBoardId': getdata.get('boardId'),
            'bmcBootSequence': BMCBOOTSTR.get(
                getdata.get('systemBootsequence'), 'Timeout'),
            'bmcFRUControl': BMCFPCSTR.get(
                getdata.get('fruPowerControl')),
            'setHWSerialNumber': getdata.get('fruBoardSerialNumber'),
            })
