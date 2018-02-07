<?php
/***************************************************************************
 * Author  ......... Jack Zhang
 * Version ......... 1.0
 * History ......... 2017/4/19 Created
 * Purpose ......... all function related with Huawei server SNMP operations.
 ***************************************************************************/

/**
 * snmp_success is used for keep server snmp status,
 * if connect server not success, we'll not connect it in next call
 */

global $config;
include_once($config["base_path"] . "/lib/snmp.php");

global $snmp_success;
$snmp_success = array();

function _cacti_snmp_get($hostname, $community, $oid, $version, $username, $password, $auth_proto, $priv_pass, $priv_proto, $context, $port = 161, $timeout = 500, $retries = 0, $environ = SNMP_POLLER) {
    global $config;

    /* determine default retries */
    if (($retries == 0) || (!is_numeric($retries))) {
        $retries = read_config_option("snmp_retries");
        if ($retries == "") $retries = 3;
    }

    /* do not attempt to poll invalid combinations */
    if (($version == 0) || (!is_numeric($version)) ||
        (!is_numeric($port)) ||
        (!is_numeric($retries)) ||
        (!is_numeric($timeout)) ||
        (($community == "") && ($version != 3))
    ) {
        return "U";
    }

    $snmp_value = '';
    /* ucd/net snmp want the timeout in seconds */
    $timeout = ceil($timeout / 1000);

    if ($version == "1") {
        $snmp_auth = (read_config_option("snmp_version") == "ucd-snmp") ? cacti_escapeshellarg($community): "-c " . cacti_escapeshellarg($community); /* v1/v2 - community string */
    }elseif ($version == "2") {
        $snmp_auth = (read_config_option("snmp_version") == "ucd-snmp") ? cacti_escapeshellarg($community) : "-c " . cacti_escapeshellarg($community); /* v1/v2 - community string */
        $version = "2c"; /* ucd/net snmp prefers this over '2' */
    }elseif ($version == "3") {
        if ($priv_proto == "[None]" || $priv_pass == '') {
            $proto = "authNoPriv";
            $priv_proto = "";
        }else{
            $proto = "authPriv";
        }

        if (strlen($priv_pass)) {
            $priv_pass = "-X " . cacti_escapeshellarg($priv_pass) . " -x " . cacti_escapeshellarg($priv_proto);
        }else{
            $priv_pass = "";
        }

        if (strlen($context)) {
            $context = "-n " . cacti_escapeshellarg($context);
        }else{
            $context = "";
        }

        $snmp_auth = trim("-u " . cacti_escapeshellarg($username) .
            " -l " . cacti_escapeshellarg($proto) .
            " -a " . cacti_escapeshellarg($auth_proto) .
            " -A " . cacti_escapeshellarg($password) .
            " "    . $priv_pass .
            " "    . $context); /* v3 - username/password */
    }

    /* no valid snmp version has been set, get out */
    if (empty($snmp_auth)) { return; }

    if (read_config_option("snmp_version") == "ucd-snmp") {
        /* escape the command to be executed and vulnerable parameters
         * numeric parameters are not subject to command injection
         * snmp_auth is treated seperately, see above */
        exec(cacti_escapeshellcmd(read_config_option("path_snmpget")) . " -O vt -v$version -t $timeout -r $retries " . cacti_escapeshellarg($hostname) . ":$port $snmp_auth " . cacti_escapeshellarg($oid), $snmp_value);
    }else {
        exec(cacti_escapeshellcmd(read_config_option("path_snmpget")) . " -O fntevU " . $snmp_auth . " -v $version -t $timeout -r $retries " . cacti_escapeshellarg($hostname) . ":$port " . cacti_escapeshellarg($oid), $snmp_value);
    }

    /* fix for multi-line snmp output */
    if (is_array($snmp_value)) {
        $snmp_value = implode(" ", $snmp_value);
    }

    /* fix for multi-line snmp output */
    if (isset($snmp_value)) {
        if (is_array($snmp_value)) {
            $snmp_value = implode(" ", $snmp_value);
        }
    }

    if (substr_count($snmp_value, "Timeout:")) {
        cacti_log("WARNING: SNMP Get Timeout for Host:'$hostname', and OID:'$oid'", false);
    }

    /* strip out non-snmp data */
    $snmp_value = format_snmp_string($snmp_value, false);

    return $snmp_value;
}

