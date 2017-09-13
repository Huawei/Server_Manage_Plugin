package com.huawei.vcenterpluginui.services;

import javax.servlet.http.HttpSession;
import java.sql.SQLException;

/**
 * It must be declared as osgi:service with the same name in
 * main/resources/META-INF/spring/bundle-context-osgi.xml
 */
public interface ServerApiService {

	/**
	 * 查询服务器列表
	 * @param ip
	 * @param session
	 * @param servertype
	 * @param pageNo
	 * @param pageSize
	 * @return
	 * @throws Exception
	 */
	String queryServer(String ip, HttpSession session, String servertype, int pageNo, int pageSize) throws SQLException;

	/**
	 * 查询服务器详细信息
	 * @param ip
	 * @param dn
	 * @param session
	 * @return
	 * @throws Exception
	 */
	String queryDeviceDetail(String ip, String dn, HttpSession session) throws SQLException;

}
