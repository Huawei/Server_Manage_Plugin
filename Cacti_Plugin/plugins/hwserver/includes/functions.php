<?php
/***************************************************************************
 * Author  ......... Ni Peng
 * Version ......... 1.0
 * History ......... 2017/3/27 Created
 * Purpose ......... Used by cacti plugin framework, to list/install/uninstall
 *                   this plugin
 ***************************************************************************/

/**
 * generate uuid
 */
function hs_uuid() {
    if (function_exists ( 'com_create_guid' )) {
        return com_create_guid ();
    } else {
        mt_srand ((double) microtime() * 10000);
        $charid = md5 (uniqid (rand(), true));
        $hyphen = chr (45); // "-"
        $uuid = '' . //chr(123)// "{"
            substr ($charid, 0, 8) . $hyphen . substr( $charid, 8, 4) . $hyphen . substr( $charid, 12, 4) . $hyphen . substr ($charid, 16, 4) . $hyphen . substr ($charid, 20, 12);
        //.chr(125);// "}"
        return $uuid;
    }
}

/**
 * used for render cacti header
 */
function hs_render_header() {
    global $config;

    //cacti 1.x use different header
    if (strpos($config['cacti_version'], "0.") === 0) {
        include_once("./plugins/hwserver/general_header.php");
        return;
    }

    top_header();
}

/**
 * used for render cacti bottom
 */
function hs_render_footer() {
    global $config;

    if (strpos($config['cacti_version'], "0.") === 0) {
        include_once("./include/bottom_footer.php");
        return;
    }

    bottom_footer();
}

/**
 * for output ajax result in long execute functions like scan server
 */
function hs_ajax_result($is_success, $message, $data) {
    $result = array (
        'success'=>$is_success,
        'message'=>$message,
        'data'=>$data);

    die(json_encode($result));
}

/**
 * save server list to huawei_server_list table,
 *   which is used for temprary keep the batch import servers status
 */
