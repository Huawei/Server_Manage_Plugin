#!/bin/sh
#####################################################
# Filename:        install.sh
# Version:         v1.0
# Created:         09.21.2017
# Author:          y
# Description:     The OpenNMS plugin installer
# History:			
# ---------------------------------------------------

if [ "$USER" != "root" -o "$UID" != "0" -o "$EUID" != "0" ]; then
    echo ""
    echo "Execution of OpenNMS plugin must be as the root user."
    echo "Please log in as root user and re-run $0"
    echo ""
    exit 1;
fi

opennmsdir=/opt/opennms

BASEDIR=$(dirname $0)

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


rsync -rc $BASEDIR/data/ $opennmsdir/

sed -i '/<event-file>events\/Huawei-Server-IBMC.events.xml<\/event-file>/d ; /<event-file>events\/Huawei-Server-HMM.events.xml<\/event-file>/d' $opennmsdir/etc/eventconf.xml

sed -i '/<event-file>events\/Translator.default.events.xml<\/event-file>/a\<event-file>events\/Huawei-Server-HMM.events.xml<\/event-file>' $opennmsdir/etc/eventconf.xml
sed -i '/<event-file>events\/Translator.default.events.xml<\/event-file>/a\<event-file>events\/Huawei-Server-IBMC.events.xml<\/event-file>' $opennmsdir/etc/eventconf.xml


sed -i '/<include-collection dataCollectionGroup="HUAWEI-SERVER-IBMC-MIB"\/>/d ; /<include-collection dataCollectionGroup="HWSMM-MIB"\/>/d' $opennmsdir/etc/datacollection-config.xml

sed -i '/<include-collection dataCollectionGroup="MIB2"\/>/a\<include-collection dataCollectionGroup="HUAWEI-SERVER-IBMC-MIB"\/>' $opennmsdir/etc/datacollection-config.xml
sed -i '/<include-collection dataCollectionGroup="MIB2"\/>/a\<include-collection dataCollectionGroup="HWSMM-MIB"\/>' $opennmsdir/etc/datacollection-config.xml

echo ""
echo "The OpenNMS plugin was installed successfully."