function _cacti_snmp_walk($hostname, $community, $oid, $version, $username, $password, $auth_proto, $priv_pass, $priv_proto, $context, $port = 161, $timeout = 500, $retries = 0, $max_oids = 10, $environ = SNMP_POLLER) {
    global $config, $banned_snmp_strings;

    $snmp_oid_included = true;
    $snmp_auth	       = '';
    $snmp_array        = array();
    $temp_array        = array();

    /* determine default retries */
    if (($retries == 0) || (!is_numeric($retries))) {
        $retries = read_config_option("snmp_retries");
        if ($retries == "") $retries = 3;
    }

    /* determine default max_oids */
    if (($max_oids == 0) || (!is_numeric($max_oids))) {
        $max_oids = read_config_option("max_get_size");

        if ($max_oids == "") $max_oids = 10;
    }

    /* do not attempt to poll invalid combinations */
    if (($version == 0) || (!is_numeric($version)) ||
        (!is_numeric($max_oids)) ||
        (!is_numeric($port)) ||
        (!is_numeric($retries)) ||
        (!is_numeric($timeout)) ||
        (($community == "") && ($version != 3))
    ) {
        return array();
    }

    $path_snmpbulkwalk = read_config_option("path_snmpbulkwalk");

    /* ucd/net snmp want the timeout in seconds */
    $timeout = ceil($timeout / 1000);

    if ($version == "1") {
        $snmp_auth = (read_config_option("snmp_version") == "ucd-snmp") ? cacti_escapeshellarg($community): "-c " . cacti_escapeshellarg($community); /* v1/v2 - community string */
    }elseif ($version == "2") {
        $snmp_auth = (read_config_option("snmp_version") == "ucd-snmp") ? cacti_escapeshellarg($community): "-c " . cacti_escapeshellarg($community); /* v1/v2 - community string */
        $version = "2c"; /* ucd/net snmp prefers this over '2' */
    }elseif ($version == "3") {
        if ($priv_proto == "[None]" || $priv_pass == '') {
            $proto = "authNoPriv";
            $priv_proto = "";
        }else{
            $proto = "authPriv";
        }

        if (strlen($priv_pass)) {
            $priv_pass = "-X " . cacti_escapeshellarg($priv_pass) . " -x " . cacti_escapeshellarg($priv_proto);
        }else{
            $priv_pass = "";
        }

        if (strlen($context)) {
            $context = "-n " . cacti_escapeshellarg($context);
        }else{
            $context = "";
        }

        $snmp_auth = trim("-u " . cacti_escapeshellarg($username) .
            " -l " . cacti_escapeshellarg($proto) .
            " -a " . cacti_escapeshellarg($auth_proto) .
            " -A " . cacti_escapeshellarg($password) .
            " "    . $priv_pass .
            " "    . $context); /* v3 - username/password */
    }

    if (read_config_option("snmp_version") == "ucd-snmp") {
        /* escape the command to be executed and vulnerable parameters
         * numeric parameters are not subject to command injection
         * snmp_auth is treated seperately, see above */
        $temp_array = exec_into_array(cacti_escapeshellcmd(read_config_option("path_snmpwalk")) . " -v$version -t $timeout -r $retries " . cacti_escapeshellarg($hostname) . ":$port $snmp_auth " . cacti_escapeshellarg($oid));
    }else {
        if (file_exists($path_snmpbulkwalk) && ($version > 1) && ($max_oids > 1)) {
            $temp_array = exec_into_array(cacti_escapeshellcmd($path_snmpbulkwalk) . " -O Qn $snmp_auth -v $version -t $timeout -r $retries -Cr$max_oids " . cacti_escapeshellarg($hostname) . ":$port " . cacti_escapeshellarg($oid));
        }else{
            $temp_array = exec_into_array(cacti_escapeshellcmd(read_config_option("path_snmpwalk")) . " -O Qn $snmp_auth -v $version -t $timeout -r $retries " . cacti_escapeshellarg($hostname) . ":$port " . cacti_escapeshellarg($oid));
        }
    }

    if (substr_count(implode(" ", $temp_array), "Timeout:")) {
        cacti_log("WARNING: SNMP Walk Timeout for Host:'$hostname', and OID:'$oid'", false);
    }

    /* check for bad entries */
    if (is_array($temp_array) && sizeof($temp_array)) {
        foreach($temp_array as $key => $value) {
            foreach($banned_snmp_strings as $item) {
                if(strstr($value, $item) != "") {
                    unset($temp_array[$key]);
                    continue 2;
                }
            }
        }
    }

    for ($i=0; $i < count($temp_array); $i++) {
        if ($temp_array[$i] != "NULL") {
            /* returned SNMP string e.g.
             * .1.3.6.1.2.1.31.1.1.1.18.1 = STRING: === bla ===
             * split off first chunk before the "="; this is the OID
             */
            list($oid, $value) = explode("=", $temp_array[$i], 2);
            $snmp_array[$i]["oid"]   = trim($oid);
            $snmp_array[$i]["value"] = format_snmp_string($temp_array[$i], true);
        }
    }

    return $snmp_array;
}

/**
 * basic function send snmp request to server and get information
 * @param $server
 * @param $oid
 */