function hs_save_import_server_list($batch_id, &$server_ip_list) {
    //for scan server, it will insert data first
    $exist = db_fetch_row("SELECT batch_id FROM huawei_import_temp WHERE batch_id='$batch_id'");
    if ($exist) return;

    $success = 0;
    foreach ($server_ip_list as $ip_address) {
        $result = db_execute("INSERT INTO huawei_import_temp (batch_id, ip_address, status, import_status, last_update) 
                    VALUES ('" .$batch_id. "', '" .$ip_address. "', 0, -1, NOW())");
        if ($result) {
            $success++;
        }
    }

    return $success;
}

/**
 * get imported server list
 */
function hs_get_import_server_list($batch_id, $need_snmp=false) {
    if ($need_snmp) {
        $sql = "
            SELECT t.batch_id, t.ip_address, t.host_id, t.ping_latency, t.status, t.import_status, t.message,
                h.hostname, h.snmp_community,h.snmp_version,h.snmp_username,h.snmp_password,
                h.snmp_auth_protocol,h.snmp_priv_passphrase,h.snmp_priv_protocol,h.snmp_context,
                h.snmp_port,h.snmp_timeout 
            FROM huawei_import_temp t
            INNER JOIN host AS h ON h.id = t.host_id
            WHERE batch_id='" . $batch_id . "' 
            ORDER BY ip_address";
    } else {
        $sql = "
            SELECT t.batch_id, t.ip_address, t.ping_latency, t.status, t.import_status, t.message
            FROM huawei_import_temp t
            WHERE batch_id='" . $batch_id . "' 
            ORDER BY ping_latency DESC, ip_address";
    }

    return db_fetch_assoc($sql);
}

/**
 * set batch import server ping status
 */
function hs_set_server_ping_status($batch_id, $server_ip, $ping_status, $latency) {
    $sql = "UPDATE huawei_import_temp
            SET status=" .$ping_status. ", ping_latency=" .$latency. "
            WHERE batch_id='" .$batch_id. "' AND ip_address='" .$server_ip. "'";

    return db_execute($sql);
}

/**
 * set batch import server snmp query status
 */
function hs_set_server_import_status($batch_id, &$server, $import_status, $pid=null) {
    if (!empty($batch_id)) {
        $sql = "UPDATE huawei_import_temp
            SET import_status=" . $import_status;

        if (!empty($server) && array_key_exists("host_id", $server)) {
            $sql .= ", host_id=" . $server['host_id'];
        }

        $sql .= " WHERE batch_id='" . $batch_id . "'";
        if (!empty($server)) {
            $sql .= " AND ip_address='" . $server['ip_address'] . "'";
        }

        db_execute($sql);
    }

    //duplicate the info for display
    if (isset($server) && array_key_exists("host_id", $server)) {
        $sql = "UPDATE huawei_server_info
                SET import_status=$import_status, last_update=NOW()";
        if (!empty($pid)) {
            $sql .= ", import_pid=$pid";
        }

        $sql .= " WHERE host_id='" .$server['host_id']. "'";
        db_execute($sql);
    }
}

/**
 * get batch import server status,
 */
function hs_get_import_status($batch_id, $server_ip=null) {
    $sql = "SELECT ip_address, status, import_status FROM huawei_import_temp 
            WHERE batch_id='" .$batch_id. "'";

    if (!empty($server_ip)) {
        $sql = $sql . " AND ip_address='" . $server_ip . "'";
        return db_fetch_row($sql);
    }

    return db_fetch_assoc($sql);
}

/**
 * get status html
 */
function hs_get_status_html($key, $status, $is_hmm) {
    global $hs_def_status_texts;

    if (!is_numeric($status)) {
        if ($status == '')
            return "<div title=\"Status Value: Unknown\" class=\"state5\" />";
        else
            return "<div title=\"Status Value: $status\" class=\"state5\" />";
    }

    $ori_status = $status;
    $status_texts = $hs_def_status_texts;
    switch($key) {
        case "status":
        case "smm_presence":
        case "shelf_health":
        case "blade_status":
        case "blade_health":
            if ($is_hmm) {
                $status_texts = array(0 => "OK", 1 => "Minor", 2 => "Major", 3 => "Major and Minor", 4 => "Critical",
                    5 => "Critical and Minor", 6 => "Critical and Major", 7 => "Critical and Major and Minor");

                $status = $status + 1;
                if ($status == 4) //please refer to HMM document
                    $status = 3;
                elseif ($status > 4)
                    $status = 4;
                elseif ($status < 0)
                    $status = 5;
            }

            break;

        case "fan_status":
            if ($is_hmm) {
                $status_texts = array(1 => "OK", 2 => "Critical", 5 => "Unknown");
                if($status == 2) {
                    $status = 4;
                }
            }
            break;

        case "power_status":
            if ($is_hmm) {
                $status_texts = array(0 => "OK", 1 => "Critical");
                if($status == 1)
                    $status = 4;
                elseif($status == 0)
                    $status = 1;
            }
            break;

        /*
        case "smm_presence":
            $status_texts = array(0 => "Not present", 1 => "Present", 2 => "Indeterminate");
            if ($status == 0)
                $status = 4;
            if ($status == 2)
                $status = 5;
            break;
        */
    }

    if (array_key_exists($ori_status, $status_texts)) {
        $status_text = $status_texts[$ori_status];
    } elseif ($key == "raid_status") {
        $status_text = '';
        if ($status == -1 || $status == 0xffff || $status == dechex(-1)) {
            $status_text = "Unknown";
        } else {
            $status_text = "OK";

            //Please refer to ibmc doc, 状态定义差一位
            if (!$is_hmm) {
                $status = $status << 1;
                if ($status == 0)
                    $status = 1;
            }

            if (($status & 2) > 0)
                $status_text .= "Memory Correctable Error";
            if (($status & 4) > 0)
                $status_text .= "Memory Uncorrectable Error";
            if (($status & 8) > 0)
                $status_text .= "Memory ECC Error";
            if (($status & 16) > 0)
                $status_text .= "NVRAM Uncorrectable Error";
            if ($status != 1)
                $status = 4;
        }
    } else {
        $status_text = "Unknown";
    }

    if ($status > 0 && $status < 6) {
        return "<div title=\"Status Value: $status_text\" class=\"state" .$status. "\" />";
    } else {
        return "<div title=\"Status Value: Unknown\" class=\"state5\" />";
    }
}

/**
 * get value display text for thoses special keys like: mem_size
 */
function hs_get_value_html($key, $value) {
    if ($value == "Unknown" || $value == '') {
        return "Unknown";
    }

    switch($key) {
    case "shelf_power_realtime":
    case "shelf_power_capping_value":
        return $value . " W";
    case "power_current":
        if (preg_match("/watts?/i", $value, $matches)) {
            $value = preg_replace("/watts?/i", "W", $value);
            return $value;
        }
        return $value . " W";
    case "power_peak":
    case "power_average":
    case "power_limit_max":
        return $value . " W";
    case "power_total_consumption":
        return $value . " KWH";
    case "shelf_power_capping_enable":
        return $value == 0 ? "Disabled" : "Enabled";
    case "power_is_limit":
        return $value == 1 ? "Disabled" : "Enabled";
    case "fan_speed":
        if (is_numeric($value)) {
            if ($value <= 100)
                return $value . "%";
            else
                return $value . " RPM";
        }
        return $value;
    case "mem_size":
    case "disk_size":
    case "disk_capacity":
        if (is_numeric($value)) {
            if ($value > 0)
                return $value . " GB";
            else
                return "Unknown";
        } else {
            return $value;
        }
    case "disk_media_type":
        if ($value == 1)
            return "HDD";
        elseif ($value == 2)
            return "SSD";
        elseif ($value == 3)
            return "SSM";
        else
            return "Unknown";
    case "disk_interface_type":
        if ($value == 2)
            return "Parallel-SCSI";
        elseif ($value == 1)
            return "Undefined";
        elseif ($value == 4)
            return "SATA";
        elseif ($value == 255)
            return "Unknown";
        elseif ($value == 6)
            return "SATA-SAS";
        elseif ($value == 3)
            return "SAS";
        elseif ($value == 5)
            return "Fiber-Channel";
        elseif ($value == 7)
            return "PCIE";
        else
            return "Unknown";
    default:
        return $value;
    }
}

/**
 * get import status description html
 */
function hs_get_import_status_html($import_status) {
    if ($import_status == 999) {
        //global $config;
        //return "<img src=\"". $config['url_path'] . "plugins/hwserver/static/images/loading.gif\" width=\"16\">";
        return "<span style=\"color: blue;\" title=\"Importing server information is in progress, please wait.\">, Importing...</span>";
    }

    return "";
}

/**
 * clear batch import record
 */
function hs_remove_import_server($batch_id) {
    db_execute("DELETE FROM huawei_import_temp WHERE batch_id='" .$batch_id. "'");
}

/**
 * remove host device by ip address
 */
function hs_remove_host_by_ip($ip_address) {
    db_execute("DELETE FROM host WHERE hostname='" .$ip_address. "'");
}

/**
 * get host template id
 */
function hs_get_host_template($host_type) {
    if ($host_type == 1) {
        $template_hash = 'f7675b948e98b3840683f63750d27aa4';
    } elseif ($host_type == 2) {
        $template_hash = '72f1d28592306a00748d9010b55bc9a3';
    } else {
        return -1;
    }

    $template_id = db_fetch_cell("SELECT id FROM host_template WHERE hash='$template_hash'");
    return $template_id;
}

/**
 * function to update cacti device host, set the hw_is_ibmc, hw_is_hmm
 *  field value according host_type.
 *
 * @param $host_id cacti host id
 * @param $host_type 1: iBMC, 2: HMM
 */
function hs_update_cacti_host_type($host_id, $host_type, &$host_info, $is_enable=null) {
    if (!is_numeric($host_id))
        return false;
    $is_ibmc = ($host_type == 1) ? 1 : 0;
    $is_hmm = ($host_type == 2) ? 1 : 0;
    $host_template = hs_get_host_template($host_type);

    $sql = "UPDATE host SET hw_is_ibmc=" .$is_ibmc. ", hw_is_hmm=" .$is_hmm. ", hw_last_update=NOW()";
    if ($is_ibmc == 1 || $is_hmm == 1)
        $sql .= ", hw_is_huawei_server='on'";

    if ($is_enable === 1)
        $sql .= ", disabled=''";
    if ($is_enable === 0)
        $sql .= ", disabled='on'";

    $sql .= " WHERE id=" .$host_id;
    db_execute($sql);

    //update host template
    if ($host_template > 0) {
        global $config;
        include_once($config["base_path"] . "/lib/data_query.php");
        include_once($config["base_path"] . "/lib/api_device.php");
        if (strpos($config['cacti_version'], "0.") === 0) {
            $snmp_queries = db_fetch_assoc("select snmp_query_id from host_template_snmp_query where host_template_id=$host_template");

            if (sizeof($snmp_queries) > 0) {
                foreach ($snmp_queries as $snmp_query) {
                    db_execute("replace into host_snmp_query (host_id,snmp_query_id,reindex_method) values ($host_id," . $snmp_query["snmp_query_id"] . "," . read_config_option("reindex_method") . ")");

                    /* recache snmp data */
                    run_data_query($host_id, $snmp_query["snmp_query_id"]);
                }
            }

            $graph_templates = db_fetch_assoc("select graph_template_id from host_template_graph where host_template_id=$host_template");

            if (sizeof($graph_templates) > 0) {
                foreach ($graph_templates as $graph_template) {
                    db_execute("replace into host_graph (host_id,graph_template_id) values ($host_id," . $graph_template["graph_template_id"] . ")");
                    api_plugin_hook_function('add_graph_template_to_host', array("host_id" => $host_id, "graph_template_id" => $graph_template["graph_template_id"]));
                }
            }
        } else {
            api_device_update_host_template($host_id, $host_template);
        }
    }
    //re-create all graphs
    //hs_create_all_device_graphs($host_id, $host_type, $host_template, $host_info);
}

/**
 * create all graphics according the host template settings
 */
function hs_create_all_device_graphs($host_id, $host_type, $host_template, $host_info) {
    /*
     * below code will have performance problem!!
     */
    /*
    //frist remove all exist graphics
    $sql = "SELECT g.id FROM graph_local AS g 
              INNER JOIN graph_templates AS t
              ON g.graph_template_id=t.id
            WHERE g.host_id=$host_id 
              AND t.hash IN(
                'd6df69a4da6f5ed4feb10b432d694c27', 
                '3f3e251cd5c9c3ac4bce1b1f010576da', 
                '9b4b61972a610c86f46ec7bbf40d8638', 
                '7fc3fe49c6e7a606f34e6623af333275', 
                'f2dab8c23ad4b2f11ad59f8274251beb', 
                '6d866cff5dd65ec1f3e82e8d6381ae58', 
                '7b9be4cd100d4dc073b635f0efce7c1a', 
                '7c57868c3aaf16320e4f6f11a3e461b7'
              )";

    $rows = db_fetch_assoc($sql);
    if (sizeof($rows) > 0) {
        $selected_items = array();
        foreach ($rows as $row) {
            array_push($selected_items, $row['id']);
        }

        //delete all
        $data_sources = array_rekey(db_fetch_assoc("SELECT data_template_data.local_data_id
							FROM (data_template_rrd, data_template_data, graph_templates_item)
							WHERE graph_templates_item.task_item_id=data_template_rrd.id
							AND data_template_rrd.local_data_id=data_template_data.local_data_id
							AND " . array_to_sql_or($selected_items, "graph_templates_item.local_graph_id") . "
							AND data_template_data.local_data_id > 0"), "local_data_id", "local_data_id");

        if (sizeof($data_sources)) {
            api_data_source_remove_multi($data_sources);
            api_graph_remove_multi($selected_items);
        }
    }

    //now create all graphs
    $rows = db_fetch_assoc("SELECT graph_template_id FROM host_graph WHERE host_id=" . $host_id);
    foreach($rows as $graph_template_id) {
        $return_array = create_complete_graph_from_template($graph_template_id, $host_id, "", null);
        debug_log_insert("Huawei Server plugin", "Created graph: " . get_graph_title($return_array["local_graph_id"]));

        if (sizeof($return_array["local_data_id"])) { # we expect at least one data source associated
            foreach ($return_array["local_data_id"] as $item) {
                push_out_host($host_id, $item);
            }
        } else {
            debug_log_insert("Huawei Server plugin", "ERROR: no Data Source associated. Check Template");
        }
    }

    //snmp query related graphs
    SELECT
			graph_templates.id AS graph_template_id,
			graph_templates.name AS graph_template_name
			FROM (host_graph,graph_templates)
			WHERE host_graph.graph_template_id=graph_templates.id
    AND host_graph.host_id=19

    $snmp_query_ids = array();
    if ($host_type == 1)
        $rows = db_fetch_assoc("SELECT id FROM snmp_query WHERE hash='08e65b62e4016f7705ec6d76b1843276'");
    elseif ($host_type == 2)
        $rows = db_fetch_assoc("SELECT id FROM snmp_query WHERE hash IN('a5aa1f0b0acb77e48535e9c205669628', 'c4e7ed3a060c4f36a1d438642932780d')");
    if (!empty($rows)) {
        foreach($rows as $row) {
            run_data_query($host_id, $row["id"]);
            array_push($snmp_query_ids, $row["id"]);
        }
    }
    */
}

/**
 * save server detail infomation
 */
function hs_save_server_details($host_id, $host_type, $blade_index, $category_num, &$detail_info, $recursive=true) {
    global $hs_oids;
    $host_type = ($host_type) == 1 ? "ibmc" : "hmm";
    foreach($detail_info as $instance=>$values) {
        if (is_array($values)) {
            if (!$recursive)
                continue;

            foreach($values as $key=>$value) {
                $sql = "
                    INSERT INTO huawei_server_detail (host_id,blade,instance,`key`,category_num,`value`,oid,last_update)
                    VALUES ("
                    .$host_id. ","
                    .$blade_index. ",'"
                    .$instance. "','"
                    .$key. "',"
                    .$category_num. ",'"
                    .$value. "','"
                    .(array_key_exists($key, $hs_oids[$host_type]) ? $hs_oids[$host_type][$key] : "")
                    . "',NOW())";
                db_execute($sql);
            }
        } else {
            $sql = "
                INSERT INTO huawei_server_detail (host_id,blade,instance,`key`,category_num,`value`,oid,last_update)
                VALUES ("
                .$host_id. ","
                .$blade_index. ",'"
                .(-1). "','"
                .$instance. "',"
                .$category_num. ",'"
                .$values. "','"
                .(array_key_exists($instance, $hs_oids[$host_type]) ? $hs_oids[$host_type][$instance] : "")
                . "',NOW())";
            db_execute($sql);
        }
    }

}

/**
 * update server info, mostly for cpu_count, mem_size, disk_size
 */
function hs_update_server_info($host_id, &$server_info) {
    $keys = array("import_status", "ibmc_ver", "smm_ver", "ip_address", "hostname", "cpu_count", "mem_size",
        "disk_size", "shelf_id", "blade_count");
    $find = false;
    $sql = "UPDATE huawei_server_info SET ";

    foreach($keys as $key) {
        if (array_key_exists($key, $server_info)) {
            if (!$find)
                $find = true;
            else
                $sql .= ", ";
            $value = $server_info[$key];
            if (is_integer($value))
                $sql .= $key. "=" .$value;
            else
                $sql .= $key. "='" .$value. "'";
        }
    }

    if (!$find) return;
    $sql .= " WHERE host_id=" .$host_id;
    db_execute($sql);
}

/**
 * save server before get snmp info, only init data
 */
function hs_init_server_info(&$server, $host_id, $import_status) {
    $exist_server = hs_get_server_byid($host_id);
    if (!empty($exist_server)) {
        $sql = "UPDATE huawei_server_info SET import_status=$import_status, last_update=NOW() WHERE host_id=$host_id";
    } else {
        $sql = "INSERT INTO huawei_server_info (host_id, import_status, sys_guid, model, hostname, 
                    ip_address, ibmc_ver, cpu_count, mem_size, disk_size, blade_count, last_update)
                VALUES ($host_id,$import_status,'Unknown','Unknown','Unknown','"
                    . $server["ip_address"] . "','Unknown',-1,-1,-1,-1,NOW())";
        cacti_log("Huawei Server plugin: server created: " .$server["ip_address"], false, "HWSERVER");
    }

    db_execute($sql);
    return false;
}

/**
 * save Huawei server ibmc info to database
 */
function hs_save_ibmc_info($host_id, &$ibmc_info, $category_nums=null) {
    if ($category_nums == null || in_array(1, $category_nums)) {
        if (empty($ibmc_info["ip_address"]) || empty($host_id)) {
            cacti_log("Huawei Server plugin: BAD ip address or host_id!", false, "HWSERVER");
            return;
        }

        $cpu_count = -1;
        $mem_size = -1;
        $disk_size = -1;

        $sql = "SELECT cpu_count,mem_size,disk_size FROM huawei_server_info
                WHERE host_id=$host_id";
        $exist_ibmc_info = db_fetch_row($sql);
        if (!empty($exist_ibmc_info)) {
            $cpu_count = $exist_ibmc_info["cpu_count"];
            $mem_size = $exist_ibmc_info["mem_size"];
            $disk_size = $exist_ibmc_info["disk_size"];
        }

        if (array_key_exists("cpu", $ibmc_info))
            $cpu_count = $ibmc_info["cpu"]["cpu_count"];
        if (array_key_exists("memory", $ibmc_info))
            $mem_size = $ibmc_info["memory"]["mem_size"];
        if (array_key_exists("disk", $ibmc_info))
            $disk_size =  $ibmc_info["disk"]["disk_size"];

        $sql = "DELETE FROM huawei_server_info WHERE host_id=" . $host_id;
        db_execute($sql);

        $general_info = $ibmc_info["general"];
        $sql = "INSERT INTO huawei_server_info (host_id, status, import_status, sys_guid, model, hostname, ip_address, ibmc_ver, cpu_count, mem_size, disk_size,last_update)
            VALUES ("
            . $host_id . ","
            . $ibmc_info["status"] . ",1,'"
            . $ibmc_info["sys_guid"] . "','"
            . $ibmc_info["model"] . "','"
            . $ibmc_info["hostname"] . "','"
            . $ibmc_info["ip_address"] . "','"
            . $ibmc_info["ibmc_ver"] . "',"
            . $cpu_count . ","
            . $mem_size . ","
            . $disk_size . ",NOW())";
        db_execute($sql);

        debug_log_insert("Huawei Server plugin", "Rack server info updated: " . $host_id);
    }

    $sql = "DELETE FROM huawei_server_detail WHERE host_id=" .$host_id;
    if ($category_nums != null) {
        $sql .= " AND category_num IN (" .join(",", $category_nums). ")";
    }
    db_execute($sql);

    //iBMC no blade
    if ($category_nums == null || in_array(1, $category_nums)) {
        hs_save_server_details($host_id, 1, -1, 1, $ibmc_info['general']);
    }
    if ($category_nums == null || in_array(2, $category_nums)) {
        hs_save_server_details($host_id, 1, -1, 2, $ibmc_info['mainboard']);
    }
    if ($category_nums == null || in_array(3, $category_nums)) {
        hs_save_server_details($host_id, 1, -1, 3, $ibmc_info['memory']);
        db_execute("UPDATE huawei_server_info SET mem_size=" .$ibmc_info['memory']['mem_size']. " WHERE host_id=" .$host_id);
    }
    if ($category_nums == null || in_array(4, $category_nums)) {
        hs_save_server_details($host_id, 1, -1, 4, $ibmc_info['cpu']);
        db_execute("UPDATE huawei_server_info SET cpu_count=" .$ibmc_info["cpu"]['cpu_count']. " WHERE host_id=" .$host_id);
    }
    if ($category_nums == null || in_array(5, $category_nums)) {
        hs_save_server_details($host_id, 1, -1, 5, $ibmc_info['disk']);
        db_execute("UPDATE huawei_server_info SET disk_size=" .$ibmc_info['disk']['disk_size']. " WHERE host_id=" .$host_id);
    }
    if ($category_nums == null || in_array(6, $category_nums)) {
        hs_save_server_details($host_id, 1, -1, 6, $ibmc_info['power']);
    }
    if ($category_nums == null || in_array(7, $category_nums)) {
        hs_save_server_details($host_id, 1, -1, 7, $ibmc_info['fan']);
    }
    if ($category_nums == null || in_array(8, $category_nums)) {
        hs_save_server_details($host_id, 1, -1, 8, $ibmc_info['raidcard']);
    }
}

/**
 * save Huawei server hmm info to database
 */
function hs_save_hmm_info($host_id, &$hmm_info, $blades=null, $category_nums=null) {
    if ($category_nums == null || in_array(1, $category_nums)) {
        $sql = "DELETE FROM huawei_server_info WHERE host_id=" . $host_id;
        db_execute($sql);

        $general_info = $hmm_info["general"];
        $sql = "INSERT INTO huawei_server_info (host_id, status, import_status, sys_guid, model, hostname, ip_address, smm_ver, cpu_count, mem_size, disk_size, blade_count, last_update)
            VALUES ("
            . $host_id . ","
            . $hmm_info["status"] . ",1,'','"
            . $hmm_info["model"] . "','"
            . $hmm_info["hostname"] . "','"
            . $hmm_info["ip_address"] . "','"
            . $hmm_info["smm_ver"] . "',"
            . "-1,"
            . "-1,"
            . "-1,"
            . $hmm_info["blade_count"] . ",NOW())";
        db_execute($sql);

        debug_log_insert("Huawei Server plugin", "Blade server info updated: " . $host_id);
    }

    $sql = "DELETE FROM huawei_server_detail WHERE host_id=" .$host_id;
    if ($category_nums != null) {
        $sql .= " AND category_num IN (" .join(",", $category_nums). ")";
    }
    db_execute($sql);

    if ($category_nums == null || in_array(1, $category_nums)) {
        hs_save_server_details($host_id, 2, -1, 1, $hmm_info['general']);
    }
    if ($category_nums == null || in_array(9, $category_nums)) {
        hs_save_server_details($host_id, 2, -1, 9, $hmm_info['shelf']);
    }
    if ($category_nums == null || in_array(6, $category_nums)) {
        hs_save_server_details($host_id, 2, -1, 6, $hmm_info['power']);
    }
    if ($category_nums == null || in_array(7, $category_nums)) {
        hs_save_server_details($host_id, 2, -1, 7, $hmm_info['fan']);
    }

    //HMM no mainboard
    for ($blade_index=1; $blade_index<=32; $blade_index++) {
        $blade = "blade" . $blade_index;
        if (($blades == null || in_array($blade_index, $blades))
            && (array_key_exists($blade, $hmm_info))) {

            if ($category_nums == null || in_array(1, $category_nums)) {
                hs_save_server_details($host_id, 2, $blade_index, 1, $hmm_info[$blade], false);
                if(!empty($hmm_info[$blade]["general"])) {
                    hs_save_server_details($host_id, 2, $blade_index, 1, $hmm_info[$blade]["general"], false);
                }
            }
            //if ($category_nums == null || in_array(2, $category_nums))
            //  hs_save_server_details($host_id, 2, $blade_index, 2, $hmm_info[$blade]['mainboard']);
            if ($category_nums == null || in_array(3, $category_nums))
                hs_save_server_details($host_id, 2, $blade_index, 3, $hmm_info[$blade]['memory']);
            if ($category_nums == null || in_array(4, $category_nums))
                hs_save_server_details($host_id, 2, $blade_index, 4, $hmm_info[$blade]['cpu']);
            if ($category_nums == null || in_array(5, $category_nums))
                hs_save_server_details($host_id, 2, $blade_index, 5, $hmm_info[$blade]['disk']);
            if ($category_nums == null || in_array(11, $category_nums))
                hs_save_server_details($host_id, 2, $blade_index, 11, $hmm_info[$blade]['network']);
            if ($category_nums == null || in_array(8, $category_nums))
                hs_save_server_details($host_id, 2, $blade_index, 8, $hmm_info[$blade]['raidcard']);
        }
    }
}

/**
 * get iBMC servers list from
 */
function hs_get_server_list($filter, $status, $server_type, $sortc, $sortd, $page, $rows) {
    $sql_where = " WHERE 1=1";
    /* form the 'where' clause for our main sql query */
    if (strlen($filter)) {
        $sql_where .= " AND (t.hostname like '%%" . $filter . "%%')";
    }

    if ($status == "-1") {
        /* Show all items */
    } elseif ($status == "-2") {
        $sql_where .= " AND h.disabled='on'";
    } elseif ($status == "-3") {
        $sql_where .= " AND h.disabled=''";
    } elseif ($status == "-4") {
        $sql_where .= " AND (h.status!='3' or host.disabled='on')";
    } elseif (isset($status)) {
        $sql_where .= " AND (h.status=" . $status . " AND h.disabled = '')";
    }

    if ($server_type == "1") {
        $sql_where .= " AND h.hw_is_ibmc=1";
    } elseif ($server_type == "2") {
        $sql_where .= " AND h.hw_is_hmm=1";
    }

    $total_rows = db_fetch_cell("SELECT count(*) FROM huawei_server_info AS t INNER JOIN host AS h ON (t.host_id=h.id) $sql_where");
    $server_list = db_fetch_assoc("SELECT t.*, h.status AS host_status, h.disabled, h.hw_is_ibmc, h.hw_is_hmm, h.hw_last_update 
            FROM huawei_server_info AS t INNER JOIN host AS h ON (t.host_id=h.id)
            $sql_where
            ORDER BY " . $sortc . " " . $sortd . "
            LIMIT " . ($rows * ($page - 1)) . "," . $rows);

    $result = array();
    $result["total"] = $total_rows;
    $result["server_list"] = $server_list;

    return $result;
}

/**
 * get server by host_id
 */
function hs_get_server_byid($host_id) {
    $result = db_fetch_row("
            SELECT t.ip_address, t.host_id, t.hostname, t.import_status,
                h.snmp_community,h.snmp_version,h.snmp_username,h.snmp_password,
                h.snmp_auth_protocol,h.snmp_priv_passphrase,h.snmp_priv_protocol,h.snmp_context,
                h.snmp_port,h.snmp_timeout,h.description,h.hw_is_ibmc,h.hw_is_hmm     
            FROM huawei_server_info AS t LEFT JOIN host AS h ON (t.host_id=h.id)
            WHERE t.host_id=" .$host_id);

    return $result;
}

/**
 * get servers by host_id
 */
function hs_get_server_list_byids($host_ids) {
    $server_list = db_fetch_assoc("
            SELECT t.ip_address, t.host_id, t.hostname, t.import_status,
                h.snmp_community,h.snmp_version,h.snmp_username,h.snmp_password,
                h.snmp_auth_protocol,h.snmp_priv_passphrase,h.snmp_priv_protocol,h.snmp_context,
                h.snmp_port,h.snmp_timeout,h.description,h.hw_is_ibmc,h.hw_is_hmm 
            FROM huawei_server_info AS t LEFT JOIN host AS h ON (t.host_id=h.id)
            WHERE t.host_id IN (" .join(",", $host_ids). ")");

    if (empty($server_list)) {
        return array();
    }

    $result = array();
    foreach($server_list as $server) {
        $result[$server["host_id"]] = $server;
    }
    return $result;
}

/**
 * get cacti host list by ip address
 */
function hs_get_cacti_host_list_byips($ip_list) {
    $host_list = db_fetch_assoc("SELECT id, host_template_id, hostname
            FROM host
            WHERE hostname IN ('" .join("','", $ip_list). "')");

    if (empty($host_list)) {
        return $host_list;
    }

    $result = array();
    foreach($host_list as $host) {
        $result[$host["hostname"]] = $host;
    }

    return $result;
}

/**
 * read server detailed information from huawei_server_detail table,
 *  which is stored as key -> value
 */
function hs_get_server_details($server_type, &$result, $host_id, $blades, $category_nums, $refresh_status=false, &$snmp_info) {
    global $hs_display_lables, $hs_oids;

    $sql = "
        SELECT * FROM huawei_server_detail
        WHERE host_id=$host_id";

    if ($blades != null) {
        $sql .= " AND blade IN (" .join(",", $blades). ")";
    }

    if ($category_nums != null) {
        $sql .= " AND category_num IN (" .join(",", $category_nums). ")";
    }

    $sql .= " ORDER BY category_num, CAST(instance AS integer)";
    $rows = db_fetch_assoc($sql);
    if (empty($rows)) {
        return array();
    }

    $server_type_str = ($server_type == 1) ? "ibmc" : "hmm";
    $key_names = hs_get_server_info_category();

    foreach ($rows as &$row) {
        $put_to = &$result;
        $value = $row["value"];

        //blade
        if ($row["blade"] > -1) {
            if (!array_key_exists("blade".$row["blade"], $put_to)) {
                $result["blade" . $row["blade"]] = array();
            }

            $put_to = &$result["blade" . $row["blade"]];
        }

        //category
        if ($row["category_num"] > -1) {
            $category_name = $key_names[$row["category_num"]][0];
            if (!array_key_exists($category_name, $put_to)) {
                $put_to[$category_name] = array();
            }

            $put_to = &$put_to[$category_name];
        }

        //check if need update value from snmp runtime
        if ($refresh_status && $snmp_info != null) {
            $display_keys = &$hs_display_lables[$server_type];
            if ($row["blade"] > -1) {
                $display_keys = &$hs_display_lables[3];
            }

            if ($row["category_num"] > -1 && array_key_exists($category_name, $display_keys)) {
                $display_keys = &$display_keys[$category_name];
            }

            if (array_key_exists($row["key"], $display_keys)
                && $display_keys[$row["key"]] == 1
                && array_key_exists($row["key"], $hs_oids[$server_type_str])) {
                $oid = $hs_oids[$server_type_str][$row["key"]];
                if(isset($row["instance"]) && $row["instance"] > -1) {
                    $oid = $oid . "." .$row["instance"];
                }
                if ($row["blade"] > -1 && strpos($oid, ".#.") > -1) {
                    $oid = str_replace("#", $row["blade"]."", $oid);
                }

                $new_value = hs_snmp_get($snmp_info, $oid, null);
                if (isset($new_value)) {
                    $value = $new_value;
                } else {
                    $value = "Unknown";
                }
            }
        }

        //instance
        if ($row["instance"] == "-1" || !isset($row["instance"])) {
            $put_to[$row["key"]] = $value;
        } else {
            if (!array_key_exists($row["instance"], $put_to))
                $put_to[$row["instance"]] = array();
            $put_to[$row["instance"]][$row["key"]] = $value;
        }
    }

    return $result;
}

/**
 * get Huawei server iBMC information
 */
function hs_get_ibmc_info($host_info, $category_nums=null, $refresh_status=false) {
    global $hs_oids;

    $result = array();
    $host_id = $host_info["host_id"];

    if ($category_nums == null || in_array(1, $category_nums)) {
        $sql = "
            SELECT host_id,status,model,sys_guid,ibmc_ver,ip_address,hostname,cpu_count,mem_size,disk_size,import_status,last_update
            FROM huawei_server_info
            WHERE host_id=$host_id";
        $result = db_fetch_row($sql);
        if ($refresh_status && $host_info != null) {
            $result["status"] = hs_snmp_get($host_info, $hs_oids["ibmc"]["sys_health"], -2);
        }
    }

    hs_get_server_details(1, $result, $host_id, null, $category_nums, $refresh_status, $host_info);
    if ($category_nums == null || in_array(1, $category_nums)) {
        if (!empty($result["general"])) {
            $result = array_merge($result, $result["general"]);
            unset($result["general"]);
        }
    }

    return $result;
}

/**
 * get Huawei server HMM information
 */
function hs_get_hmm_info($host_info, $blades=null, $category_nums=null, $refresh_status=false) {
    global $hs_oids;

    $result = array();
    $host_id = $host_info["host_id"];

    if ($blades == null && ($category_nums == null || in_array(1, $category_nums))) {
        $sql = "
            SELECT host_id,status,model,sys_guid,smm_ver,ip_address,hostname,blade_count,import_status,last_update
            FROM huawei_server_info
            WHERE host_id=$host_id";
        $result = db_fetch_row($sql);
        if ($refresh_status && $host_info != null) {
            $result["status"] = hs_snmp_get($host_info, $hs_oids["hmm"]["sys_health"], -2);
        }

        //for general info, we need cpu_count, mem_size, disk_size
        if ($category_nums != null && !in_array(3, $category_nums)) {
            array_push($category_nums, 3);
        }
        if ($category_nums != null && !in_array(4, $category_nums)) {
            array_push($category_nums, 4);
        }
        if ($category_nums != null && !in_array(5, $category_nums)) {
            array_push($category_nums, 5);
        }

        hs_get_server_details(2, $result, $host_id, null, $category_nums, $refresh_status, $host_info);
        if(array_key_exists("general", $result)) {
            $result = array_merge($result, $result["general"]);
            unset($result["general"]);
        }

        for ($index=1; $index<=32; $index++) {
            $blade_index = "blade" . $index;
            if (!array_key_exists($blade_index, $result))
                continue;

            if (array_key_exists("cpu", $result[$blade_index]))
                $result[$blade_index]["general"]["cpu_count"] = $result[$blade_index]["cpu"]["cpu_count"];
            if (array_key_exists("memory", $result[$blade_index]))
                $result[$blade_index]["general"]["mem_size"] = $result[$blade_index]["memory"]["mem_size"];
            //if (array_key_exists("disk", $result[$blade_index]))
            //    $result[$blade_index]["general"]["disk_size"] = $result[$blade_index]["disk"]["disk_size"];
        }
    } else {
        hs_get_server_details(2, $result, $host_id, $blades, $category_nums, $refresh_status, $host_info);
    }

    return $result;
}

function hs_get_server_snmp_info($host_id) {
    $sql = "
            SELECT t.ip_address, t.host_id, t.hostname, 
                h.snmp_community,h.snmp_version,h.snmp_username,h.snmp_password,
                h.snmp_auth_protocol,h.snmp_priv_passphrase,h.snmp_priv_protocol,h.snmp_context,
                h.snmp_port,h.snmp_timeout,h.hw_is_ibmc,h.hw_is_hmm 
            FROM huawei_server_info t
            INNER JOIN host AS h ON h.id = t.host_id
            WHERE t.host_id='" . $host_id . "'";
    return db_fetch_row($sql);
}

/*
 * delete server related information
 */
function hs_remove_server($host_id) {
    $sql = "DELETE FROM huawei_server_info WHERE host_id=" . $host_id;
    db_execute($sql);

    $sql = "DELETE FROM huawei_server_detail WHERE host_id=" .$host_id;
    db_execute($sql);

    debug_log_insert("Huawei Server plugin", "server removed: " . $host_id);
}

/**
 * connect server with snmp and refresh server information
 */
function hs_snmp_update_server_info($host_id, &$host, $blades, $category_nums) {
    //first detect HMM or iBMC, or just a normal PC
    $server_type = hs_snmp_get_server_type($host);

    //snmp failed
    if ($server_type == -1) {
        return 3;
    }

    if ($server_type == 1 /*iBMC*/) {
        $ibmc_info = hs_snmp_get_ibmc_info($host, $category_nums);
        if (empty($ibmc_info)) {
            return 5; /* 5 for Unknown error*/
        }

        hs_save_ibmc_info($host_id, $ibmc_info, $category_nums);
        if ($host["hw_is_ibmc"] != 1) {
            $host["hw_is_ibmc"] = 1;
            hs_update_cacti_host_type($host_id, 1, $host, 1);
        }
    }
    else if ($server_type == 2 /*HMM*/) {
        $hmm_info = hs_snmp_get_hmm_info($host, $blades, $category_nums);
        if (empty($hmm_info)) {
            return 5;
        }

        hs_save_hmm_info($host_id, $hmm_info, $blades, $category_nums);
        if ($host["hw_is_hmm"] != 1) {
            $host["hw_is_hmm"] = 1;
            hs_update_cacti_host_type($host_id, 2, $host);
        }
    }
    else {
        return 4; /* 4 for read snmp error*/
    }

    return 1; /* 1 for success*/
}

function hs_get_server_info_category() {
    return array(
        0 => array("undefined", "Undefined"),
        1 => array("general", "General"),
        2 => array("mainboard", "Mainboard"),
        3 => array("memory", "Memory"),
        4 => array("cpu", "CPU"),
        5 => array("disk", "Hard Disk"),
        11=> array("network", "Mezz"),
        6 => array("power", "Power"),
        7 => array("fan", "Fan"),
        8 => array("raidcard", "RAID Card Controller"),
        9 => array("shelf", "Shelf"),
        10 => array("blade", "Blades")
    );
}