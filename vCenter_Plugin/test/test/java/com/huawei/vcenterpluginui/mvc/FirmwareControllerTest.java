package com.huawei.vcenterpluginui.mvc;

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
public class FirmwareControllerTest extends ContextSupported  {
	
	MockHttpSession mockHttpSession = new MockHttpSession();
	
	@Autowired
    private FirmwareController firmwareController;
	
	@Before
	public void setUp() {
		mockHttpSession.setAttribute("openId_192.168.11.35", "bfec0163-dd56-473e-b11f-6d5845e1b684");
	}
	
	@Test
	public void list() throws Exception{
		//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class); 
        String responseBody = "{\n" +
				"  \"code\": 0,\n" +
				"  \"totalNum\": 3,\n" +
				"  \"data\": [\n" +
				"    {\n" +
				"      \"basepackageName\": \"basePackage1\",\n" +
				"      \"basepackageDesc\": \"This is basepackage1.\",\n" +
				"      \"basepackageType\": \"Firmware\"\n" +
				"    },\n" +
				"    {\n" +
				"      \"basepackageName\": \"basePackage2\",\n" +
				"      \"basepackageDesc\": \"This is basepackage2.\",\n" +
				"      \"basepackageType\": \"Driver\"\n" +
				"    },\n" +
				"    {\n" +
				"      \"basepackageName\": \"basePackage3\",\n" +
				"      \"basepackageDesc\": \"This is basepackage3.\",\n" +
				"      \"basepackageType\": \"Bundle\"\n" +
				"    }\n" +
				"  ],\n" +
				"  \"description\": \"Get basePackage list success.\"\n" +
				"}";
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseBody, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
        
        //request data
		ResponseBodyBean responseBodyBean = firmwareController.list(null, "192.168.11.35",1,10, mockHttpSession);
		Assert.assertEquals("0", responseBodyBean.getCode());
	}
	
