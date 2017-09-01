<?php
require_once 'PHPUnit/Autoload.php';
include("../../include/config.php");
include("../../lib/poller.php");
include("../../lib/api_device.php");
include('../hwserver/includes/oid_tables.php');
include('../hwserver/includes/jobs.php');

/**
 * Created by IntelliJ IDEA.
 * User: Jack Zhang
 * Date: 2017/4/15
 * Time: 13:56
 */

/**
 * save servers to host table, init with disabled status
 */
function save_host_list($batch_id, $server_list) {
    $result = array();
    foreach($server_list as $server) {
        $is_v3 = $server["snmp_version"] == 3 ? true : false;
        $host_id = api_device_save(
            0 /*id*/,
            1 /*host template id*/,
            "Huawei Server - " . $server['ip_address'],
            $server['ip_address'],
            $server['snmp_community'],
            $server["snmp_version"],
            ($is_v3 ? $server['snmp_username'] : ''),
            ($is_v3 ? $server['snmp_password'] : ''),
            (isset($server["snmp_port"]) ? $server["snmp_port"] : 161),
            30000 /* snmp timeout */,
            "on" /* init with disabled*/,
            3 /* availability_method */,
            2 /* ping_method */,
            23 /* ping_port */,
            400 /* ping_timeout */,
            1 /* ping_retries */,
            "" /* notes */,
            ($is_v3 ? $server['snmp_auth_protocol'] : ''),
            ($is_v3 ? $server['snmp_priv_passphrase'] : ''),
            ($is_v3 ? $server['snmp_priv_protocol'] : ''),
            "" /* snmp_context */,
            10 /* max_oids */,
            1 /* device_threads */);

        $server['host_id'] = $host_id;
        if ($host_id <= 0) {
            hs_set_server_import_status($batch_id, $server, 2); //2: save host failed
        } else {
            hs_set_server_import_status($batch_id, $server, 0);
        }
    }

    //clear cacti error messages, we don't need to display it
    clear_messages();
}

class ImportServerTester extends PHPUnit_Framework_TestCase
{
    function test(){
        $batch_id = '56bf56f0-a31e-4b8a-a27a-f6fc1954849e'; //uuid();
        $ibmc_info = json_decode(file_get_contents("test-ibmc.txt"),TRUE);
        hs_save_ibmc_info(31, $ibmc_info);
        hs_update_cacti_host_type(31, 1);
        $server = array("ip_address" => "58.251.166.177", "host_id" => 31);
        hs_set_server_import_status($batch_id, $server, 1 /* 1 for success*/);

        //$json[$user] = array("first" => $first, "last" => $last);
        //file_put_contents($file, json_encode($json));

        return;
    }

    function testImportIbmcServer()
    {
        $ip_address = "58.251.166.177";
        $batch_id = '88948bfe-a5c5-4632-b84e-e01d140695fa';
        $server_list = array($ip_address);

        hs_remove_import_server($batch_id);
        hs_remove_host_by_ip($ip_address);
        $result = hs_save_import_server_list($batch_id, $server_list);

        $servers = array();
        $server = array();
        $server["snmp_version"] = 3;
        $server['ip_address'] = $ip_address;
        $server['snmp_community'] = "public";
        $server['snmp_username'] = "cacti";
        $server['snmp_password'] = "cacti@123";
        $server["snmp_port"] = 161;
        $server['snmp_auth_protocol'] = "SHA";
        $server['snmp_priv_passphrase'] = "cacti@123";
        $server['snmp_priv_protocol'] = "AES128";
        $servers[0] = $server;

        save_host_list($batch_id, $servers);
        import_servers($batch_id);

        //scan_iprange('56bf56f0-a31e-4b8a-a27a-f6fc1954849e', '192.168.test', '192.168.99.10');
        $this->assertEquals(true, true);
    }

    function testImportHmmServer()
    {
        $ip_address = "192.168.63.2";
        $batch_id = '56bf56f0-a31e-4b8a-a27a-f6fc1954849e'; //uuid();
        $server_list = array($ip_address);

        hs_remove_import_server($batch_id);
        hs_remove_host_by_ip($ip_address);
        $result = hs_save_import_server_list($batch_id, $server_list);

        $servers = array();
        $server = array();
        $server["snmp_version"] = 3;
        $server['ip_address'] = $ip_address; //"58.251.166.177";
        $server['snmp_community'] = "public";
        $server['snmp_username'] = "user1"; //"cacti";
        $server['snmp_password'] = "authkey1"; //"cacti@123";
        $server["snmp_port"] = 161;
        $server['snmp_auth_protocol'] = "MD5"; "SHA";
        $server['snmp_priv_passphrase'] = ""; //"cacti@123";
        $server['snmp_priv_protocol'] = ""; //"AES128";
        $servers[0] = $server;

        save_host_list($batch_id, $servers);
        import_servers($batch_id);

        //scan_iprange('56bf56f0-a31e-4b8a-a27a-f6fc1954849e', '192.168.test', '192.168.99.10');
        $this->assertEquals(true, true);
    }

    function testGetIbmcDiskInfo() {
        $server = hs_get_server_byid(30);
        $diskI_info = hs_snmp_get_disk_info_i($server);
    }
}
