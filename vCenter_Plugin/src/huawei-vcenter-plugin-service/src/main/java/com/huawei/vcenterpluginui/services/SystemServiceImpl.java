package com.huawei.vcenterpluginui.services;

import com.huawei.vcenterpluginui.dao.H2DataBaseDao;
import com.huawei.vcenterpluginui.utils.DBUtils;
import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import java.io.IOException;
import java.sql.Connection;
import java.sql.SQLException;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

/**
 * Created by hyuan on 2017/5/10.
 */
public class SystemServiceImpl implements SystemService {

    private static final Log LOGGER = LogFactory.getLog(SystemServiceImpl.class);

    private H2DataBaseDao h2DataBaseDao;

    private static final String HW_ESIGHT_HOST = "HW_ESIGHT_HOST";
    private static final String HW_ESIGHT_TASK = "HW_ESIGHT_TASK";
    private static final String HW_TASK_RESOURCE = "HW_TASK_RESOURCE";

    @Override
    public void initDB() {
        Connection connection = h2DataBaseDao.getConnection();
        try {
            if (!connection.getMetaData().getTables(null, null, HW_ESIGHT_HOST, null).next()) {
                LOGGER.info("creating " + HW_ESIGHT_HOST);
                DBUtils.createTable(connection, DBUtils.getDBScript("HW_ESIGHT_HOST.sql"));
            }
            if (!connection.getMetaData().getTables(null, null, HW_ESIGHT_TASK, null).next()) {
                LOGGER.info("creating " + HW_ESIGHT_TASK);
                DBUtils.createTable(connection, DBUtils.getDBScript("HW_ESIGHT_TASK.sql"));
            }
            if (!connection.getMetaData().getTables(null, null, HW_TASK_RESOURCE, null).next()) {
                LOGGER.info("creating " + HW_TASK_RESOURCE);
                DBUtils.createTable(connection, DBUtils.getDBScript("HW_TASK_RESOURCE.sql"));
            }
        } catch (SQLException e) {
            LOGGER.error(e);
        } catch (IOException e) {
            LOGGER.error(e);
        } finally {
        	h2DataBaseDao.closeConnection(connection, null, null);
        }
	}

	public void setH2DataBaseDao(H2DataBaseDao h2DataBaseDao) {
		this.h2DataBaseDao = h2DataBaseDao;
	}

	@Override
	public void refreshKey() {
		LOGGER.info("Hello !!");
		Runnable runnable = new Runnable() {
			public void run() {
				// task to run goes here
				LOGGER.info("Hello !!");
			}
		};
		ScheduledExecutorService service = Executors.newSingleThreadScheduledExecutor();
		// 第二个参数为首次执行的延时时间，第三个参数为定时执行的间隔时间
		service.scheduleAtFixedRate(runnable, 10, 1, TimeUnit.SECONDS);
	}

}
