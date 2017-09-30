package com.huawei.vcenterpluginui.dao;

import java.io.IOException;
import java.io.InputStream;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.SQLException;
import java.sql.Statement;

import com.huawei.vcenterpluginui.constant.SqlFileConstant;

public class SystemDao extends H2DataBaseDao {
	/**
     * create table in h2
     *
     * @param connection
     * @param sql
     * @throws SQLException
     */
	public void createTable(String sqlFile) throws SQLException, IOException {
		Connection con = null;
		PreparedStatement ps = null;
		try {
			con = getConnection();
			if (sqlFile != null) {
				if (SqlFileConstant.HW_ESIGHT_HOST.equals(sqlFile)) {
					ps = con.prepareStatement(getDBScript(SqlFileConstant.HW_ESIGHT_HOST + SqlFileConstant.SUFFIX));
					ps.executeUpdate();
				} else if (SqlFileConstant.HW_ESIGHT_TASK.equals(sqlFile)) {
					ps = con.prepareStatement(getDBScript(SqlFileConstant.HW_ESIGHT_TASK + SqlFileConstant.SUFFIX));
					ps.executeUpdate();
				} else if (SqlFileConstant.HW_TASK_RESOURCE.equals(sqlFile)) {
					ps = con.prepareStatement(getDBScript(SqlFileConstant.HW_TASK_RESOURCE + SqlFileConstant.SUFFIX));
					ps.executeUpdate();
				}
			}
		} catch (SQLException e) {
			LOGGER.error(e);
			throw e;
		} finally {
			closeConnection(con, ps, null);
		}
	}

	public boolean checkTable(String sqlFile) throws SQLException,IOException {
		Connection con = null;
		boolean tableExist = false;
		try {
			con = getConnection();
			if (SqlFileConstant.HW_ESIGHT_HOST.equals(sqlFile)) {
				tableExist = con.getMetaData().getTables(null, null, SqlFileConstant.HW_ESIGHT_HOST, null).next();
			} else if (SqlFileConstant.HW_ESIGHT_TASK.equals(sqlFile)) {
				tableExist = con.getMetaData().getTables(null, null, SqlFileConstant.HW_ESIGHT_TASK, null).next();
			} else if (SqlFileConstant.HW_TASK_RESOURCE.equals(sqlFile)) {
				tableExist = con.getMetaData().getTables(null, null, SqlFileConstant.HW_TASK_RESOURCE, null).next();
			}
		} catch (SQLException e) {
			LOGGER.error(e);
			throw e;
		} finally {
			closeConnection(con, null, null);
		}
		
		return tableExist;
	}
	
    /**
     * get h2 DB file path from URL
     *
     * @param url
     * @return
     */
    public static String getDBFileFromURL(String url) {
        return url.replaceAll("jdbc:h2:", "");
    }

    /**
     * load file content from resources/db folder
     *
     * @param sqlFile
     * @return
     * @throws IOException
     */
    public static String getDBScript(String sqlFile) throws IOException {
        InputStream inputStream = null;
        try {
            inputStream = Thread.currentThread().getContextClassLoader().getResourceAsStream("db/" + sqlFile);
            byte[] buff = new byte[inputStream.available()];
            if(inputStream.read(buff) != -1){
            	return new String(buff,"utf-8");
            }
        } finally {
            if (inputStream != null) {
                inputStream.close();
            }
        }
        return null;
    }
}
