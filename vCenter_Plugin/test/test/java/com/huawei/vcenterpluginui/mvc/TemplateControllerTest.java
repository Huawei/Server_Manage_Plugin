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
public class TemplateControllerTest extends ContextSupported {
    MockHttpSession mockHttpSession = new MockHttpSession();

    @Autowired
    private TemplateController templateController;

	@Before
	public void setUp() {
		mockHttpSession.setAttribute("openId_192.168.11.35", "bfec0163-dd56-473e-b11f-6d5845e1b684");
	}

    @Test
    public void setTemplate() throws IOException {
    	//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\n" +
							"  \"code\": 0,\n" +
							"  \"description\": \"Createtemplatesuccess.\"\n" +
							"}";
        Map<String,Object> map = JsonUtil.readAsMap(responseBody);
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(map, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
        
        //request data
        String data = "{\n" +
                "    \"esights\": [\n" +
                "        \"192.168.11.35\""+
                "    ],\n" +
                "    \"data\": {\n" +
                "        \"templateName\": \"paramtemplateName1\",\n" +
                "        \"templateType\": \"POWER\",\n" +
                "        \"templateDesc\": \"param.templateDesc\",\n" +
                "        \"templateProp\": {\n" +
                "            \"powerPolicy\": \"1\"\n" +
                "        }\n" +
                "    }\n" +
                "}";
        ResponseBodyBean responseBodyBean = templateController.postTemplate(null, data, mockHttpSession);
        Assert.assertEquals("0", responseBodyBean.getCode());
    }
    
    @Test
    public void setTemplateForException() throws IOException {
    	//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = null;
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseBody, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
        
        //request data
        String data = "{\n" +
                "    \"esights\": [\n" +
                "        \"192.168.11.35\",\"192.168.11.37\""+
                "    ],\n" +
                "    \"data\": {\n" +
                "        \"templateName\": \"paramtemplateName1\",\n" +
                "        \"templateType\": \"POWER\",\n" +
                "        \"templateDesc\": \"param.templateDesc\",\n" +
                "        \"templateProp\": {\n" +
                "            \"powerPolicy\": \"1\"\n" +
                "        }\n" +
                "    }\n" +
                "}";
        ResponseBodyBean responseBodyBean = templateController.postTemplate(null, data, mockHttpSession);
        Assert.assertEquals("-80010", responseBodyBean.getCode());
    }
    
    @Test
    public void getTemplateList() throws IOException, SQLException {
    	//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\n" +
				"  \"code\": 0,\n" +
				"  \"totalNum\": 3,\n" +
				"  \"data\": [\n" +
				"    {\n" +
				"      \"templateName\": \"OS_Template1\",\n" +
				"      \"templateType\": \"OS\",\n" +
				"      \"templateDesc\": \"ThisisOStemplate1.\"\n" +
				"    },\n" +
				"    {\n" +
				"      \"templateName\": \"OS_Template2\",\n" +
				"      \"templateType\": \"OS\",\n" +
				"      \"templateDesc\": \"ThisisOStemplate2.\"\n" +
				"    },\n" +
				"    {\n" +
				"      \"templateName\": \"OS_Template3\",\n" +
				"      \"templateType\": \"OS\",\n" +
				"      \"templateDesc\": \"ThisisOStemplate3.\"\n" +
				"    }\n" +
				"  ],\n" +
				"  \"description\": \"Gettemplatelistsuccess.\"\n" +
				"}";
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseBody, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
    	
    	//request data
    	ResponseBodyBean responseBodyBean = templateController.templateList(null, "192.168.11.35","BIOS","1","10", mockHttpSession);
    	Assert.assertEquals("0", responseBodyBean.getCode());
    }
	
	@Test
	public void getTaskList() throws IOException, SQLException {
		//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\n" +
				"  \"code\": 0,\n" +
				"  \"data\": {\n" +
				"    \"taskName\": \"API@Task_1456209500919\",\n" +
				"    \"templates\": \"BIOS_Template;PowerOff_Template\",\n" +
				"    \"deviceDetails\": [{\"dn\":\"NE=34603009\", \"deviceProgress\":0, \"deviceResult\":\"\", \"errorDetail\":\"\"}],\n"+
				"    \"dn\": \"NE=12345678\",\n" +
				"    \"taskStatus\": \"Complete\",\n" +
				"    \"taskProgress\": 100,\n" +
				"    \"taskResult\": \"Success\",\n" +
				"    \"taskCode\": 0,\n" +
				"    \"errorDetail\": \"\"\n" +
				"  },\n" +
				"  \"description\": \"Get task detail success.\"\n" +
				"}";
        Map<String,Object> map = JsonUtil.readAsMap(responseBody);
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(map, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
		
		//request data
		ResponseBodyBean responseBodyBean = templateController.templateTasklist(null,"192.168.11.35", "API@Task_1456209500919", "HW_FAILED", 1, 10, "taskName", "false", mockHttpSession);
		Assert.assertEquals("0", responseBodyBean.getCode());
	}
	
	@Test
	public void postTask() throws IOException, SQLException {
		//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\n" +
				"  \"code\": 0,\n" +
				"  \"data\": {\n" +
				"    \"taskName\": \"API@Task_1456209500919\"\n" +
				"  },\n" +
				"  \"description\": \"Createtasksuccess.\"\n" +
				"}";
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseBody, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
        
		//request data
		String data ="{"+
					    "\"esight\": \"192.168.11.35\","+
					    "\"param\": {"+
					        "\"deployTaskName\": \"deployTaskName\","+
					        "\"templates\": \"template1,poweron1\","+
					        "\"dn\": \"NE=123;NE=1234\""+
					    "}"+
					"}";
		
        ResponseBodyBean responseBodyBean = templateController.templateTaskPost(null,data, mockHttpSession);
        Assert.assertEquals("0", responseBodyBean.getCode());
	}
	
	@Test
	public void deleteTemplate() throws IOException, SQLException {
		//mock data
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\"code\":0,\"description\":\"Delete template success.\"}";
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseBody, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any(), Mockito.any())).thenReturn(responseEntity);
        
        //request data
		ResponseBodyBean responseBodyBean = templateController.deleteTemplate(null, "123", "192.168.11.35", mockHttpSession);
		Assert.assertEquals("0", responseBodyBean.getCode());
	}
	
	@Test
	public void deleteTemplateTask() throws SQLException {
		ResponseBodyBean responseBodyBean = templateController.deleteTask(null, -1);
		Assert.assertEquals("-99999", responseBodyBean.getCode());
	}
}
