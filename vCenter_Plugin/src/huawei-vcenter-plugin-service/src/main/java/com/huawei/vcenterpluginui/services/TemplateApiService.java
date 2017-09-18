package com.huawei.vcenterpluginui.services;

import java.io.IOException;
import java.sql.SQLException;
import java.util.List;
import java.util.Map;

import javax.servlet.http.HttpSession;

/**
 * It must be declared as osgi:service with the same name in
 * main/resources/META-INF/spring/bundle-context-osgi.xml
 */
public interface TemplateApiService {
	/**
	 * 创建模版
	 * @param data  提交模版数据{
								"esights": [
								"127.0.0.1",
								"192.168.1.1"
								],
								"data": {
									"templateName": "模板名称",
									"templateType": "模版类型",
									"templateDesc": "this is a template",
									"templateProp": {
									"模版属性": "模版属性值"
									}
								}
							  }
	 * @param session
	 * @return
	 * @throws Exception
	 */
	List<Map<String,Object>> templatePost(String data, HttpSession session) throws IOException;
	
	/**
	 * 获取模版列表
	 * @param ip
	 * @param session
	 * @param templateType
	 * @param pageNo
	 * @param pageSize
	 * @return
	 */
	String list(String ip, HttpSession session, String templateType, String pageNo, String pageSize) throws SQLException;
	
	/**
	 * 获取模版详情
	 * @param ip
	 * @param templateName
	 * @param session
	 * @return
	 */
	String getDetail(String ip,String templateName,HttpSession session) throws SQLException;
	
	/**
	 * 删除模版
	 * @param ip
	 * @param templateName
	 * @param session
	 * @return
	 */
	String delete(String ip,String templateName,HttpSession session) throws SQLException;
	
	//===========模版任务==============
	/**
     * 模版任务列表
     *
     * @param esightIp
     * @param taskName
     * @param taskStatus
     * @param pageNo
     * @param pageSize
     * @param order
     * @param orderDesc
     * @param session
     * @return
     */
	Map<String,Object> templateTaskList(String esightIp, String taskName, String taskStatus, int pageNo,int pageSize, String order, String orderDesc, HttpSession session) throws SQLException;
	/**
     * 模版添加部署任务
     *
     * @param data    // var postParam = {
			          //     "esight": "127.0.0.1",
			          //     "param": {
			          //         "deployTaskName": "test1",
			          //         "dn": "NE=123;NE=1234",
			          //         "templates": "template1,poweron1"
			          //     }
			          // };
     * @param session
     * @return
     */
	String postTemplateTask(String data, HttpSession session) throws IOException, SQLException;
	/**
	 * 删除模版任务
	 * @param taskId
	 * @return
	 */
	int deleteTemplateTask(int taskId) throws SQLException;
	
	
}
