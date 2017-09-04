import os
import sys
import time
import random
import string

# BMC 112 94
# HMM 101 82

sensorname = string.join(random.sample(['z','y','x','w','v','u','t','s','r','q','p','o','n','m','l','k','j','i','h','g','f','e','d','c','b','a'], 6)).replace(' ','')
print sensorname
filename = 'mibtraps.txt'
idx = 0
idxall = 0
input = None
inputname = None
jump = None
if len(sys.argv) > 3:
    inputname = sys.argv[1]
    input = sys.argv[2]
    jump = sys.argv[3]
    if '-' not in str(inputname):
        sensorname = inputname
        print sensorname
elif len(sys.argv) > 2:
    inputname = sys.argv[1]
    input = sys.argv[2]
    sensorname = inputname
    print sensorname
with open(filename, 'r') as file_to_read:
    while True:
        lines = file_to_read.readline()
        if not lines:
            break

        idxall = idxall + 1
        componentname = sensorname + str(idxall).zfill(3)
        obj, desc, severity, oid, = [str(i) for i in lines.split('\t')]
        if jump is not None:
            print inputname, input, jump
            if str(jump) == str(1):
                if 'Deassert' in obj:
                    continue
            else:
                if 'Deassert' not in obj:
                    continue
                else:
                    componentname = sensorname + str(idxall-1).zfill(3)

        if input is not None:
            if str(input) != str(idx) and '-' not in str(input):
                continue
        idx = idx + 1

        snmpstr = "snmptrap -v 2c -c public 127.0.0.1 123 "+oid.strip('\r\n')+" 1.3.6.1.4.1.2011.2.82.1.82.500.1.1 i 3 1.3.6.1.4.1.2011.2.235.1.1.500.1.2 s "+componentname+" 1.3.6.1.4.1.2011.2.235.1.1.500.1.3 s "+oid.strip('\r\n')+" 1.3.6.1.4.1.2011.2.235.1.1.500.1.4 i 1 1.3.6.1.4.1.2011.2.235.1.1.500.1.5 s 0x00000001 1.3.6.1.4.1.2011.2.235.1.1.500.1.6 i 0 1.3.6.1.4.1.2011.2.235.1.1.500.1.7 i 0 1.3.6.1.4.1.2011.2.235.1.1.500.1.8 s test 1.3.6.1.4.1.2011.2.235.1.1.500.1.9 s room 1.3.6.1.4.1.2011.2.235.1.1.500.1.10 s 20170427"

        time.sleep(0.3)
        print str(idx) + ":" + str(idxall) + ":"+snmpstr
        os.system(snmpstr)
