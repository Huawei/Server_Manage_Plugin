package com.huawei.vcenterpluginui;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;
import org.junit.runner.RunWith;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

/**
 * Created by hyuan on 2017/5/10.
 */
@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration({"classpath:META-INF/spring/bundle-context-test.xml"})
public class ContextSupported {
    protected static final Log LOGGER = LogFactory.getLog(ContextSupported.class);
}
