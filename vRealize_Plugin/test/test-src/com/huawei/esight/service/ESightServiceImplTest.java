
package com.huawei.esight.service;

import static org.junit.Assert.fail;

import com.huawei.esight.service.bean.ServerDeviceBean;
import com.huawei.esight.service.bean.ServerDeviceDetailBean;

import java.io.InputStream;
import java.util.List;
import java.util.Properties;

import org.apache.log4j.Logger;
import org.junit.Assert;
import org.junit.Test;

/**
 * ESightServiceImpl单元测试类.
 * @author harbor
 *
 */
public class ESightServiceImplTest {
    
    private final Logger logger = Logger.getLogger(ESightServiceImplTest.class);
    
    @Test
    public void testGetServerDetail() {
        
        ESightService service = new ESightServiceImpl() ;
        service.setLogger(Logger.getLogger(ESightServiceImpl.class));
        
        try {
            
            Properties configProperties = new Properties();
            InputStream inputStream = 
                    this.getClass().getClassLoader().getResourceAsStream("unit-test.properties");
            configProperties.load(inputStream);
            
            String host = configProperties.getProperty("test.esight.ip");
            int port = Integer.parseInt(configProperties.getProperty("test.esight.port"));
            String account = configProperties.getProperty("test.esight.account");
            String eSightCode = configProperties.getProperty("test.esight.code");
            
            String openid = service.login(host, port, account, eSightCode);
            Assert.assertNotNull(openid);
            
            if (openid == null || openid.isEmpty()) {
                logger.error("Login FAIL!");
                fail("Login FAIL!");
            }
            
            logger.info("openid is: " + openid);
            
            String[] serverTypes = new String[]{"rack","blade","highdensity"};
            for (String serverType:serverTypes) {
                List<ServerDeviceBean> serverList = service.getServerDeviceList(serverType);
                Assert.assertNotNull(serverList);
                logger.info("list of serverType = " + serverType + "\n " + serverList);
                for (ServerDeviceBean bean : serverList) {
                    if (bean.getChildBlades().isEmpty() == false) {
                        continue;
                    }
                    ServerDeviceDetailBean detailBean = service.getServerDetail(bean.getDn());
                    if (detailBean == null) {
                        logger.error("faile to fetch detail for device with dn ='" + bean.getDn() + "'");
                    }
                    Assert.assertNotNull(detailBean);
                    logger.info("detail of dn = is " + bean.getDn() + " \n   " + detailBean);
                }
            }
            
            service.logout(openid);
            
            logger.info("All Junit test PASSED!"); 
        } catch (Exception e) {
            logger.error("Call API ERROR", e);
            fail("Excetion throw found!");
        }
        
    }
    
}