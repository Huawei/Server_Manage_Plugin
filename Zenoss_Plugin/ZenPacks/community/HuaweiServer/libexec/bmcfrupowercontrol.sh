#!/bin/bash
###########################################################################
# Author:	kingkong
# Date:		May 3th 2017
# Modified:
#
# use example : 
# snmpset -v2c -c public 192.168.4.85 1.3.6.1.4.1.2011.2.235.1.1.1.19.0 i 1
# v2c :snmpset $2 $3 $1 1.3.6.1.4.1.2011.2.235.1.1.1.19.0 i 1
# v3  :snmpset $2 $4 -l authPriv $5 $6 $7 $8 $1 1.3.6.1.4.1.2011.2.235.1.1.1.19.0 i 1
# 
# $1 = ip 
# $2 = zSnmpVer
# $3 = zSnmpCommunity
# $4 = zSnmpSecurityName
# $5 = zSnmpAuthType
# $6 = zSnmpAuthPassword
# $7 = zSnmpPrivType
# $8 = zSnmpPrivPassword
# $9 = intvalue
###########################################################################
#set -x


echo "$1" "$2" "$3" "$4" "$5" "$6" "$7" "$8" "$9"
if [ "$2" = "-v2c" ]; then	snmpset $2 $3 $1 1.3.6.1.4.1.2011.2.235.1.1.1.12.0 i $9
elif [ "$2" = "-v3" ]; then	snmpset $2 $4 -l authPriv $5 $6 $7 $8 $1 1.3.6.1.4.1.2011.2.235.1.1.1.12.0 i $9
fi