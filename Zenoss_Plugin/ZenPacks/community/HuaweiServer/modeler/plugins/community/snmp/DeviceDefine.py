'''
SNMP Device Defines
'''

BMCSTATUS = {
    1: 'ok',
    2: 'minor',
    3: 'major',
    4: 'critical',
    5: 'absence',
    6: 'unknown',
}

BMCPRESENCE = {
    1: 'absence',
    2: 'presence',
    3: 'unknown',
}

BMCPCESTATUS = {
    1: 'disable',
    2: 'enable',
}

BMCPCFA = {
    1: 'eventlog(1)',
    2: 'eventlogAndPowerOff(2)',
}

BMCBOOTSTR = {
    1: 'No override',
    2: 'PXE',
    3: 'Hard Drive',
    4: 'DVD ROM',
    5: 'FDD Removable Device',
    6: 'Unspecified',
    7: 'Bios Setup',
}

BMCBOOTSEQUENCE = {
    1: 'noOverride(1)',
    2: 'pxe(2)',
    3: 'hdd(3)',
    4: 'cdDvd(4)',
    5: 'floppyPrimaryRemovableMedia(5)',
    6: 'unspecified(6)',
    7: 'biossetup(7)',
}

BMCFPCSTR = {
    1: 'Power Off',
    2: 'Power On',
    3: 'Forced System Reset',
    4: 'Forced Power Cycle',
    5: 'Forced Power Off',
}

BMCFRUPOWERCONTROL = {
    1: 'normalPowerOff(1)',
    2: 'powerOn(2)',
    3: 'forcedSystemReset(3)',
    4: 'forcedPowerCycle(4)',
}

HMMPRESENCE = {
    0: 'not present',
    1: 'present',
    2: 'indeterminate',
}

HMMPCE = {
    0: 'disable',
    1: 'enable',
}

HMMHEALTH = {
    0: 'ok',
    1: 'minor',
    2: 'major',
    3: 'majorandminor',
    4: 'critical',
    5: 'criticalandminor',
    6: 'criticalandmajor',
    7: 'criticalandmajorandminor',
}

HMMCPUHEALTH = {
    1: 'normal',
    2: 'minor',
    3: 'major',
    4: 'critical',
    5: 'unknown',
}

HMMPOWERMODE = {
    '0': 'AC',
    '1': 'DC',
    '2': 'AC and DC',
    '3': 'HVDC',
    '4': 'unknown',
}

HMMBIOSBOOTOPTION = {
    0: 'No override',
    1: 'Force PXE',
    2: 'Force boot from default Hard-drive[2]',
    3: 'Force boot from default Hard-drive, request Safe Mode[2]',
    4: 'Force boot from default Diagnostic Partition[2]',
    5: 'Force boot from default CD/DVD[2]',
    6: 'Force boot into BIOS Setup',
    15: 'Force boot from Floppy/primary removable media',
}

HMMFRUCONTROL = {
    '0': 'Force System Reset',
    '1': 'power off',
    '2': 'Force Power Cycle',
    '3': 'NMI',
    '4': 'power on',
}

HMMLOCATION = {
    1: 'PSU1',
    2: 'PSU2',
    3: 'PSU3',
    4: 'PSU4',
    5: 'PSU5',
    6: 'PSU6',
}

HMMSTATUS = {
    1: 'normal',
    2: 'minor',
    3: 'major',
    4: 'critical',
}

HMMNEWSTATUS = {
    0: 'reset',
    2: 'power off then on',
    3: 'interrupt control',
}