function hs_snmp_get(&$server, $oid, $default=null) {
    global $snmp_success;

    $snmp_port = isset($server["snmp_port"]) ? $server["snmp_port"] : 161;
    $key = $server["ip_address"] . $snmp_port;
    if (!array_key_exists($key, $snmp_success)) {
        if ($oid != "1.3.6.1.4.1.2011.2.235.1.1.1.1.0" && $oid != "1.3.6.1.4.1.2011.2.82.1.82.1.1.0") {
            $server_type = hs_snmp_get_server_type($server);
            if ($server_type != 1 && $server_type != 2) {
                $snmp_success[$key] = false;
            } else {
                $snmp_success[$key] = true;
            }
        }
    } elseif ($snmp_success[$key] == false) {
        return $default;
    }

    $is_v3 = $server["snmp_version"] == 3 ? true : false;
    $val = _cacti_snmp_get(
        $server["ip_address"],
        $server['snmp_community'],
        $oid,
        $server["snmp_version"],
        $is_v3 ? $server["snmp_username"] : "",
        $is_v3 ? $server["snmp_password"] : "",
        $is_v3 ? $server["snmp_auth_protocol"] : "",
        $is_v3 ? $server["snmp_priv_passphrase"] : "",
        $is_v3 ? $server["snmp_priv_protocol"] : "",
        $is_v3 ? $server["snmp_context"] : "",
        $snmp_port,
        isset($server["snmp_timeout"]) ? $server["snmp_timeout"] : 500,
        0,
        SNMP_POLLER);

    if ((!isset($val) || $val == 'U') && isset($default))
        return $default;
    return $val;
}

/**
 * perform a snmp walk operate on target server
 */
function hs_snmp_walk(&$server, $oid) {
    $is_v3 = $server["snmp_version"] == 3 ? true : false;
    $values = _cacti_snmp_walk(
        $server["ip_address"],
        $server['snmp_community'],
        $oid,
        $server["snmp_version"],
        $is_v3 ? $server["snmp_username"] : "",
        $is_v3 ? $server["snmp_password"] : "",
        $is_v3 ? $server["snmp_auth_protocol"] : "",
        $is_v3 ? $server["snmp_priv_passphrase"] : "",
        $is_v3 ? $server["snmp_priv_protocol"] : "",
        $is_v3 ? $server["snmp_context"] : "",
        isset($server["snmp_port"]) ? $server["snmp_port"] : 161,
        isset($server["snmp_timeout"]) ? $server["snmp_timeout"] : 500,
        0,
        5,
        SNMP_POLLER);

    return $values;
}

/**
 * perform a snmpwalk and get related data from target table.
 */
function hs_snmp_get_table(&$server, $type, &$keys_list, $blade=null, $presence_check=null, $absence_val=0, $need_first_val=null) {
    global $hs_oids;

    $table_info = array();
    $type_key = ($type == 1) ? "ibmc" : "hmm";
    if (!empty($presence_check)) {
        $first_oid = $hs_oids[$type_key][$presence_check];
        $first_key = $presence_check;
    } else {
        $first_oid = $hs_oids[$type_key][$keys_list[0]];
        $first_key = $keys_list[0];
    }
    if ($blade != null) {
        $first_oid = str_replace("#", $blade."", $first_oid);
    }

    $first_oid_sub = substr($first_oid, strpos($first_oid, ".2011."));
    $first_vals = hs_snmp_walk($server, $first_oid);
    foreach ($first_vals as $first_val) {
        $oid = $first_val["oid"];
        $key_value = $first_val["value"];
        $instance = substr($oid, strpos($oid, $first_oid_sub) + strlen($first_oid_sub) + 1);

        if ($need_first_val !== null) {
            if ($key_value != $need_first_val)
                continue;
        }

        if (!empty($presence_check)) {
            if ($key_value == $absence_val) {
                continue;
            }
        }

        $inst_info = array();
        for ($i=0; $i<count($keys_list); $i++) {
            $key = $keys_list[$i];
            if ($key == $first_key) {
                $inst_info[$key] = $key_value;
            } else {
                $oid = $hs_oids[$type_key][$key];
                if ($blade != null) {
                    $oid = str_replace("#", $blade."", $oid);
                }
                $inst_info[$key] = hs_snmp_get($server, $oid. "." . $instance, "Unknown");
            }
        }

        $table_info[$instance] = $inst_info;
        if ($need_first_val !== null) {
            break;
        }
    }

    /*
    if (!empty($presence_check)) {
        $keys = array_keys($table_info);
        for ($i=count($keys)-1; $i>=0; $i--) {
            $key = $keys[$i];
            if (is_array($table_info[$key])
                && isset($table_info[$key][$presence_check])
                && $table_info[$key][$presence_check] == $absence_val) {
                unset($table_info[$key]);
            }
        }
    }*/

    return $table_info;
}

/**
 * used to calculate total size of memory, harddisk
 */
function hs_calculate_size($instances, $key) {
    $sum = 0;
    foreach($instances as $inst) {
        if (is_array($inst) && array_key_exists($key, $inst)) {
            $val = (int)$inst[$key];
            if ($val > 0) $sum += $val;
        }
    }

    return round(($sum / 1024), 1);
}

/**
 * get server type by send a snmp query, use oid 1.3.6.1.4.1.2011.2.235.1.1.1.1.0
 *   to query iBMC info, and use oid 1.3.6.1.4.1.2011.2.82.1.82.1.1.0
 *   to query HMM info.
 */
