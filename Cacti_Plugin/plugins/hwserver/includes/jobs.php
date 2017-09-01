<?php
/***************************************************************************
 * Author  ......... Jack Zhang
 * Version ......... 1.0
 * History ......... 2017/4/18 Created
 * Purpose ......... Background jobs for scan server and get server info
 *                   through snmp.
 ***************************************************************************/

/* max execute 6 hours */
ini_set("max_execution_time", 3600 * 6);

/* We are not talking to the browser */
$no_http_headers = true;

/* start initialization section */
include_once(dirname(__FILE__) . "/../../../include/global.php");
include_once($config["base_path"] . "/lib/snmp.php");
include_once($config["base_path"] . "/plugins/hwserver/lib/ping.php");
include_once($config["base_path"] . "/plugins/hwserver/includes/oid_tables.php");
include_once($config["base_path"] . "/plugins/hwserver/includes/functions.php");
include_once($config["base_path"] . "/plugins/hwserver/includes/snmp.php");

/* process calling arguments */
$action = '';
$batch_id = '';
$overwrite_exist = 0;
$debug = FALSE;

$parms = $_SERVER["argv"];
array_shift($parms);

if (sizeof($parms)) {
    foreach($parms as $parameter) {
        @list($arg, $value) = @explode("=", $parameter);

        switch ($arg) {
            case "--action":
                $action = $value;
                break;
            case "--batch_id":
                $batch_id = $value;
                break;
            case "--overwrite":
                $overwrite_exist = (int)$value;
                break;
            case "-d":
            case "--debug":
                $debug = TRUE;
                break;

            case "--version":
            case "-V":
            case "-H":
            case "--help":
                display_help();
                //exit(0);

            default:
                echo "ERROR: Invalid Argument: ($arg)\n\n";
                display_help();
                //exit(1);
        }
    }
}

//$action = "import_server"; //DEBUG only
//$batch_id = "ba3dad34-4348-f1ff-f483-14958e1f7d46";
//$batch_id = "c0cf9853-aaca-6b51-a781-68902770a51c";

switch ($action) {
    case "scan_server":
        if ($batch_id == '') {
            print("BAD batch_id!");
            exit(2);
        }

        ping_servers($batch_id);
        break;

    case "import_server":
        import_servers($batch_id, $overwrite_exist);
        break;

    case "update_server":
        refresh_server_info();
        break;

    default:
        print("BAD action: " . $action);
        //exit(3);
}

/**
 * display help information
 */
function display_help() {
    echo "Command line utility to scan server list, and update server detail information.\n\n";
    echo "usage: jobs.php 
                 [--debug|-d] \n
                 --action|-a=scan_server|import_server|update_server \n
                 --batch_id=batch_id \n";
}

/**
 * get all servers from huawei_import_temp,
 * send ping command to each server to get the server latency.
 */
function ping_servers($batch_id)
{
    $server_list = hs_get_import_server_list($batch_id);

    //use english code page
    if (strtoupper(substr(PHP_OS, 0, 3)) === 'WIN') {
        exec("chcp 437");
    }

    //ping server
    $live_server_list = array();
    foreach ($server_list as &$server) {
        $ip_address = $server['ip_address'];
        $ping = new Ping($ip_address, 255, 100);
        $latency = $ping->ping('exec');

        print("ping to: " .$ip_address. " latency: " .$latency. "\n");

        if ($latency == -1) {
            hs_set_server_ping_status($batch_id, $ip_address, 3, -1); //not reachable
        } else if ($latency < 100) {
            array_push($live_server_list, $ip_address);
            hs_set_server_ping_status($batch_id, $ip_address, 1, $latency); //success
        } else {
            hs_set_server_ping_status($batch_id, $ip_address, 2, $latency); //timeout
        }
    }
}

/**
 * get all servers from huawei_import_temp with status 0,
 * retrive server information by snmp, and save the server to host cacti table.
 */
function import_servers($batch_id, $overwrite_exist) {
    $server_list = hs_get_import_server_list($batch_id, true /*need_snmp=true*/);

    //first save all server to huawei_server_info table
    foreach ($server_list as &$server) {
        if ($server['import_status'] != 0)
            continue;

        $server_type = hs_snmp_get_server_type($server);
        if ($server_type > -1) {
            //NOTE: setup->hwserver_device_save will create new record
            //UPDATE 5/12: when batch import, $save['hw_is_huawei_server'] == '', so will not create

            hs_init_server_info($server, $server["host_id"], 999);
            hs_update_cacti_host_type($server['host_id'], $server_type, $server, 1);
            $server["server_type"] = $server_type;
        }
    }

    $live_server_list = array();
    foreach ($server_list as &$server) {
        //only the user selected server will marked as import_status->0
        if ($server['import_status'] != 0)
            continue;

        $server_type = $server["server_type"];

        //snmp failed
        if ($server_type == -1) {
            hs_set_server_import_status($batch_id, $server, 3 /* 3 for connect snmp fail*/);
            continue;
        }

        //update pid and status to 999, 999 means in importing
        hs_set_server_import_status($batch_id, $server, 999, getmypid());

        if ($server_type == 1 /*iBMC*/) {
            $ibmc_info = hs_snmp_get_ibmc_info($server);
            if (!$ibmc_info) {
                hs_set_server_import_status($batch_id, $server, 99 /* 99 for Unknown error*/);
                continue;
            }

            //Debug only
            //file_put_contents("test-ibmc.txt", json_encode($ibmc_info));

            hs_save_ibmc_info($server['host_id'], $ibmc_info);
            //hs_update_cacti_host_type($server['host_id'], 1, 1);
            //update host_id and import status
            hs_set_server_import_status($batch_id, $server, 1 /* 1 for success*/);
        }
        else if ($server_type == 2 /*HMM*/) {
            $hmm_info = hs_snmp_get_hmm_info($server);
            if (!$hmm_info) {
                hs_set_server_import_status($batch_id, $server, 99);
                continue;
            }

            hs_save_hmm_info($server['host_id'], $hmm_info);
            //hs_update_cacti_host_type($server['host_id'], 2, 1);
            hs_set_server_import_status($batch_id, $server, 1);
        }
        else {
            hs_set_server_import_status($batch_id, $server, 4 /* 4 for read snmp error*/);
        }
    }
}

