package com.huawei.vcenterpluginui.mvc;

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
import com.huawei.vcenterpluginui.ContextSupported;
import com.huawei.vcenterpluginui.entity.ResponseBodyBean;

import java.io.IOException;
import java.sql.SQLException;

@RunWith(PowerMockRunner.class)
@PowerMockRunnerDelegate(SpringJUnit4ClassRunner.class)
@PrepareForTest({HttpRequestUtil.class})
@PowerMockIgnore({"sun.security.*", "javax.net.*","javax.crypto.*"})
public class ServerControllerTest extends ContextSupported {

    MockHttpSession mockHttpSession = new MockHttpSession();

    @Autowired
    private ServerController serverController;

    @Before
    public void setUp() {
        mockHttpSession.setAttribute("openId_192.168.11.35", "bfec0163-dd56-473e-b11f-6d5845e1b684");
    }

    @Test
    public void getServerList() throws IOException, SQLException {
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\n" +
                "  \"code\": 0,\n" +
                "  \"data\": [\n" +
                "    {\n" +
                "      \"dn\": \"NE=34603409\",\n" +
                "      \"ipAddress\": \"10.137.62.207\",\n" +
                "      \"serverName\": \"E6000-10.137.62.207\",\n" +
                "      \"serverModel\": \"E6000\",\n" +
                "      \"manufacturer\": \"Huawei Technologies Co., Ltd.\",\n" +
                "      \"status\": -3,\n" +
                "      \"childBlades\": [\n" +
                "        {\n" +
                "          \"ipAddress\": \"10.137.62.208\",\n" +
                "          \"dn\": \"NE=34603411\"\n" +
                "        },\n" +
                "        {\n" +
                "          \"ipAddress\": \"10.137.62.208\",\n" +
                "          \"dn\": \"NE=34603412\"\n" +
                "        }\n" +
                "      ]\n" +
                "    }\n" +
                "  ],\n" +
                "  \"size\": 1,\n" +
                "  \"totalPage\": 1,\n" +
                "  \"description\": \"Succeeded in querying the server list.\"\n" +
                "}";
        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseBody, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(),Mockito.any(),Mockito.any(),Mockito.any(),Mockito.any())).thenReturn(responseEntity);
        ResponseBodyBean responseBodyBean = serverController.getServerList(null, "123", "192.168.11.35", 1, 20, mockHttpSession);
        Assert.assertEquals("0", responseBodyBean.getCode());
    }

    @Test
    public void getDeviceDetail() throws IOException, SQLException {
    	PowerMockito.mockStatic(HttpRequestUtil.class);
        String responseBody = "{\n" +
                "    \"code\": 0,\n" +
                "    \"data\": [\n" +
                "        {\n" +
                "            \"dn\": \"NE=34603409\",\n" +
                "            \"ipAddress\": \"xx.xx.xx.xx\",\n" +
                "            \"name\": \"E6000-xx.xx.xx.xx\",\n" +
                "            \"type\": \"E6000\",\n" +
                "            \"status\": -3,\n" +
                "            \"desc\": \"\",\n" +
                "            \"PSU\": [\n" +
                "                {\n" +
                "                    \"name\": \"PSU1\",\n" +
                "                    \"healthState\": -3,\n" +
                "                    \"inputPower\": \"0 W\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"PSU2\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"inputPower\": \"0 W\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"PSU3\",\n" +
                "                    \"healthState\": -1,\n" +
                "                    \"inputPower\": \"0 W\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"PSU4\",\n" +
                "                    \"healthState\": -3,\n" +
                "                    \"inputPower\": \"0 W\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"PSU5\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"inputPower\": \"0 W\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"PSU6\",\n" +
                "                    \"healthState\": -1,\n" +
                "                    \"inputPower\": \"0 W\"\n" +
                "                }\n" +
                "            ],\n" +
                "            \"Fan\": [\n" +
                "                {\n" +
                "                    \"name\": \"Fan1 Front\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3600.000 RPM\",\n" +
                "                    \"rotatePercent\": \"30\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan1 Rear\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3360.000 RPM\",\n" +
                "                    \"rotatePercent\": \"30\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan2 Front\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3600.000 RPM\",\n" +
                "                    \"rotatePercent\": \"30\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan2 Rear\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3360.000 RPM\",\n" +
                "                    \"rotatePercent\": \"30\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan3 Front\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"4320.000 RPM\",\n" +
                "                    \"rotatePercent\": \"35\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan3 Rear\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3960.000 RPM\",\n" +
                "                    \"rotatePercent\": \"35\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan4 Front\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3600.000 RPM\",\n" +
                "                    \"rotatePercent\": \"30\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan4 Rear\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3360.000 RPM\",\n" +
                "                    \"rotatePercent\": \"30\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan5 Front\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3600.000 RPM\",\n" +
                "                    \"rotatePercent\": \"30\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan5 Rear\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3360.000 RPM\",\n" +
                "                    \"rotatePercent\": \"30\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan6 Front\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"4320.000 RPM\",\n" +
                "                    \"rotatePercent\": \"35\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan6 Rear\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3720.000 RPM\",\n" +
                "                    \"rotatePercent\": \"35\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan7 Front\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3840.000 RPM\",\n" +
                "                    \"rotatePercent\": \"30\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan7 Rear\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3480.000 RPM\",\n" +
                "                    \"rotatePercent\": \"30\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan8 Front\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3720.000 RPM\",\n" +
                "                    \"rotatePercent\": \"30\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan8 Rear\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3360.000 RPM\",\n" +
                "                    \"rotatePercent\": \"30\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan9 Front\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"4200.000 RPM\",\n" +
                "                    \"rotatePercent\": \"35\"\n" +
                "                },\n" +
                "                {\n" +
                "                    \"name\": \"Fan9 Rear\",\n" +
                "                    \"healthState\": 0,\n" +
                "                    \"rotate\": \"3960.000 RPM\",\n" +
                "                    \"rotatePercent\": \"35\"\n" +
                "                }\n" +
                "            ]\n" +
                "        }\n" +
                "    ],\n" +
                "    \"description\": \"Succeeded in querying server details.\"\n" +
                "}";

        ResponseEntity<Object> responseEntity = new ResponseEntity<Object>(responseBody, HttpStatus.OK);
        PowerMockito.when(HttpRequestUtil.requestWithBody(Mockito.any(),Mockito.any(),Mockito.any(),Mockito.any(),Mockito.any())).thenReturn(responseEntity);
        ResponseBodyBean responseBodyBean = serverController.getDeviceDetail(null, "123", "192.168.11.35", mockHttpSession);
        Assert.assertEquals("0", responseBodyBean.getCode());
    }
}
