#!/bin/sh
#####################################################
# Filename:        uninstall.sh
# Version:         v1.0
# Created:         09.21.2017
# Author:          y
# Description:     The OpenNMS plugin uninstaller
# History:			
# ---------------------------------------------------

opennmsdir=/opt/opennms

echo "Please enter the OpenNMS installation directory[/opt/opennms]:"
read ONMS_DIR

if [ "x${ONMS_DIR}" != "x" ] ; then
    opennmsdir=${ONMS_DIR}
fi

if [ ! -d "${opennmsdir}" ] ; then
    echo ""
    echo "Installed of OpenNMS does not exist or is not executable."
    echo ""
    exit 1;
fi

if [ ! -f "${opennmsdir}/etc/eventconf.xml" ] ; then
    echo ""
    echo "Installed of OpenNMS does not exist or is not executable."
    echo ""
    exit 1;
fi

rm -f $opennmsdir/etc/events/Huawei-Server-HMM.events.xml
rm -f $opennmsdir/etc/events/Huawei-Server-IBMC.events.xml

rm -f $opennmsdir/etc/datacollection/huawei-server-hmm.xml
rm -f $opennmsdir/etc/datacollection/huawei-server-ibmc.xml

rm -f $opennmsdir/etc/snmp-graph.properties.d/huawei-server-hmm-graph.properties
rm -f $opennmsdir/etc/snmp-graph.properties.d/huawei-server-ibmc-graph.properties

sed -i '/<event-file>events\/Huawei-Server-IBMC.events.xml<\/event-file>/d ; /<event-file>events\/Huawei-Server-HMM.events.xml<\/event-file>/d' $opennmsdir/etc/eventconf.xml

sed -i '/<include-collection dataCollectionGroup="HUAWEI-SERVER-IBMC-MIB"\/>/d ; /<include-collection dataCollectionGroup="HWSMM-MIB"\/>/d' $opennmsdir/etc/datacollection-config.xml

echo ""
echo "Uninstall the OpenNMS plugin successfully."