function hs_snmp_get_server_type(&$server) {
    global $hs_oids;

    $server["snmp_version"] == 1;
    $val = hs_snmp_get($server, $hs_oids["ibmc"]["type"], null);
    if (is_numeric($val))
        return 1; //iBMC

    $val = hs_snmp_get($server, $hs_oids["hmm"]["type"], null);
    if (is_numeric($val))
        return 2; //HMM

    if ($val == null || $val == "")
        return -1;

    return 3; //Unknown
}

/**
 * get all information for iBMC
 */
function hs_snmp_get_ibmc_info(&$server, $category_nums=null){
    global $hs_oids;

    if (hs_snmp_get_server_type($server) < 0) {
        return array();
    }

    $ibmc_info = array();
    if ($category_nums == null || in_array(1, $category_nums)) {
        $ibmc_info["hostname"] = hs_snmp_get($server, $hs_oids["ibmc"]["hostname"], "Unknown");
        if (empty($ibmc_info["hostname"]) || trim($ibmc_info["hostname"]) == '')
            $ibmc_info["hostname"] = 'Untitled';

        $ibmc_info["ibmc_ver"] = 'Unknown';
        $ibmc_info["sys_guid"] = hs_snmp_get($server, $hs_oids["ibmc"]["sys_guid"], "Unknown");
        $ibmc_info["status"] = hs_snmp_get($server, $hs_oids["ibmc"]["sys_health"], -1);
        $ibmc_info["model"] = hs_snmp_get($server, $hs_oids["ibmc"]["model"], "Unknown");
        $ibmc_info["ip_address"] = $server["ip_address"];

        //get general info
        $general_info = array();
        $general_info["cpld_ver"] = 'Unknown';
        $general_info["uboot_ver"] = 'Unknown';
        $general_info["fpga_ver"] = 'Unknown';
        $general_info["bios_ver"] = 'Unknown';

        $type_keys = array("ibmc_ver", "fpga_ver", "cpld_ver", "bios_ver", "uboot_ver", "lcd_ver");
        $firmware_types = hs_snmp_walk($server, $hs_oids["ibmc"]["firmware_type"]);
        foreach ($firmware_types as $firmware_type) {
            $oid = $firmware_type["oid"];
            $ftype = (int)$firmware_type["value"];
            $instance = substr($oid, strpos($oid, "2011.2.235.1.1.11.50.1.2.") + 25);

            $firm_name = hs_snmp_get($server, $hs_oids["ibmc"]["firmware_name"] . "." . $instance, "Unknown");
            if (preg_match("/backup/i", $firm_name, $matches))
                continue;
            if ($ftype == 3 && !preg_match("/^CPLD$/i", $firm_name, $matches))
                continue;
            if ($ftype == 4 && !preg_match("/^BIOS$/i", $firm_name, $matches))
                continue;

            $value = hs_snmp_get($server, $hs_oids["ibmc"]["firmware_value"] . "." . $instance, "Unknown");
            if ($ftype > 0 && $ftype < 7) {
                $general_info[$type_keys[$ftype - 1]] = $value;
            } else {
                //Log error
            }
        }

        $ibmc_info['general'] = $general_info;
        if (isset($general_info['ibmc_ver'])) {
            $ibmc_info["ibmc_ver"] = $general_info['ibmc_ver'];
        }
    }

    if ($category_nums == null || in_array(2, $category_nums)) {
        $ibmc_info['mainboard'] = hs_snmp_get_mainboard_info_i($server);
    }
    if ($category_nums == null || in_array(3, $category_nums)) {
        $ibmc_info['memory'] = hs_snmp_get_memory_info_i($server);
    }
    if ($category_nums == null || in_array(4, $category_nums)) {
        $ibmc_info['cpu'] = hs_snmp_get_cpu_info_i($server);
    }
    if ($category_nums == null || in_array(5, $category_nums)) {
        $ibmc_info['disk'] = hs_snmp_get_disk_info_i($server);
    }
    if ($category_nums == null || in_array(6, $category_nums)) {
        $ibmc_info['power'] = hs_snmp_get_power_info_i($server);
    }
    if ($category_nums == null || in_array(7, $category_nums)) {
        $ibmc_info['fan'] = hs_snmp_get_fan_info_i($server);
    }
    if ($category_nums == null || in_array(8, $category_nums)) {
        $ibmc_info['raidcard'] = hs_snmp_get_raidcard_info_i($server);
    }

    return $ibmc_info;
}

/**
 * get all information for HMM
 * for SNMP, all status value we add 100, to make difference with iBMC
 */
