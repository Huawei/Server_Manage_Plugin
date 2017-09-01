<?php
/***************************************************************************
 * Author  ......... Jack Zhang
 * Version ......... 1.0
 * History ......... 2017/3/27 Created
 * Purpose ......... Used by cacti plugin framework, to list/install/uninstall
 *                   this plugin
 ***************************************************************************/

/**
 * upgrade databases, for later use
 */
function hwserver_upgrade_database () {
    global $config, $database_default;
    include_once($config['library_path'] . '/database.php');

    hwserver_setup_database ();

    include_once($config['base_path'] . '/plugins/hwserver/setup.php');
    include_once('./functions.php');
    $v = plugin_hwserver_version();

    $oldv = read_config_option('plugin_hwserver_version');

    if ($oldv < .1) {
        db_execute('INSERT INTO settings (name, value) VALUES ("plugin_hwserver_version", "' . $v['version'] . '")');
        $oldv = $v['version'];
    }
}
/**
 * setup needed database tables
 */
function hwserver_setup_database () {

    /**
     * server info table
     */
    $data = array();
    $data['columns'][] = array('name' => 'host_id', 'type' => 'integer', 'NULL' => false);
    $data['columns'][] = array('name' => 'sys_guid', 'type' => 'varchar(64)');
    $data['columns'][] = array('name' => 'status', 'type' => 'integer');
    $data['columns'][] = array('name' => 'import_status', 'type' => 'integer', 'default' => '-1');
    $data['columns'][] = array('name' => 'import_pid', 'type' => 'integer', 'default' => '-1');
    $data['columns'][] = array('name' => 'model', 'type' => 'varchar(64)');
    $data['columns'][] = array('name' => 'ibmc_ver', 'type' => 'varchar(16)');
    $data['columns'][] = array('name' => 'smm_ver', 'type' => 'varchar(16)');
    $data['columns'][] = array('name' => 'ip_address', 'type' => 'varchar(32)');
    $data['columns'][] = array('name' => 'hostname', 'type' => 'varchar(255)');
    $data['columns'][] = array('name' => 'cpu_count', 'type' => 'integer');
    $data['columns'][] = array('name' => 'mem_size', 'type' => 'integer');
    $data['columns'][] = array('name' => 'disk_size', 'type' => 'integer');
    $data['columns'][] = array('name' => 'blade_count', 'type' => 'integer');
    $data['columns'][] = array('name' => 'last_update', 'type' => 'datetime');
    $data['keys'][] = array('name' => 'host_id', 'columns' => 'host_id');
    $data['type'] = 'MyISAM';
    $data['comment'] = 'Table of Huawei Server iBMC infomation';

    //_plugin_db_table_create only create the table when it's not exist
    _plugin_db_table_create ('hwserver', 'huawei_server_info', $data);

    /**
     * Server detail table, use key/value to store all iBMC and HMM server detail informations
     * include cou,memory,harddisk, raidcard, power, fan and mainboard infomations
     */
    $data = array();
    $data['columns'][] = array('name' => 'host_id', 'type' => 'integer', 'NULL' => false);
    $data['columns'][] = array('name' => 'blade', 'type' => 'tinyint', 'default' => '-1');
    $data['columns'][] = array('name' => 'instance', 'type' => 'varchar(64)', 'default' => '');
    $data['columns'][] = array('name' => 'key', 'type' => 'varchar(128)');
    $data['columns'][] = array('name' => 'category_num', 'type' => 'tinyint');
    $data['columns'][] = array('name' => 'value', 'type' => 'varchar(255)');
    $data['columns'][] = array('name' => 'oid', 'type' => 'varchar(128)');
    $data['columns'][] = array('name' => 'last_update', 'type' => 'datetime');
    $data['keys'][] = array('name' => 'host_id', 'columns' => 'host_id');
    $data['keys'][] = array('name' => 'blade', 'columns' => 'blade');
    $data['keys'][] = array('name' => 'instance', 'columns' => 'instance');
    $data['type'] = 'MyISAM';
    $data['comment'] = 'Table of Huawei Server detailes information';
    _plugin_db_table_create ('hwserver', 'huawei_server_detail', $data);

    /**
     * Batch import temp table, keep batch import temp datas
     */
    $data = array();
    $data['columns'][] = array('name' => 'host_id', 'type' => 'integer');
    $data['columns'][] = array('name' => 'batch_id', 'type' => 'varchar(40)');
    $data['columns'][] = array('name' => 'ip_address', 'type' => 'varchar(32)');
    $data['columns'][] = array('name' => 'status', 'type' => 'tinyint', 'default' => '0');
    $data['columns'][] = array('name' => 'import_status', 'type' => 'integer', 'default' => '-1');
    $data['columns'][] = array('name' => 'ping_latency', 'type' => 'integer');
    $data['columns'][] = array('name' => 'message', 'type' => 'text');
    $data['columns'][] = array('name' => 'last_update', 'type' => 'datetime');
    $data['keys'][] = array('name' => 'batch_id', 'columns' => 'batch_id');
    $data['type'] = 'MyISAM';
    $data['comment'] = 'Batch import temp table';
    _plugin_db_table_create ('hwserver', 'huawei_import_temp', $data);

    // add two columns of is_hwserver, import_status, import_result
    _plugin_db_add_column ('hwserver', 'host', array('name' => 'hw_is_ibmc', 'type' => 'tinyint(2)', 'NULL' => false, 'default' => '0', 'after' => 'disabled'));
    _plugin_db_add_column ('hwserver', 'host', array('name' => 'hw_is_hmm', 'type' => 'tinyint(2)', 'NULL' => false, 'default' => '0', 'after' => 'hw_is_ibmc'));
    _plugin_db_add_column ('hwserver', 'host', array('name' => 'hw_is_huawei_server', 'type' => 'varchar(4)', 'NULL' => false, 'default' => '0', 'after' => 'hw_is_hmm'));
    _plugin_db_add_column ('hwserver', 'host', array('name' => 'hw_last_update', 'type' => 'datetime', 'NULL' => true, 'after' => 'hw_is_huawei_server'));

	/* increase the size of the settings table */
	// db_execute("ALTER TABLE settings MODIFY column `value', 'type' => 'varchar(1024) not null default ''");
}

