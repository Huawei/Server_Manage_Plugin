'''
BMC HMM Router
'''
from Products.ZenUtils.Ext import DirectRouter, DirectResponse
from Products import Zuul


class bmcRouter(DirectRouter):
    '''
    BMC Router
    '''

    def _getFacade(self):
        '''
        getfacade
        '''

        # The parameter in the next line - myAppAdapter - must match with
        #   the name field in an adapter stanza in configure.zcml

        return Zuul.getFacade('BMCAdapter', self.context)

    # The method name - myRouterFunc - and its parameters - must match with
    #   the last part of the call for Zenoss.remote.myAppRouter.myRouterFunc
    #   in the javascript file myFooterMenu.js . The parameters will be
    #   populated by the items defined in the js file.

    # Note that the router function has 2 parameters, comments and rackSlot
    #  that are passed as the "opts" parameters from myFooterMenu.js.  The
    #  values of these fields were provided by the form input.

    def routerbs(self, deviceip, bootsequence, cfgboottype):
        '''
        routerBS
        '''
        facade = self._getFacade()

        # The object that is being operated on is in self.context

        devobject = self.context

        success, message = facade.bootsequence(
            devobject, deviceip, bootsequence, cfgboottype)

        if success:
            return DirectResponse.succeed(message)
        return DirectResponse.fail(message)

    def routerfpc(self, deviceip, frupowercontrol):
        '''
        routerFPC
        '''
        facade = self._getFacade()

        devobject = self.context

        frunum = 1
        success, message = facade.frupowerctrl(devobject, deviceip,
                                               frunum, frupowercontrol)

        if success:
            return DirectResponse.succeed(message)
        return DirectResponse.fail(message)


class hmmRouter(DirectRouter):
    '''
    HMM Router
    '''

    def _getFacade(self):
        '''
        getfacade
        '''
        return Zuul.getFacade('HMMAdapter', self.context)

    def routerbbo(self, deviceip, hmmbladenum,
                  hmmbiosbootoption, hmmbotype):
        '''
        routerBBO
        '''
        facade = self._getFacade()

        # The object that is being operated on is in self.context
        devobject = self.context

        success, message = facade.biosbootoption(devobject, deviceip, hmmbladenum,
                                                 hmmbiosbootoption,
                                                 hmmbotype)

        if success:
            return DirectResponse.succeed(message)

        return DirectResponse.fail(message)

    def routerfrucontrol(self, deviceip, hmmbladenum, hmmfrucontrol):
        '''
        routerFruControl
        '''
        hmmallblade = False
        facade = self._getFacade()

        devobject = self.context

        hmmfrunum = 1
        success, message = facade.frucontrol(devobject, deviceip, hmmbladenum,
                                             hmmfrunum, hmmfrucontrol,
                                             hmmallblade)

        if success:
            return DirectResponse.succeed(message)

        return DirectResponse.fail(message)
