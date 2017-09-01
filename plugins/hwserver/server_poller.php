<?php
/***************************************************************************
 * Author  ......... Jack Zhang
 * Version ......... 1.0
 * History ......... 2017/4/11 Created
 * Purpose ......... This script is used for auto update Huawei server information
 *                   every few minutes
 ***************************************************************************/

function hwserver_poller_bottom () {
    global $config;
    include_once($config['base_path'] . "/lib/database.php");
    include_once($config['base_path'] . "/lib/functions.php");
    include_once($config['base_path'] . "/lib/poller.php");

    $server_list = db_fetch_assoc("SELECT host_id FROM huawei_server_info WHERE import_status IN (0,1,5) AND TIMESTAMPDIFF(HOUR, last_update, NOW()) >= 24");
    if (!isset($server_list) || count($server_list) == 0) {
        return;
    }

    //check clean huawei_import_temp table
    db_execute("DELETE FROM huawei_import_temp WHERE TIMESTAMPDIFF(HOUR, last_update, NOW()) >= 24");

    //check refresh server info
    $debug_str = ""; //"-dxdebug.remote_enable=1 -dxdebug.remote_mode=req -dxdebug.remote_port=9000 -dxdebug.remote_host=192.168.31.240 ";
    $command_string = read_config_option("path_php_binary");
    exec_background($command_string, $debug_str. "\"" . $config["base_path"] . "/plugins/hwserver/includes/jobs.php\" --action=update_server");

    cacti_log('Huawei Server plugin [poller]: start update server info process, total: ' .count($server_list). " server(s).");
}
