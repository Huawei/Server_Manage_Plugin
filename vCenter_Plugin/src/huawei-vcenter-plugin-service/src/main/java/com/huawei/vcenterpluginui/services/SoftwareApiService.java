package com.huawei.vcenterpluginui.services;

import java.io.IOException;
import java.sql.SQLException;
import java.util.List;
import java.util.Map;

import javax.servlet.http.HttpSession;
import com.huawei.vcenterpluginui.entity.Task;

/**
 * It must be declared as osgi:service with the same name in
 * main/resources/META-INF/spring/bundle-context-osgi.xml
 */
public interface SoftwareApiService {
	/**
	 * 上传软件源
	 * @param data   {
			        //     "esights": [
			        //         "192.168.1.1", "192.168.1.4", "192.168.1.4"
			        //     ], //这里是一个数组
			        //     "data": {
			        //         "softwareName": "OS_Software1",
			        //         "softwareDescription": "..This is a OS template.",
			        //         "softwareType": "Windows",
			        //         "softwareVersion": "Windows Server 2008 R2 x64",
			        //         "softwareEdition": null,
			        //         "softwareLanguage": "Chinese",
			        //         "sourceName": "7601.17514.101119 - 1850_x64fre_server_eval_en - us - GRMSXEVAL_EN_DVD.iso",
			        //         "sftpserverIP": "188.10.18.188",
			        //         "username": "itSftpUser"
			        //     }
			        // }
	 * @param session
	 * @return
	 */
	List<Map<String,Object>> upload(String data, HttpSession session) throws IOException, SQLException;
	/**
	 * 上传软件源任务进度列表
	 * @param session
	 * @return
	 */
	List<Task> progressList(HttpSession session) throws SQLException;
	/**
	 * 软件源列表（上传成功）
	 * @param esightIp
	 * @param session
	 * @return
	 */
	String list(String esightIp,int pageNo, int pageSize, HttpSession session) throws SQLException;
	/**
	 * 删除失败任务（上传软件源）
	 * @return
	 */
	int deleteUploadFailed() throws SQLException;
	/**
	 * 删除软件源
	 * @param softwareName
	 * @return
	 */
	String deleteSoftResource(String softwareName,String ip, HttpSession session) throws SQLException;
}
