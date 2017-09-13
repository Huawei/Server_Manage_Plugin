package com.huawei.vcenterpluginui.mvc;

import java.io.IOException;
import java.sql.SQLException;
import java.util.Collection;
import java.util.Map;

import org.junit.Assert;
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
import com.huawei.vcenterpluginui.entity.ESight;
import com.huawei.vcenterpluginui.entity.ResponseBodyBean;
import com.huawei.vcenterpluginui.exception.NoEsightException;

@RunWith(PowerMockRunner.class)
@PowerMockRunnerDelegate(SpringJUnit4ClassRunner.class)
@PrepareForTest({HttpRequestUtil.class})
@PowerMockIgnore({"sun.security.*", "javax.net.*","javax.crypto.*"})
public class ESightControllerTest extends ContextSupported {

	MockHttpSession mockHttpSession = new MockHttpSession();
	
    @Autowired
    private ESightController servicesController;

    private static final String CODE_SUCCESS = "0";

    @Test
    public void getESightList() throws SQLException {
        ResponseBodyBean responseBodyBean = servicesController.getESightList(null, null,0,0);
        Assert.assertEquals(CODE_SUCCESS, responseBodyBean.getCode());
        Assert.assertEquals(2, ((Collection) responseBodyBean.getData()).size());
        Assert.assertEquals(null, responseBodyBean.getDescription());
    }

    @Test
    public void saveESight() throws IOException, SQLException {
        PowerMockito.mockStatic(HttpRequestUtil.class);
        
        Map<String,Object> responseMap = JsonUtil.readAsMap("{\"code\":0,\"data\":\"bfec0163-dd56-473e-b11f-6d5845e1b684\", \"description\":\"Operation success.\"}");
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseMap, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.anyString(), Mockito.any(), Mockito.any(),Mockito.anyString(),Mockito.any())).thenReturn(responseEntity);
    	
        ESight eSight = newESight();
        ResponseBodyBean responseBodyBean = servicesController.saveESight(null, eSight, mockHttpSession);
        Assert.assertEquals(CODE_SUCCESS, responseBodyBean.getCode());
        Assert.assertEquals(null, responseBodyBean.getData());
        Assert.assertEquals(null, responseBodyBean.getDescription());
        
        //测试同名修改
        ResponseBodyBean responseBodyBean2 = servicesController.saveESight(null, eSight, mockHttpSession);
        Assert.assertEquals(CODE_SUCCESS, responseBodyBean2.getCode());

        deleteESight();
    }

    @Test
    public void updateESight() throws IOException, SQLException {
        PowerMockito.mockStatic(HttpRequestUtil.class);
        
        Map<String,Object> responseMap = JsonUtil.readAsMap("{\"code\":0,\"data\":\"bfec0163-dd56-473e-b11f-6d5845e1b684\", \"description\":\"Operation success.\"}");
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseMap, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.anyString(), Mockito.any(), Mockito.any(),Mockito.anyString(),Mockito.any())).thenReturn(responseEntity);
    	
        ESight eSight = newESight();
        ResponseBodyBean responseBodyBean = servicesController.updateESight(null, eSight, mockHttpSession);
        Assert.assertEquals(CODE_SUCCESS, responseBodyBean.getCode());
        Assert.assertEquals(null, responseBodyBean.getData());
        Assert.assertEquals(null, responseBodyBean.getDescription());
        
        //测试不修改账号密码
        eSight.setLoginAccount("");
        eSight.setLoginPwd("");
        ResponseBodyBean responseBodyBean2 = servicesController.updateESight(null, eSight, mockHttpSession);
        Assert.assertEquals(CODE_SUCCESS, responseBodyBean2.getCode());
        
        deleteESight();
    }

    @Test(expected = NoEsightException.class)
    public void deleteESight() throws SQLException, IOException {
        ResponseBodyBean responseBodyBean = servicesController.getESightList(null, "192.168.11.36",0,0);
        Collection<ESight> eSightList = (Collection) responseBodyBean.getData();

        for (ESight eSight : eSightList) {
            Assert.assertEquals("0", servicesController.deleteESight(null,"{ids:["+eSight.getId()+"]}").getCode());
        }
    }

    @Test
    public void testESight() throws IOException, SQLException {
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        
        Map<String,Object> responseMap = JsonUtil.readAsMap("{\"code\":0,\"data\":\"bfec0163-dd56-473e-b11f-6d5845e1b684\", \"description\":\"Operation success.\"}");
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseMap, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.anyString(), Mockito.any(), Mockito.any(),Mockito.anyString(),Mockito.any())).thenReturn(responseEntity);
        ESight eSight = newESight();
        eSight.setHostIp("192.168.11.35");
        ResponseBodyBean responseBodyBean = servicesController.testESight(null, eSight);
        Assert.assertEquals("0", responseBodyBean.getCode());
    }

    private ESight newESight() {
        ESight eSight = new ESight();
        eSight.setHostIp("192.168.11.36");
        eSight.setAliasName("ForTest");
        eSight.setHostPort(32102);
        eSight.setLoginAccount("account");
        eSight.setLoginPwd("pwd");
        return eSight;
    }

}
