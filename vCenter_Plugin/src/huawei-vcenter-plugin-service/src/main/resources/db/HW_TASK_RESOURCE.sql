

-- ----------------------------
-- Table structure for HW_TASK_RESOURCE
-- ----------------------------
DROP TABLE IF EXISTS "HW_TASK_RESOURCE";
CREATE TABLE "HW_TASK_RESOURCE" (
"ID"  INTEGER  PRIMARY KEY AUTO_INCREMENT NOT NULL,
"HW_ESIGHT_TASK_ID"  varchar(255) NOT NULL,
"DN"  varchar(255) NOT NULL,
"IP_ADDRESS"  varchar(255),
"SYNC_STATUS"  varchar(255),
"TASK_TYPE"  varchar(255) NOT NULL,
"DEVICE_RESULT"  varchar(255),
"ERROR_DETAIL"  varchar(2000),
"DEVICE_PROGRESS"  INTEGER,
"ERROR_CODE"  varchar(255) ,
"RESERVED_INT1" INTEGER,
"RESERVED_INT2" INTEGER,
"RESERVED_STR1" varchar(500),
"RESERVED_STR2" varchar(500),
"LAST_MODIFY_TIME"  datetime,
"CREATE_TIME"  datetime NOT NULL
);