	@Test
	public void upload() throws Exception{
		//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\"code\" : 0, \"data\":{\"taskName\":\"API@Task_1456209500919\"},\"description\" : \"Upload basepacakge success.\"}";
        Map<String,Object> map = JsonUtil.readAsMap(responseBody);
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(map, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
        
        //request data
		String data = "{"+
	            "\"esights\": [\"192.168.11.35\"],"+
	            "\"data\": {"+
	                "\"basepackageName\": \"123456\","+
	                "\"basepackageDescription\": \"详情\","+
	                "\"basepackageType\":\"type\","+
	                "\"fileList\": \"123\","+
	                "\"sftpserverIP\": \"123\","+
	                "\"username\": \"456\","+
	                "\"password\": \"789\""+
	            "}"+
	        "}";
			
		ResponseBodyBean responseBodyBean = firmwareController.upload(null, data, mockHttpSession);
		Assert.assertEquals("0", responseBodyBean.getCode());
	}
	
	@Test
	public void progressList()throws Exception{
		//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\n" +
				"  \"code\": 0,\n" +
				"  \"data\": {\n" +
				"    \"taskName\": \"API@Task_ 1456209500919\",\n" +
				"    \"basepackageName\": \"basePackage1\",\n" +
				"    \"taskStatus\": \"Complete\",\n" +
				"    \"taskProgress\": 100,\n" +
				"    \"taskResult\": \"Success\",\n" +
				"    \"taskCode\": 0,\n" +
				"    \"errorDetail\": \"升级文件上传失败，传输中断\"\n" +
				"  },\n" +
				"  \"description\": \"Get task detail success.\"\n" +
				"}";
        Map<String,Object> map = JsonUtil.readAsMap(responseBody);
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(map, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
        
        //request data
		ResponseBodyBean responseBodyBean = firmwareController.progressList(null, mockHttpSession);
		Assert.assertEquals("0", responseBodyBean.getCode());
	}
	
	@Test
	public void getDetail()throws Exception{
		//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\n" +
				"  \"code\": 0,\n" +
				"  \"data\": {\n" +
				"    \"basepackageName\": \"basepackage1\",\n" +
				"    \"basepackageType\": \"Base\",\n" +
				"    \"basepackageDesc\": \"This is basepackage1.\",\n" +
				"    \"basepackageProp\": [\n" +
				"      {\n" +
				"        \"driverPackageName\": \"onboard_driver_win2k8r2sp1.iso\",\n" +
				"        \"supportOS\": \"ubuntu14.04\",\n" +
				"        \"releaseDate\": \" 2017-2-16 \",\n" +
				"        \"driverPackageProp\": [\n" +
				"          {\n" +
				"            \"firmwareType\": \"RAID\",\n" +
				"            \"version\": \"2.00.76.00\",\n" +
				"            \"supportModel\": \"MZ111;SM210;MZ110\"\n" +
				"          },\n" +
				"          {\n" +
				"            \"firmwareType\": \"CNA\",\n" +
				"            \"version\": \"4.0.3\",\n" +
				"            \"supportModel\": \"MZ510;MZ512\"\n" +
				"          }\n" +
				"        ]\n" +
				"      },\n" +
				"      {\n" +
				"        \"firmwarePackageName\": \"RH2288 V2-BIOS-V516.zip\",\n" +
				"        \"supportDevice\": \"RH2288 V2\",\n" +
				"        \"releaseDate\": \" 2017-2-16 \",\n" +
				"        \"firmwarePackageProp\": [\n" +
				"          {\n" +
				"            \"firmwareType\": \"iBMC\",\n" +
				"            \"version\": \"2.30\",\n" +
				"            \"activeMode\": \" ResetBMC \",\n" +
				"            \"supportModel\": \"XH628 V3\"\n" +
				"          }\n" +
				"        ]\n" +
				"      }\n" +
				"    ]\n" +
				"  },\n" +
				"  \"description\": \"Get template detail success.\"\n" +
				"}";
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseBody, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
        
        //request data
		String data = "{"+
	            "esight: '192.168.11.35',"+
	            "param: {"+
	                "basepackageName: '123456'"+
	            "}"+
	        "}";
			
		ResponseBodyBean responseBodyBean = firmwareController.detail(null, "192.168.11.35", "", mockHttpSession);
		Assert.assertEquals("0", responseBodyBean.getCode());
	}
	
	@Test
	public void deleteFailedTask()throws Exception{
		ResponseBodyBean responseBodyBean = firmwareController.delete(null);
		Assert.assertEquals("0", responseBodyBean.getCode());
	}
	
	@Test
	public void getUpgradeTaskList() throws Exception {
		//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\n" +
				"  \"code\": 0,\n" +
				"  \"data\": {\n" +
				"    \"taskName\": \"API@Task_1456209500919\",\n" +
				"    \"taskDesc\": \"This is a BMC and BIOS upgrade task.\",\n" +
				"    \"dn\": \"NE=12345678;NE=87654321\",\n" +
				"    \"taskStatus\": \"Complete\",\n" +
				"    \"taskProgress\": 50,\n" +
				"    \"taskResult\": \"Success\",\n" +
				"    \"taskCode\": \"0\"\n" +
				"  },\n" +
				"  \"description\": \"\"\n" +
				"}";
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseBody, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
        
        //request data
		ResponseBodyBean responseBodyBean = firmwareController.upgradeTasklist(null, "192.168.11.35", "Task_1456209500919", "HW_FAILED", 1, 10, "syncStatus", "true", mockHttpSession);
		Assert.assertEquals("0", responseBodyBean.getCode());
	}
	
	@Test
	public void postUpgradeTask() throws Exception{
		//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\"code\" : 0, \"data\":{        \"taskName\":\"API@Task_1456209500919\"},\"description\" : \"Create task success.\"}";
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseBody, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
        
        //request data
		String data ="{"+
					    "\"esight\": \"192.168.11.35\","+
					    "\"param\": {"+
					        "\"basepackageName\": \"basepackage1\","+
					        "\"firmwareList\": \"CAN,SSD\","+
					        "\"dn\": \"NE=123;NE=1234\""+
					    "}"+
					"}";
        ResponseBodyBean responseBodyBean = firmwareController.upgradeTaskPost(null,data, mockHttpSession);
        Assert.assertEquals("0", responseBodyBean.getCode());
	}
	
	@Test
	public void deleteTask() throws Exception{
		ResponseBodyBean responseBodyBean = firmwareController.deleteTask(null, -1);
		Assert.assertEquals("-99999", responseBodyBean.getCode());
	}
}
