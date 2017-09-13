package com.huawei.vcenterpluginui.utils;

import java.io.IOException;
import java.io.InputStream;
import java.sql.Connection;
import java.sql.SQLException;
import java.sql.Statement;

/**
 * Created by hyuan on 2017/5/10.
 */
public final class DBUtils {

    private DBUtils() {

    }

    /**
     * create table in h2
     *
     * @param connection
     * @param sql
     * @throws SQLException
     */
    public static void createTable(Connection connection, String sql) throws SQLException {
        Statement statement = null;
        try {
            statement = connection.createStatement();
            statement.executeUpdate(sql);
        } finally {
            if (statement != null) {
                statement.close();
            }
        }
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
            inputStream.read(buff);
            return new String(buff);
        } finally {
            if (inputStream != null) {
                inputStream.close();
            }
        }
    }

}
