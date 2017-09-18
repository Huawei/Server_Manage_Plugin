

-- ----------------------------
-- Table structure for HWESightHosts
-- ----------------------------
DROP TABLE IF EXISTS "HW_ESIGHT_HOST";
CREATE TABLE "HW_ESIGHT_HOST" (
"ID"  integer PRIMARY KEY AUTO_INCREMENT NOT NULL,
"HOST_IP"  nvarchar(255),
"ALIAS_NAME"  nvarchar(255),
"HOST_PORT"  int,
"LOGIN_ACCOUNT"  nvarchar(255),
"LOGIN_PWD"  nvarchar(255),
"LATEST_STATUS"  nvarchar(50),
"RESERVED_INT1"  int,
"RESERVED_INT2"  int,
"RESERVED_STR1"  nvarchar(255),
"RESERVED_STR2"  nvarchar(255),
"LAST_MODIFY_TIME"  datetime,
"CREATE_TIME"  datetime NOT NULL,
"CERT_PATH"  nvarchar(255)
);