function _plugin_db_add_column ($plugin, $table, $column) {
    // Example: api_plugin_db_add_column ('thold', 'plugin_config', array('name' => 'test' . rand(1, 200), 'type' => 'varchar (255)', 'NULL' => false));

    global $config, $database_default;
    include_once($config['library_path'] . '/database.php');

    $result = db_fetch_assoc('show columns from `' . $table . '`') or die (mysql_error());
    $columns = array();
    foreach($result as $index => $arr) {
        foreach ($arr as $t) {
            $columns[] = $t;
        }
    }
    if (isset($column['name']) && !in_array($column['name'], $columns)) {
        $sql = 'ALTER TABLE `' . $table . '` ADD `' . $column['name'] . '`';
        if (isset($column['type']))
            $sql .= ' ' . $column['type'];
        if (isset($column['unsigned']))
            $sql .= ' unsigned';
        if (isset($column['NULL']) && $column['NULL'] == false)
            $sql .= ' NOT NULL';
        if (isset($column['NULL']) && $column['NULL'] == true && !isset($column['default']))
            $sql .= ' default NULL';
        if (isset($column['default']))
            $sql .= ' default ' . (is_numeric($column['default']) ? $column['default'] : "'" . $column['default'] . "'");
        if (isset($column['auto_increment']))
            $sql .= ' auto_increment';
        if (isset($column['after']))
            $sql .= ' AFTER ' . $column['after'];

        if (db_execute($sql)) {
            cacti_log("Huawei Server plugin:new column added: $table -> $column", false, "HWSERVER");
        }
    }
}

function _plugin_db_table_create ($plugin, $table, $data) {
    global $config;

    include_once($config['library_path'] . '/database.php');

    $result = db_fetch_assoc('SHOW TABLES');
    $tables = array();
    foreach($result as $index => $arr) {
        foreach ($arr as $t) {
            $tables[] = $t;
        }
    }

    if (!in_array($table, $tables)) {
        $c = 0;
        $sql = 'CREATE TABLE `' . $table . "` (\n";
        foreach ($data['columns'] as $column) {
            if (isset($column['name'])) {
                if ($c > 0)
                    $sql .= ",\n";
                $sql .= '`' . $column['name'] . '`';
                if (isset($column['type']))
                    $sql .= ' ' . $column['type'];
                if (isset($column['unsigned']))
                    $sql .= ' unsigned';
                if (isset($column['NULL']) && $column['NULL'] == false)
                    $sql .= ' NOT NULL';
                if (isset($column['NULL']) && $column['NULL'] == true && !isset($column['default']))
                    $sql .= ' default NULL';
                if (isset($column['default']))
                    $sql .= ' default ' . (is_numeric($column['default']) ? $column['default'] : "'" . $column['default'] . "'");
                if (isset($column['auto_increment']))
                    $sql .= ' auto_increment';
                $c++;
            }
        }

        if (isset($data['primary'])) {
            $sql .= ",\n PRIMARY KEY (`" . $data['primary'] . '`)';
        }

        if (isset($data['keys']) && sizeof($data['keys'])) {
            foreach ($data['keys'] as $key) {
                if (isset($key['name'])) {
                    $sql .= ",\n INDEX `" . $key['name'] . '` (`' . $key['columns'] . '`)';
                }
            }
        }

        if (isset($data['unique_keys'])) {
            foreach ($data['unique_keys'] as $key) {
                if (isset($key['name'])) {
                    $sql .= ",\n UNIQUE INDEX `" . $key['name'] . '` (`' . $key['columns'] . '`)';
                }
            }
        }

        $sql .= ') ENGINE = ' . $data['type'];

        if (isset($data['comment'])) {
            $sql .= " COMMENT = '" . $data['comment'] . "'";
        }

        if (db_execute($sql)) {
            cacti_log("Huawei Server plugin:table created: $table", false, "HWSERVER");
        }
    }
}