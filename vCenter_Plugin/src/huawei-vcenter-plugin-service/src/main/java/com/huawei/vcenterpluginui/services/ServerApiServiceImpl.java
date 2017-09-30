package com.huawei.vcenterpluginui.services;

import javax.servlet.http.HttpSession;

import com.huawei.esight.api.rest.server.GetServerDeviceApi;
import com.huawei.esight.api.rest.server.GetServerDeviceDetailApi;
import com.huawei.vcenterpluginui.entity.ESight;
import com.huawei.vcenterpluginui.provider.SessionOpenIdProvider;

import java.sql.SQLException;

public class ServerApiServiceImpl extends ESightOpenApiService implements ServerApiService {

	@Override
	public String queryServer(String ip, HttpSession session, String servertype, int pageNo, int pageSize) throws SQLException {
		ESight eSight = getESightByIp(ip);
		String response = new GetServerDeviceApi<String>(eSight, new SessionOpenIdProvider(eSight, session)).doCall(servertype, String.valueOf(pageNo), String.valueOf(pageSize), String.class);
		return response;
	}

	@Override
	public String queryDeviceDetail(String ip, String dn, HttpSession session) throws SQLException {
		ESight eSight = getESightByIp(ip);
		String response = new GetServerDeviceDetailApi<String>(eSight, new SessionOpenIdProvider(eSight, session)).doCall(dn, String.class);
		return response;
	}

}
