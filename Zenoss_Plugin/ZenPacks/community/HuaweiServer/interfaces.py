'''
BMC HMM IFacade
'''
from Products.Zuul.interfaces import IFacade

# ZuulMessageFactory is the translation layer. You will see strings intended to
# been seen in the web interface wrapped in _t(). This is so that these strings
# can be automatically translated to other languages.
from Products.Zuul.utils import ZuulMessageFactory as _t


class IBMCFacade(IFacade):
    '''
    IBMCFacade
    '''

    def bootsequence(self, devobj, deviceip, bootsequence, boottype):
        """ Modify deviceip / bootsequence boottype for a device object"""

    def frupowerctrl(self, devobj, deviceip, frunum, frupowercontrol):
        """ Modify deviceip / frunum frupowercontrol for a device object"""


class IHMMFacade(IFacade):
    '''
    IHMMFacade
    '''

    def biosbootoption(self, devobj, deviceip, hmmbladenum, hmmboo, hmmbotype):
        """ Modify deviceip / hmmbladenum attributes for a device object"""

    def frucontrol(self, devobj, deviceip, hmmbladenum, hmmfrunum,
                   hmmfrucontrol, hmmallblade):
        """ Modify deviceip / hmmbladenum attributes for a device object"""
