'''
HMMDeviceMap
'''
from Products.DataCollector.plugins.CollectorPlugin import (
    SnmpPlugin, GetMap
    )


class HMMDeviceMap(SnmpPlugin):
    '''
    HMMDeviceMap
    '''

    snmpGetMap = GetMap({
        '.1.3.6.1.4.1.2011.2.82.1.82.2.2001.1.15.0': 'shelfSerialNumber',
        })

    def process(self, device, results, log):
        '''
        process oid
        '''

        log = log
        device = device
        getdata = results[0]

        return self.objectMap({
            'setHWSerialNumber': getdata.get('shelfSerialNumber'),
            })
