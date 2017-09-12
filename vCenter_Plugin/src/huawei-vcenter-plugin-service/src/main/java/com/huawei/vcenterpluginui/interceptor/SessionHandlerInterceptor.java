package com.huawei.vcenterpluginui.interceptor;

import com.huawei.vcenterpluginui.services.SessionService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.web.servlet.ModelAndView;
import org.springframework.web.servlet.handler.HandlerInterceptorAdapter;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import java.io.IOException;

public class SessionHandlerInterceptor extends HandlerInterceptorAdapter {
	
	protected SessionService sessionService;
	
	@Autowired
    public SessionHandlerInterceptor(@Qualifier("sessionService") SessionService sessionService) {
		this.sessionService = sessionService;
    }

    // Empty Interceptor to avoid compiler warnings in huawei-vcenter-plugin-ui's
    // bundle-context.xml
    // where the bean is declared
    public SessionHandlerInterceptor() {
    	sessionService = null;
    }

	@Override
	public boolean preHandle(HttpServletRequest request, HttpServletResponse response, Object handler) throws IOException {
		if (sessionService.getUserSession() == null) {
			response.getWriter().write("{\"code\":\"-90003\",\"data\":null,\"description\":\"Auth failed\"}");
			return false;
		}
		
		return true;
	}

	@Override
	public void postHandle(HttpServletRequest request, HttpServletResponse response, Object handler, ModelAndView modelAndView) {
	}

	@Override
	public void afterCompletion(HttpServletRequest request, HttpServletResponse response, Object handler, Exception ex) {
	}

	public SessionService getSessionService() {
		return sessionService;
	}

	public void setSessionService(SessionService sessionService) {
		this.sessionService = sessionService;
	}
}