#!/usr/bin/env python
# Copyright 2014-2015 VMware, Inc.  All rights reserved.

import sys
import os

def exitValidateScript(exitDescription, exitCode=0):
   print('Exiting-{0}, exit code: {1}'.format(exitDescription, str(exitCode)))
   sys.exit(exitCode)

print('Entering validate a simple Pak file')
print('Entering check if VCOPS_BASE is set')
try:
   os.environ['VCOPS_BASE']
except KeyError, e:
   exitValidateScript('Failed-VCOPS_BASE check failed', 1)
print('VCOPS_BASE check passed')
print('Exiting validate a simple Pak file')
sys.exit()