function hs_snmp_get_hmm_info(&$server, $blades=null, $category_nums=null){
    global $hs_oids;

    if (hs_snmp_get_server_type($server) < 0) {
        return array();
    }

    $hmm_info = array();
    $blade_count = -1;

    if ($category_nums == null || in_array(1, $category_nums)) {
        $hmm_info["hostname"] = hs_snmp_get($server, $hs_oids["hmm"]["hostname"], "Unknown");
        if (empty($hmm_info["hostname"]) || trim($hmm_info["hostname"]) == '')
            $hmm_info["hostname"] = 'Untitled';
        $hmm_info["smm_ver"] = 'Unknown';
        $hmm_info["model"] = 'E9000'; //hs_snmp_get($server, $hs_oids["hmm"]["model"], "Unknown");
        $hmm_info["ip_address"] = $server["ip_address"];
        $hmm_info["status"] = hs_snmp_get($server, $hs_oids["hmm"]["sys_health"], -2);

        //get general info
        $general_info = array();
        $general_info["uboot_ver"] = 'Unknown';
        $general_info["cpld_ver"] = 'Unknown';
        $general_info["pcb_ver"] = 'Unknown';
        $general_info["bom_ver"] = 'Unknown';
        $general_info["fpga_ver"] = 'Unknown';

        //get version info
        //Uboot    Version :(U54)3.03  CPLD     Version :(U1082)108  161206   PCB      Version :SMMA REV B   BOM      Version :001  FPGA     Version :(U1049)008  130605  Software Version :(U54)6.01  IPMI Module Built:Mar  7 2017 20:21:09
        $version_info = hs_snmp_get($server, $hs_oids["hmm"]["version_info"], "");
        if ($version_info != "") {
            $lines = preg_split("/[\\s:]+/", $version_info, -1, PREG_SPLIT_NO_EMPTY);
            $pre_line = '';
            $value = '';
            @$last_ver = null;
            foreach ($lines as $line) {
                if ($line == '')
                    continue;
                if ($line == "Version"
                    || ($line == "Module" && $pre_line == "IPMI")) {
                    if ($last_ver != null && $value != '') {
                        $last_ver = substr($value, 0, strlen($value) - strlen($pre_line) - 1);
                    }
                    $value = '';

                    if ($pre_line == "Uboot")
                        $last_ver = &$general_info["uboot_ver"];
                    elseif ($pre_line == "CPLD")
                        $last_ver = &$general_info["cpld_ver"];
                    elseif ($pre_line == "PCB")
                        $last_ver = &$general_info["pcb_ver"];
                    elseif ($pre_line == "BOM")
                        $last_ver = &$general_info["bom_ver"];
                    elseif ($pre_line == "FPGA")
                        $last_ver = &$general_info["fpga_ver"];
                    elseif ($pre_line == "Software")
                        $last_ver = &$hmm_info["smm_ver"];
                } else {
                    $value .= $line . " ";
                }

                $pre_line = $line;
            }
        }

        $general_info["device_id"] = hs_snmp_get($server, $hs_oids["hmm"]["device_id"], "Unknown");
        $general_info["smm_presence"] = hs_snmp_get($server, $hs_oids["hmm"]["smm_presence"], -1);
        //manufacturer, sn currently no oid
        $hmm_info['general'] = $general_info;
    }

    if ($category_nums == null || in_array(9, $category_nums)) {
        $shelf_info = array();
        $shelf_info["shelf_chassis_name"] = hs_snmp_get($server, $hs_oids["hmm"]["shelf_chassis_name"], "Unknown");
        $shelf_info["shelf_health"] = hs_snmp_get($server, $hs_oids["hmm"]["shelf_health"], -1);
        $shelf_info["shelf_power_realtime"] = hs_snmp_get($server, $hs_oids["hmm"]["shelf_power_realtime"], "Unknown");
        $shelf_info["shelf_power_capping_enable"] = hs_snmp_get($server, $hs_oids["hmm"]["shelf_power_capping_enable"], -1);
        $shelf_info["shelf_power_capping_value"] = hs_snmp_get($server, $hs_oids["hmm"]["shelf_power_capping_value"], "Unknown");
        $shelf_info["shelf_location"] = hs_snmp_get($server, $hs_oids["hmm"]["shelf_location"], "Unknown");
        $shelf_info["shelf_chassis_id"] = hs_snmp_get($server, $hs_oids["hmm"]["shelf_chassis_id"], "Unknown");
        $shelf_info["shelf_backplane_type"] = hs_snmp_get($server, $hs_oids["hmm"]["shelf_backplane_type"], "Unknown");
        $hmm_info['shelf'] = $shelf_info;
    }

    if ($category_nums == null || in_array(6, $category_nums)) {
        $hmm_info['power'] = hs_snmp_get_power_info_h($server);
    }
    if ($category_nums == null || in_array(7, $category_nums)) {
        $hmm_info['fan'] = hs_snmp_get_fan_info_h($server);
    }

    //total 32 blades slot
    for ($index=1; $index<=32; $index++) {
        $blade_presence = hs_snmp_get($server, str_replace("#", $index."", $hs_oids["hmm"]["blade_presence"]), "Unknown");
        if ($blade_presence == 0) //not presence
            continue;

        if ($blade_count < 0)
            $blade_count = 0;
        $blade_count++;

        if ($blades != null && (!in_array($index, $blades)))
            continue;

        $blade_info = array();

        if ($category_nums == null || in_array(1, $category_nums)) {
            $general_info = array();
            $general_info["blade_health"] = hs_snmp_get($server, str_replace("#", $index . "", $hs_oids["hmm"]["blade_health"]), "-2");
            $general_info["sys_guid"] = hs_snmp_get($server, str_replace("#", $index . "", $hs_oids["hmm"]["sys_guid"]), "Unknown");
            //"the  Straight  BMC  IP:  192.168.100.14,  the  Straight BMC Mask: 255.255.255.0"
            $ip_mac = hs_snmp_get($server, str_replace("#", $index . "", $hs_oids["hmm"]["bmc_ip"]), "Unknown");
            $matches = array();
            if (preg_match("/IP: (.*),.*Mask: (.*)/i", $ip_mac, $matches)) {
                $general_info["bmc_ip"] = $matches[1] . "/" . $matches[2];
            } else {
                $general_info["bmc_ip"] = "Unknown";
            }

            $blade_info["general"] = $general_info;
        }

        //if ($category_nums == null || in_array(2, $category_nums)) {
        //    $blade_info['mainboard'] = hs_snmp_get_mainboard_info_h($server, $index);
        //}
        if ($category_nums == null || in_array(3, $category_nums)) {
            $blade_info['memory'] = hs_snmp_get_memory_info_h($server, $index);
        }
        if ($category_nums == null || in_array(4, $category_nums)) {
            $blade_info['cpu'] = hs_snmp_get_cpu_info_h($server, $index);
        }
        if ($category_nums == null || in_array(5, $category_nums)) {
            $blade_info['disk'] = hs_snmp_get_disk_info_h($server, $index);
        }
        if ($category_nums == null || in_array(11, $category_nums)) {
            $blade_info['network'] = hs_snmp_get_network_info_h($server, $index);
        }
        if ($category_nums == null || in_array(8, $category_nums)) {
            $blade_info['raidcard'] = hs_snmp_get_raidcard_info_h($server, $index);
        }

        if (count($blade_info) > 0) {
            $hmm_info["blade" . $index] = $blade_info;
        }
    }

    if ($category_nums == null || in_array(1, $category_nums)) {
        $hmm_info["blade_count"] = $blade_count;
    }
    return $hmm_info;
}