/**
 * get all Huawei servers, and update each server info through snmp.
 */
function refresh_server_info() {
    //query status in [success, not start, skipped, in progress]
    $server_list = db_fetch_assoc("SELECT host_id FROM huawei_server_info WHERE import_status IN (0,1,5,999) AND TIMESTAMPDIFF(HOUR, last_update, NOW()) >= 24");
    if (empty($server_list)) {
        return;
    }

    $ip_list = array();
    foreach ($server_list as $server) {
        $host_id = $server["host_id"];
        array_push($ip_list, $host_id);
    }

    global $ignore_keys;
    $ignore_keys = array(
        "host_id",
        "status",
        "import_status",
        "last_update",
        "mem_status",
        "power_current",
        "power_peak",
        "power_average",
        "power_total_consumption",
        "power_status",
        "power_presence",
        "cpu_status",
        "cpu_presence",
        "disk_status",
        "disk_presence",
        "fan_status",
        "fan_speed",
        "fan_presence",
        "shelf_health",
        "blade_health",
        "raid_status"
    );

    $host_list = hs_get_server_list_byids($ip_list);

    foreach($host_list as $host_info) {
        $result = array();
        $import_status = 1;

        $server_type = hs_snmp_get_server_type($host_info);
        if ($server_type != 1 && $server_type != 2) {
            //TODO: test this case
            cacti_log('Huawei Server plugin: snmp gete server type failed(' . $host_info['host_id'] . '): ' . $host_info["ip_address"]);
            continue;
        }

        //set as 999 until finish, so later update check will skip it
        hs_set_server_import_status(null, $host_info, 999, getmypid());

        if (($server_type == 1 && $host_info["hw_is_ibmc"] != 1)
            || ($server_type == 2 && $host_info["hw_is_hmm"] != 1)) {
            $status = hs_snmp_update_server_info($host_info["host_id"], $host_info, null, null);
            hs_set_server_import_status(null, $host_info, $status, getmypid());
            cacti_log('Huawei Server plugin: server type changed(' . $host_info['host_id'] . '): ' . $host_info["ip_address"]);
            continue;
        }

        if ($host_info["hw_is_ibmc"] == 1) {
            $server_detail = hs_get_ibmc_info($host_info, null, false);
            $server_detail_snmp = hs_snmp_get_ibmc_info($host_info);
            if (!empty($server_detail_snmp)) {
                if (array_key_exists("general", $server_detail_snmp)) {
                    $server_detail_snmp = array_merge($server_detail_snmp, $server_detail_snmp["general"]);
                }

                array_diff_assoc2_deep($result, $server_detail, $server_detail_snmp);
                if (count($result) > 0) {
                    hs_save_ibmc_info($host_info['host_id'], $server_detail_snmp);
                    cacti_log('Huawei Server plugin: server iBMC information updated(' . $host_info['host_id'] . '): ' . $host_info["ip_address"]);
                }
                $import_status = 1;
            } else {
                $import_status = 4; //4 for read snmp error
            }
        } elseif ($host_info["hw_is_hmm"] == 1) {
            $server_detail = hs_get_hmm_info($host_info, null, null, false);
            $server_detail_snmp = hs_snmp_get_hmm_info($host_info);
            if (!empty($server_detail_snmp)) {
                if (array_key_exists("general", $server_detail_snmp)) {
                    $server_detail_snmp = array_merge($server_detail_snmp, $server_detail_snmp["general"]);
                }

                array_diff_assoc2_deep($result, $server_detail, $server_detail_snmp);
                if (count($result) > 0) {
                    hs_save_hmm_info($host_info['host_id'], $server_detail_snmp);
                    cacti_log('Huawei Server plugin: server HMM information updated(' . $host_info['host_id'] . '): ' . $host_info["ip_address"]);
                }
                $import_status = 1;
            } else {
                $import_status = 4;
            }
        } else {
            continue;
        }

        //now update status
        hs_set_server_import_status(null, $host_info, $import_status);
    }
}

function array_diff_assoc2_deep(&$ret, &$array1, &$array2, $parent_k="") {
    global $ignore_keys;
    foreach ($array1 as $k => $v) {
        if (in_array($k, $ignore_keys)) {
            continue;
        }

        if (!isset($array2[$k])) {
            continue;
        } else if (is_array($v) && is_array($array2[$k])) {
            array_diff_assoc2_deep($ret, $v, $array2[$k], (isset($parent_k) ? $parent_k.'/'.$k : $k));
        } else if ($v !=$array2[$k]) {
            if (isset($parent_k)) {
                $ret[$parent_k.'/'.$k] = $v;
            } else {
                $ret[$k] = $v;
            }
        }
        /*
        else {
            unset($array1[$k]);
        }*/
    }

    return $ret;
}
