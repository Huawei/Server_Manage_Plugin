package com.huawei.vcenterpluginui.utils;

import com.huawei.vcenterpluginui.ContextSupported;
import com.huawei.vcenterpluginui.dao.H2DataBaseDao;
import org.junit.Assert;
import org.junit.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;

import java.io.IOException;
import java.sql.Connection;

/**
 * Created by hyuan on 2017/5/10.
 */
public class DBUtilsTest extends ContextSupported {

    @Autowired
    private H2DataBaseDao h2DataBaseDao;

    @Value("${h2.url}")
    private String h2Url;

    @Test
    public void testGetDBPathFromURL() {
        Assert.assertEquals("./src/test/resources/db/testH2DB", DBUtils.getDBFileFromURL(h2Url));
    }

    @Test
    public void testLoadDBScript() throws IOException {
        String script = DBUtils.getDBScript("HW_ESIGHT_HOST.sql");
        Assert.assertTrue(script != null && script.length() > 0);
    }

}