/***************************************************************************
 * get detail informations of iBMC
 ***************************************************************************/

/**
 * get mainboard information
 */
function hs_snmp_get_mainboard_info_i(&$server) {
    global $hs_oids;

    $keys_list = array("board_type", "board_id");
    $mainboard_ids = hs_snmp_get_table($server, 1, $keys_list, null, null, 0, 1 /*1 for mainboard*/);
    $maiboard_id = "Unknown";
    foreach($mainboard_ids as &$mainboard_id_info) {
        $maiboard_id = $mainboard_id_info["board_id"];
    }

    $keys_list = array("board_index", "board_manufacturer", "board_sn", "board_external_name");
    $mainboard_infos = hs_snmp_get_table($server, 1, $keys_list, null, null, 0, 0 /*only need mainboard, which fruid is 0*/);
    foreach($mainboard_infos as &$mainboard_info) {
        $mainboard_info["board_model"] = hs_snmp_get($server, $hs_oids["ibmc"]["model"], "Unknown");
        $mainboard_info["board_id"] = $maiboard_id;
    }

    return $mainboard_infos;
}

/**
 * get memory information
 */
function hs_snmp_get_memory_info_i(&$server) {
    $keys_list = array("mem_slot", "mem_status", "mem_location", "mem_type", "mem_size", "mem_sn",
                       "mem_manufacturer", "mem_bit_width");
    //status 5 is absence
    $memory_info = hs_snmp_get_table($server, 1, $keys_list, null, "mem_status", 5);
    $memory_info["mem_size"] = hs_calculate_size($memory_info, "mem_size");

    return $memory_info;
}

/**
 * get CPU information
 */
function hs_snmp_get_cpu_info_i(&$server) {
    $keys_list = array("cpu_location", "cpu_manufacturer", "cpu_family", "cpu_status", "cpu_type", "cpu_frequency",
                       "cpu_core_count", "cpu_thread_count");
    $cpu_info = hs_snmp_get_table($server, 1, $keys_list);

    foreach($cpu_info as $key => $value) {
        $cpu = $cpu_info[$key];

        if ($cpu["cpu_type"].'' == '0') {
            unset($cpu_info[$key]);
        }
    }

    $cpu_info["cpu_count"] = count($cpu_info);
    return $cpu_info;
}

/**
 * get harddisk information
 */
