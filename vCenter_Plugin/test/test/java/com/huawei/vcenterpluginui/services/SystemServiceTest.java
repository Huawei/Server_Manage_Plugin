package com.huawei.vcenterpluginui.services;

import com.huawei.vcenterpluginui.ContextSupported;
import org.junit.Test;
import org.springframework.beans.factory.annotation.Autowired;

/**
 * Created by hyuan on 2017/5/10.
 */
public class SystemServiceTest extends ContextSupported {

    @Autowired
    private SystemService systemService;

    @Test
    public void testInitDB() throws Exception {
        systemService.initDB();
    }

}
