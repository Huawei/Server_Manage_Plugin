

-- ----------------------------
-- Table structure for HW_ESIGHT_TASK
-- ----------------------------
DROP TABLE IF EXISTS "HW_ESIGHT_TASK";
CREATE TABLE "HW_ESIGHT_TASK" (
"ID"  INTEGER PRIMARY KEY AUTO_INCREMENT NOT NULL,
"HW_ESIGHT_HOST_ID"  INTEGER NOT NULL,
"TASK_NAME"  varchar(255) NOT NULL,
"SOFTWARE_SOURCE_NAME"  varchar(255) NOT NULL,
"TEMPLATES"  varchar(500),
"DEVICE_IP"  varchar(1024),
"TASK_STATUS"  varchar(255),
"TASK_PROGRESS"  INTEGER,
"TASK_RESULT"  varchar(255),
"TASK_CODE"  varchar(255),
"ERROR_DETAIL"  varchar(2000),
"SYNC_STATUS"  varchar(255),
"TASK_TYPE"  varchar(255) NOT NULL,
"RESERVED_INT1" INTEGER,
"RESERVED_INT2" INTEGER,
"RESERVED_STR1" varchar(500),
"RESERVED_STR2" varchar(500),
"LAST_MODIFY_TIME"  datetime,
"CREATE_TIME"  datetime NOT NULL
);