function hs_snmp_get_disk_info_i(&$server) {
    $keys_list = array("disk_presence", "disk_capacity", "disk_manufacturer", "disk_model", "disk_sn",
                       "disk_status", "disk_slot", "disk_media_type", "disk_interface_type", "disk_capable_speed", "disk_negotiated_speed");
    $disk_infos = hs_snmp_get_table($server, 1, $keys_list, null, "disk_presence", 1);
    $total_size = 0;
    foreach($disk_infos as &$disk_info) {
        if ($disk_info["disk_capacity"] > 0) {
            $total_size += $disk_info["disk_capacity"];
            $disk_info["disk_capacity"] = $disk_info["disk_capacity"] . "MB";
        } else {
            $disk_info["disk_capacity"] = "Unknown";
        }
        if ($disk_info["disk_capable_speed"] > 0)
            $disk_info["disk_capable_speed"] = $disk_info["disk_capable_speed"]."MB";
        else
            $disk_info["disk_capable_speed"] = "Unknown";
        if ($disk_info["disk_negotiated_speed"] > 0)
            $disk_info["disk_negotiated_speed"] = $disk_info["disk_negotiated_speed"]."MB";
        else
            $disk_info["disk_negotiated_speed"] = "Unknown";
    }

    $disk_infos["disk_size"] = round(($total_size / 1024), 1);
    return $disk_infos;
}

/**
 * get power information
 */
function hs_snmp_get_power_info_i(&$server) {
    global $hs_oids;

    $keys_list = array("power_manufacturer", "power_status", "power_presence", "power_model",
                        "power_location");
    $power_info = hs_snmp_get_table($server, 1, $keys_list, null, "power_presence", 1);

    $power_info["power_count"] = count($power_info);
    $power_info["power_current"] = hs_snmp_get($server, $hs_oids["ibmc"]["power_current"], "Unknown");
    $power_info["power_peak"] = hs_snmp_get($server, $hs_oids["ibmc"]["power_peak"], "Unknown");
    $power_info["power_average"] = hs_snmp_get($server, $hs_oids["ibmc"]["power_average"], "Unknown");
    $power_info["power_total_consumption"] = hs_snmp_get($server, $hs_oids["ibmc"]["power_total_consumption"], "Unknown");
    $power_info["power_is_limit"] = hs_snmp_get($server, $hs_oids["ibmc"]["power_is_limit"], "Unknown");
    $power_info["power_limit_max"] = hs_snmp_get($server, $hs_oids["ibmc"]["power_limit_max"], "Unknown");

    return $power_info;
}

/**
 * get fan information
 */
function hs_snmp_get_fan_info_i(&$server) {
    $keys_list = array("fan_location", "fan_status", "fan_speed", "fan_presence");
    $fan_info = hs_snmp_get_table($server, 1, $keys_list, null, "fan_presence", 1);
    $fan_info["fan_count"] = count($fan_info);

    return $fan_info;
}

/**
 * get raidcard information
 */
function hs_snmp_get_raidcard_info_i(&$server) {
    $keys_list = array("raid_model", "raid_firmware_ver", "raid_status");
    $raidcard_info = hs_snmp_get_table($server, 1, $keys_list);
    $raidcard_info["raid_count"] = count($raidcard_info);

    return $raidcard_info;
}


/***************************************************************************
 * get detail informations of HMM
 ***************************************************************************/

/**
 * general function to hmm table info
 */
function hs_snmp_get_hmm_table(&$server, $blade, &$keys_list, $presence_check=null, $absence_val=0) {
    //for HMM, absence valus is always 0
    return hs_snmp_get_table($server, 2, $keys_list, $blade, $presence_check, $absence_val);
}

/**
 * get mainboard information
 */
function hs_snmp_get_mainboard_info_h(&$server, $blade) {
    //HMM currently no mainboard info
    return array();
}

/**
 * get memory information
 */
function hs_snmp_get_memory_info_h(&$server, $blade) {
    $keys_list = array("mem_presence", "mem_status", "mem_location", "mem_manufacturer");
    $memory_info = hs_snmp_get_hmm_table($server, $blade, $keys_list, "mem_presence");
    foreach($memory_info as &$mem_info) {
        $info_str = $mem_info["mem_manufacturer"];
        //example "Samsung, 1333 MHz, 8192 MB"
        //$info_str = "Hynix, 2400 MHz, 16384 MB,DDR4,0x115B83F0,1200 mV,2 rank,72 bits,Synchronous| Registered (Buffered)";
        $matches = array();
        $matches = explode (",", $info_str);
        if (sizeof($matches) > 2) {
            $mem_info["mem_manufacturer"] = $matches[0];
            $mem_info["mem_size"] = $matches[2];
        }

        //if (is_numeric($mem_info["mem_status"]))
        //    $mem_info["mem_status"] = $mem_info["mem_status"] + 100;
    }

    $memory_info["mem_size"] = hs_calculate_size($memory_info, "mem_size");
    return $memory_info;
}

/**
 * get CPU information
 */
