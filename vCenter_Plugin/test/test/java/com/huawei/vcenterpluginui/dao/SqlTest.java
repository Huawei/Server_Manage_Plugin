package com.huawei.vcenterpluginui.dao;

import com.huawei.vcenterpluginui.ContextSupported;
import com.huawei.vcenterpluginui.entity.ESight;
import org.junit.Test;
import org.springframework.beans.factory.annotation.Autowired;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.util.List;

public class SqlTest extends ContextSupported {

    @Autowired
    private ESightDao eSightDao;

    @Test
    public void testConnection() throws Exception {
        Connection con = null;
        PreparedStatement ps = null;
        ResultSet rs = null;
        try {
            con = eSightDao.getConnection();
            ps = con.prepareStatement("select * from HW_ESIGHT_HOST");
            rs = ps.executeQuery();
            while (rs.next()) {
                LOGGER.info(rs.getString("id"));
            }
        } finally {
            eSightDao.closeConnection(con, ps, rs);
        }
    }

    @Test
    public void testGetESight() throws Exception {
        List<ESight> eSight = eSightDao.getESightList("1",-1,-1);
        LOGGER.info(eSight);
    }

}
