package com.huawei.vcenterpluginui.services;

import com.vmware.vise.usersession.UserSession;
import com.vmware.vise.usersession.UserSessionService;

public class SessionServiceImpl implements SessionService {
    
    protected UserSessionService userSessionService;

	@Override
	public UserSession getUserSession() {
		return userSessionService.getUserSession();
	}


	public void setUserSessionService(UserSessionService userSessionService) {
		this.userSessionService = userSessionService;
	}
}