function hs_snmp_get_cpu_info_h(&$server, $blade) {
    $keys_list = array("cpu_presence", "cpu_location", "cpu_manufacturer", "cpu_status");
    $cpu_infos = hs_snmp_get_hmm_table($server, $blade, $keys_list, "cpu_presence");

    foreach($cpu_infos as &$cpu_info) {
        $info_str = $cpu_info["cpu_manufacturer"];
        //example "Intel(R) Corporation,Xeon,3300 MHz"
        //$info_str = "Intel(R) Corporation,Genuine Intel(R) CPU @ 2.40GHz,2400 MHz,F2-06-03-00-FF-FB-EB-BF,6 cores,12 threads,64-bit Capable| Multi-Core| Hardware Thread| Execute Protection| Enhanced Virtualization| Power/Performance Control,32 K,256 K,15360 K";
        $matches = explode (",", $info_str);
        if (sizeof($matches) >= 3) {
            $cpu_info["cpu_manufacturer"] = $matches[0];
            $cpu_info["cpu_type"] = $matches[1];
            $cpu_info["cpu_frequency"] = $matches[2];
        }

        $cpu_info["cpu_core_count"] = -1;
        $cpu_info["cpu_thread_count"] = -1;
        if (sizeof($matches) >= 6) {
            $cpu_info["cpu_core_count"] = (int)$matches[4];
            $cpu_info["cpu_thread_count"] = (int)$matches[5];
        }

        //if (is_numeric($cpu_info["cpu_status"]))
        //    $cpu_info["cpu_status"] = $cpu_info["cpu_status"] + 100;
    }

    $cpu_infos["cpu_count"] = count($cpu_infos);
    return $cpu_infos;
}

/**
 * get harddisk information
 */
function hs_snmp_get_disk_info_h(&$server, $blade) {
    $keys_list = array("disk_presence", "disk_capacity", "disk_interface_type", "disk_manufacturer", "disk_model", "disk_sn",
        "disk_status", "disk_location");
    $disk_infos = hs_snmp_get_hmm_table($server, $blade, $keys_list, "disk_presence");

    $total_disk = 0;
    foreach($disk_infos as &$disk_info) {
        //if (is_numeric($disk_info["disk_status"]))
        //    $disk_info["disk_status"] = $disk_info["disk_status"] + 100;
        $disk_size = (int)$disk_info["disk_capacity"];
        if ($disk_size > 0)
            $total_disk += (int)$disk_info["disk_capacity"];
    }

    $disk_infos["disk_size"] = $total_disk;
    return $disk_infos;
}

/**
 * get network information
 */
function hs_snmp_get_network_info_h(&$server, $blade) {
    $keys_list = array("net_presence", "net_health", "net_index", "net_mark", "net_info", "net_location", "net_mac");
    $net_infos = hs_snmp_get_hmm_table($server, $blade, $keys_list, "net_presence");

    return $net_infos;
}

/**
 * get raidcard information
 */
function hs_snmp_get_raidcard_info_h(&$server, $blade) {
    $keys_list = array("raid_presence", "raid_model", "raid_status", "raid_firmware_ver");
    $raidcard_info = hs_snmp_get_hmm_table($server, $blade, $keys_list, "raid_presence", 1);
    foreach ($raidcard_info as &$raidcard) {
        /*
         bit0: 1 –  保留，固定为1
         bit1: 1- memory correctable error
         bit2: 1- memory uncorrectable error
         bit3: 1- memory ECC error
         bit4: 1- NVRAM uncorrectable error
        */
        //if($raidcard["raid_status"] > 1) {
        //    $raidcard["raid_status"] = 4;
        //}

        //if (is_numeric($raidcard["raid_status"]))
        //    $raidcard["raid_status"] = $raidcard["raid_status"] + 100;
    }

    return $raidcard_info;
}

/**
 * get power information
 */
function hs_snmp_get_power_info_h(&$server) {
    global $hs_oids;

    $keys_list = array("power_presence", "power_status", "power_current");
    $power_info = hs_snmp_get_table($server, 2, $keys_list);
    $keys = array_keys($power_info);
    for ($i=count($keys)-1; $i>=0; $i--) {
        //if($power_info[$keys[$i]]["power_status"] == 1)
        //    $power_info[$keys[$i]]["power_status"] == 4;
        //if($power_info[$keys[$i]]["power_status"] == 0)
        //    $power_info[$keys[$i]]["power_status"] == 1;

        //if (is_numeric($power_info[$keys[$i]]["power_status"]))
        //    $power_info[$keys[$i]]["power_status"] = $power_info[$keys[$i]]["power_status"] + 100;

        if ($power_info[$keys[$i]]["power_presence"] == 0)
            unset($power_info[$keys[$i]]);
    }

    return $power_info;
}

/**
 * get fan information
 */
function hs_snmp_get_fan_info_h(&$server) {
    $keys_list = array("fan_location", "fan_status", "fan_speed", "fan_presence");
    $fan_info = hs_snmp_get_table($server, 2, $keys_list);
    $keys = array_keys($fan_info);
    for ($i=count($keys)-1; $i>=0; $i--) {
        if ($fan_info[$keys[$i]]["fan_presence"] == 0) {
            unset($fan_info[$keys[$i]]);
            continue;
        }

        /*
         1：健康
         2：异常
         5：未知
         */
        //if ($fan_info[$keys[$i]]["fan_status"] == 2)
        //    $fan_info[$keys[$i]]["fan_status"] == 4;

        //if (is_numeric($fan_info[$keys[$i]]["fan_status"]))
        //    $fan_info[$keys[$i]]["fan_status"] = $fan_info[$keys[$i]]["fan_status"] + 100;
    }

    return $fan_info;
}

