package com.huawei.vcenterpluginui.constant;

public class SqlFileConstant {
	public static final String HW_ESIGHT_HOST = "HW_ESIGHT_HOST";
	public static final String HW_ESIGHT_TASK = "HW_ESIGHT_TASK";
	public static final String HW_TASK_RESOURCE = "HW_TASK_RESOURCE";
	public static final String SUFFIX = ".sql";
	
	public static final String HW_ESIGHT_HOST_SQL = "DROP TABLE IF EXISTS \"HW_ESIGHT_HOST\";\n" +
										            "CREATE TABLE \"HW_ESIGHT_HOST\" (\n" +
										            "\"ID\"  integer PRIMARY KEY AUTO_INCREMENT NOT NULL,\n" +
										            "\"HOST_IP\"  nvarchar(255),\n" +
										            "\"ALIAS_NAME\"  nvarchar(255),\n" +
										            "\"HOST_PORT\"  int,\n" +
										            "\"LOGIN_ACCOUNT\"  nvarchar(255),\n" +
										            "\"LOGIN_PWD\"  nvarchar(255),\n" +
										            "\"LATEST_STATUS\"  nvarchar(50),\n" +
										            "\"RESERVED_INT1\"  int,\n" +
										            "\"RESERVED_INT2\"  int,\n" +
										            "\"RESERVED_STR1\"  nvarchar(255),\n" +
										            "\"RESERVED_STR2\"  nvarchar(255),\n" +
										            "\"LAST_MODIFY_TIME\"  datetime,\n" +
										            "\"CREATE_TIME\"  datetime NOT NULL,\n" +
										            "\"CERT_PATH\"  nvarchar(255)\n" +
										            ");";
	
	public static final String HW_ESIGHT_TASK_SQL = "DROP TABLE IF EXISTS \"HW_ESIGHT_TASK\";\n" +
										            "CREATE TABLE \"HW_ESIGHT_TASK\" (\n" +
										            "\"ID\"  INTEGER PRIMARY KEY AUTO_INCREMENT NOT NULL,\n" +
										            "\"HW_ESIGHT_HOST_ID\"  INTEGER NOT NULL,\n" +
										            "\"TASK_NAME\"  varchar(255) NOT NULL,\n" +
										            "\"SOFTWARE_SOURCE_NAME\"  varchar(255) NOT NULL,\n" +
										            "\"TEMPLATES\"  varchar(500),\n" +
										            "\"DEVICE_IP\"  varchar(1024),\n" +
										            "\"TASK_STATUS\"  varchar(255),\n" +
										            "\"TASK_PROGRESS\"  INTEGER,\n" +
										            "\"TASK_RESULT\"  varchar(255),\n" +
										            "\"TASK_CODE\"  varchar(255),\n" +
										            "\"ERROR_DETAIL\"  varchar(2000),\n" +
										            "\"SYNC_STATUS\"  varchar(255),\n" +
										            "\"TASK_TYPE\"  varchar(255) NOT NULL,\n" +
										            "\"RESERVED_INT1\" INTEGER,\n" +
										            "\"RESERVED_INT2\" INTEGER,\n" +
										            "\"RESERVED_STR1\" varchar(500),\n" +
										            "\"RESERVED_STR2\" varchar(500),\n" +
										            "\"LAST_MODIFY_TIME\"  datetime,\n" +
										            "\"CREATE_TIME\"  datetime NOT NULL\n" +
										            ");";
	
	public static final String HW_TASK_RESOURCE_SQL = "DROP TABLE IF EXISTS \"HW_TASK_RESOURCE\";\n" +
											            "CREATE TABLE \"HW_TASK_RESOURCE\" (\n" +
											            "\"ID\"  INTEGER  PRIMARY KEY AUTO_INCREMENT NOT NULL,\n" +
											            "\"HW_ESIGHT_TASK_ID\"  varchar(255) NOT NULL,\n" +
											            "\"DN\"  varchar(255) NOT NULL,\n" +
											            "\"IP_ADDRESS\"  varchar(255),\n" +
											            "\"SYNC_STATUS\"  varchar(255),\n" +
											            "\"TASK_TYPE\"  varchar(255) NOT NULL,\n" +
											            "\"DEVICE_RESULT\"  varchar(255),\n" +
											            "\"ERROR_DETAIL\"  varchar(2000),\n" +
											            "\"DEVICE_PROGRESS\"  INTEGER,\n" +
											            "\"ERROR_CODE\"  varchar(255) ,\n" +
											            "\"RESERVED_INT1\" INTEGER,\n" +
											            "\"RESERVED_INT2\" INTEGER,\n" +
											            "\"RESERVED_STR1\" varchar(500),\n" +
											            "\"RESERVED_STR2\" varchar(500),\n" +
											            "\"LAST_MODIFY_TIME\"  datetime,\n" +
											            "\"CREATE_TIME\"  datetime NOT NULL\n" +
											            ");";
}
