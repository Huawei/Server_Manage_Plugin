package com.huawei.vcenterpluginui.mvc;

import java.io.IOException;
import java.sql.SQLException;
import java.util.Map;

import org.junit.Assert;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.mockito.Mockito;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PowerMockIgnore;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;
import org.powermock.modules.junit4.PowerMockRunnerDelegate;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.mock.web.MockHttpSession;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.huawei.esight.utils.HttpRequestUtil;
import com.huawei.esight.utils.JsonUtil;
import com.huawei.vcenterpluginui.ContextSupported;
import com.huawei.vcenterpluginui.entity.ResponseBodyBean;

@RunWith(PowerMockRunner.class)
@PowerMockRunnerDelegate(SpringJUnit4ClassRunner.class)
@PrepareForTest({HttpRequestUtil.class})
@PowerMockIgnore({"sun.security.*", "javax.net.*","javax.crypto.*"})
public class SoftwareResourceControllerTest extends ContextSupported {
	MockHttpSession mockHttpSession = new MockHttpSession();
	
	@Autowired
	private SoftwareResourceController softwareResourceController;

	@Before
	public void setUp() {
		mockHttpSession.setAttribute("openId_192.168.11.35", "bfec0163-dd56-473e-b11f-6d5845e1b684");
	}

	@Test
	public void getProgressList() throws IOException, SQLException {
		//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
//        String responseBody = "{\n" +
//				"  \"code\": 0,\n" +
//				"  \"data\": {\n" +
//				"    \"taskName\": \"API@Task_1456209500919\",\n" +
//				"    \"softwaresourceName\": \"OS_Software1\",\n" +
//				"    \"taskStatus\": \"Complete\",\n" +
//				"    \"taskProgress\": \"100\",\n" +
//				"    \"taskResult\": \"Success\",\n" +
//				"    \"taskCode\": 0,\n" +
//				"    \"errorDetail\": \"软件源上传失败，传输中断\"\n" +
//				"  },\n" +
//				"  \"description\": \"Gettaskdetailsuccess.\"\n" +
//				"}";

		String responseBody = "{\"code\":0,\"data\":{\"taskName\":\"API@UploadSoftwareTask_1498102439676\",\"taskStatus\":\"Running\",\"softwareName\":\"1213124235\",\"taskCode\":0," +
				"\"taskProgress\":0},\"description\":\"Get upload SoftWare Progress success.\"}";
        Map<String,Object> map = JsonUtil.readAsMap(responseBody);
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(map, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
        
        //request data
		ResponseBodyBean responseBodyBean = softwareResourceController.progressList(null, mockHttpSession);
		Assert.assertEquals("0", responseBodyBean.getCode());
	}
	
	@Test
	public void getList() throws IOException, SQLException {
		//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\n" +
				"  \"code\": 0,\n" +
				"  \"totalNum\": 3,\n" +
				"  \"data\": [\n" +
				"    {\n" +
				"      \"softwareName\": \"OS_Softwaresource1\",\n" +
				"      \"softwareDescription\": \"ThisisSoftwaresourcetemplate1\",\n" +
				"      \"softwareType\": \"Windows\"\n" +
				"    },\n" +
				"    {\n" +
				"      \"softwareName\": \"OS_Softwaresource2\",\n" +
				"      \"softwareDescription\": \"ThisisSoftwaresourcetemplate1\",\n" +
				"      \"softwareType\": \"Windows\"\n" +
				"    },\n" +
				"    {\n" +
				"      \"softwareName\": \"OS_Softwaresource3\",\n" +
				"      \"softwareDescription\": \"ThisisSoftwaresourcetemplate1\",\n" +
				"      \"softwareType\": \"Windows\"\n" +
				"    }\n" +
				"  ],\n" +
				"  \"description\": \"Getsoftwaresourcelistsuccess.\"\n" +
				"}";
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseBody, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
        
        //request data
		ResponseBodyBean responseBodyBean = softwareResourceController.softwareResourceList(null, "192.168.11.35",1,10, mockHttpSession);
		Assert.assertEquals("0", responseBodyBean.getCode());
	}
	
	@Test
	public void upload() throws IOException, SQLException {
		//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\"code\" : 0, \"data\":{\"taskName\":\"API@Task_1456209500919\"},\"description\" : \"Start to upload softwaresource success.\"}";
        Map<String,Object> map = JsonUtil.readAsMap(responseBody);
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(map, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
		
        //request data
		String data ="{  \"esights\": [ \"192.168.11.35\" ],"+ //这里是一个数组 
		"\"data\": { \"softwareName\":\"OS_Software1\", "+
		         " \"softwareDescription\":\"..This is a OS template.\","+
		         " \"softwareType\": \"Windows\", "+
	             " \"softwareVersion\": \"Windows Server 2008 R2 x64\", "+
	             " \"softwareEdition\": \"\", "+ 
	             " \"softwareLanguage\": \"Chinese\", "+
	             " \"sourceName\": \"7601.17514.101119 - 1850_x64fre_server_eval_en - us - GRMSXEVAL_EN_DVD.iso\","+
	             " \"sftpserverIP\": \"188.10.18.188\","+
	             " \"username\":\"itSftpUser\","+
	             " \"password\": \"Huawei@123\"  }  }";
		
		ResponseBodyBean responseBodyBean = softwareResourceController.upload(null, data, mockHttpSession);
		Assert.assertEquals("0", responseBodyBean.getCode());
	}
	
	@Test
	public void delete() throws IOException, SQLException {
		//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\"code\":0, \"description\":\"Delete softwaresource success.\"}";
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseBody, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
        
        //request data
		ResponseBodyBean responseBodyBean = softwareResourceController.deleteTask(null, "API@Task_1456209500919", "192.168.11.35", mockHttpSession);
		Assert.assertEquals("0", responseBodyBean.getCode());
	}
	
	@Test
	public void deleteFailed() throws SQLException {
		ResponseBodyBean responseBodyBean = softwareResourceController.delete(null);
		Assert.assertEquals("0", responseBodyBean.getCode());
	}
}
