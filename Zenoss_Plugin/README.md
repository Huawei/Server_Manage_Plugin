ZenPacks.community.HuaweiServer
=========================



snmpwalkv3 command

snmpwalk -${device/zSnmpVer} -u ${device/zSnmpSecurityName} -l authPriv -a ${device/zSnmpAuthType} -A ${device/zSnmpAuthPassword} -x ${device/zSnmpPrivType} -X ${device/zSnmpPrivPassword} ${device/snmpwalkPrefix}${here/manageIp} system

snmptrap -v 2c -c public 127.0.0.1 123 1.3.6.1.4.1.2011.2.235.1.1.500.10.7.17 1.3.6.1.4.1.2011.2.82.1.82.500.1.1 i 3 1.3.6.1.4.1.2011.2.235.1.1.500.1.2 s testsensor 1.3.6.1.4.1.2011.2.235.1.1.500.1.3 s testevents 1.3.6.1.4.1.2011.2.235.1.1.500.1.4 i 1 1.3.6.1.4.1.2011.2.235.1.1.500.1.5 s 0x00000001 1.3.6.1.4.1.2011.2.235.1.1.500.1.6 i 0 1.3.6.1.4.1.2011.2.235.1.1.500.1.7 i 0 1.3.6.1.4.1.2011.2.235.1.1.500.1.8 s test 1.3.6.1.4.1.2011.2.235.1.1.500.1.9 s room 1.3.6.1.4.1.2011.2.235.1.1.500.1.10 s 20170427

snmptrap -v 2c -c public 10.0.0.7 "" 1.3.6.1.4.1.2011.2.235.1.1.500.10.7.129
hwCPUOffline
1.3.6.1.4.1.2011.2.235.1.1.500.10.7.129
hwCPUOfflineDeassert
1.3.6.1.4.1.2011.2.235.1.1.500.10.7.130

Event级别对应表
'0': SEVERITY_CLEAR, 
'1': SEVERITY_DEBUG,
'2': SEVERITY_INFO,		OK
'3': SEVERITY_WARNING,	Minor
'4': SEVERITY_ERROR,	Major
'5': SEVERITY_CRITICAL,	Critical

snmpset
bmc
systemBootsequence
1.3.6.1.4.1.2011.2.235.1.1.1.2.0
fruPowerControl
1.3.6.1.4.1.2011.2.235.1.1.7.50.1.2.1
hmm 
b1BiosBootOption
1.3.6.1.4.1.2011.2.82.1.82.4.1.32.0
b1FRUControl
1.3.6.1.4.1.2011.2.82.1.82.4.1.2002.1.9.1
b32BiosBootOption
1.3.6.1.4.1.2011.2.82.1.82.4.32.32.0
b32FRUControl
1.3.6.1.4.1.2011.2.82.1.82.4.32.2002.1.9.1

autodiscover
deviceClassMap = {
'.1.3.6.1.4.1.2011.1.253':'/Server/Huawei/BMC',
'.1.3.6.1.4.1.2011.1.82':'/Server/Huawei/HMM'
}

discovered = dmd.Devices.Discovered

for device in discovered.getSubDevices():
try:
deviceClass = deviceClassMap[device.getHWProductKey()]
discovered.moveDevices(deviceClass, device.id)
except KeyError:
pass

commit()
