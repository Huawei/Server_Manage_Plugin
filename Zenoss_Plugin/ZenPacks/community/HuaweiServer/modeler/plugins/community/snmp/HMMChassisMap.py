'''
HMMChassisMap
'''
from Products.DataCollector.plugins.CollectorPlugin import (
    SnmpPlugin, GetMap
    )
from DeviceDefine import HMMHEALTH, HMMPCE


class HMMChassisMap(SnmpPlugin):
    '''
    HMMChassisMap
    '''

    relname = 'hmmchassiss'
    modname = 'ZenPacks.community.HuaweiServer.HMMChassis'
    snmpGetMap = GetMap({
        '.1.3.6.1.4.1.2011.2.82.1.82.2.4.0': 'shelfLocation',
        '.1.3.6.1.4.1.2011.2.82.1.82.2.5.0': 'shelfHealth',
        '.1.3.6.1.4.1.2011.2.82.1.82.2.8.0': 'shelfChassisID',
        '.1.3.6.1.4.1.2011.2.82.1.82.2.13.0': 'shelfRealTimePower',
        '.1.3.6.1.4.1.2011.2.82.1.82.2.14.0': 'shelfPowerCappingEnable',
        '.1.3.6.1.4.1.2011.2.82.1.82.2.15.0': 'shelfPowerCapping',
        '.1.3.6.1.4.1.2011.2.82.1.82.2.18.0': 'shelfChassisName',
        '.1.3.6.1.4.1.2011.2.82.1.82.2.2001.1.16.0': 'shelfType',
        })

    def process(self, device, results, log):
        '''
        process oid
        '''

        log = log
        device = device
        getdata = results[0]

        relmap = self.relMap()
        relmap.append(self.objectMap({
            'id': self.prepId('Chassis_1'),
            'title': 'Chassis_1',
            'hclocation': getdata.get('shelfLocation'),
            'hcchassisID': getdata.get('shelfChassisID'),
            'hchealth': HMMHEALTH.get(getdata.get('shelfHealth'), 'unknown'),
            'hcrealtimePower': str(getdata.get('shelfRealTimePower')) + '(Watts)',
            'hcpowerCappingEnable':
                HMMPCE.get(getdata.get('shelfPowerCappingEnable'), 'unknown'),
            'hcpowerCappingValue': getdata.get('shelfPowerCapping'),
            'hctype': getdata.get('shelfType'),
            }))
        return relmap
