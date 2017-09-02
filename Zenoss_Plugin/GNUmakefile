###########################################################################
#
# This program is part of Zenoss Core, an open source monitoring platform.
# Copyright (C) 2014 Zenoss Inc.
#
# This program is free software; you can redistribute it and/or modify it
# under the terms of the GNU General Public License version 2 or (at your
# option) any later version as published by the Free Software Foundation.
#
# For complete information please visit: http://www.zenoss.com/oss/
#
###########################################################################


test:
	runtests ZenPacks.community.HuaweiServer
	./check_pep8.sh
reinstall:
	zenpack --remove ZenPacks.community.HuaweiServer
	zenpack --link --install .

pretty_xml:
	python -c "from butils.pprint import format_xml; format_xml('objects/objects.xml')"
