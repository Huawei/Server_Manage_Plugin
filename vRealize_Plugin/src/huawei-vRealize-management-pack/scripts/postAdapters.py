#!/usr/bin/env python
# Copyright 2014-2015 VMware, Inc.  All rights reserved.

##### example of running shell scripts #####
#import os
#import sys
#if sys.platform.startswith('linux') == True:
#	print("This is Linux")
#	cmd = "bash ./post-install.sh"
#elif sys.platform.startswith('win') == True:
#	print("This is Windows")
#	cmd = ".\post-install.bat"
#os.system(cmd)
#
##### example to import dashboards #####
#
#cmd = "pushd $VCOPS_BASE/tools/dbcli"
#cmd = cmd + ";" + "bash ./dbcli.sh dashboard import admin $VCOPS_BASE/user/plugins/inbound/my_adapter3/conf/dashboards/Sample-Dashboard.xml"
#cmd = cmd + ";" + "popd"
#os.system(cmd)
########################################

print("In post")
