package com.huawei.vcenterpluginui.services;

import java.io.IOException;
import java.sql.SQLException;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import com.huawei.vcenterpluginui.constant.SqlFileConstant;
import com.huawei.vcenterpluginui.dao.SystemDao;

/**
 * Created by hyuan on 2017/5/10.
 */
public class SystemServiceImpl implements SystemService {

    private static final Log LOGGER = LogFactory.getLog(SystemServiceImpl.class);

    private SystemDao systemDao;

    @Override
    public void initDB() {
        try {
            if (!systemDao.checkTable(SqlFileConstant.HW_ESIGHT_HOST)) {
                LOGGER.info("creating " + SqlFileConstant.HW_ESIGHT_HOST);
                systemDao.createTable(SqlFileConstant.HW_ESIGHT_HOST);
            }
            if (!systemDao.checkTable(SqlFileConstant.HW_ESIGHT_TASK)) {
                LOGGER.info("creating " + SqlFileConstant.HW_ESIGHT_TASK);
                systemDao.createTable(SqlFileConstant.HW_ESIGHT_TASK);
            }
            if (!systemDao.checkTable(SqlFileConstant.HW_TASK_RESOURCE)) {
                LOGGER.info("creating " + SqlFileConstant.HW_TASK_RESOURCE);
                systemDao.createTable(SqlFileConstant.HW_TASK_RESOURCE);
            }
        } catch (SQLException e) {
            LOGGER.error(e);
        } catch (IOException e) {
            LOGGER.error(e);
        }
	}

	public void setSystemDao(SystemDao systemDao) {
		this.systemDao = systemDao;
	}
}
