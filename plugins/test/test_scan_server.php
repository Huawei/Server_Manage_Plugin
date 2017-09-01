<?php
require_once 'PHPUnit/Autoload.php';
require_once '../hwserver/scan_server.php';

/**
 * Created by IntelliJ IDEA.
 * User: Jack Zhang
 * Date: 2017/4/15
 * Time: 13:56
 */
class ScanServerTester extends PHPUnit_Framework_TestCase
{
    function testSaveUpdate() {
        $batch_id = '56bf56f0-a31e-4b8a-a27a-f6fc1954849e'; //uuid();
        $server_list = array("192.168.0.1", "192.168.0.2", "192.168.0.3", "192.168.0.4");
        $result = hs_save_import_server_list($batch_id, $server_list);
        $this->assertEquals($result, 4);

        $result = hs_set_server_ping_status($batch_id, "192.168.0.1", 1, 10);
        $this->assertEquals($result, 1);

        $result = hs_get_import_status($batch_id, "192.168.0.1");
        $this->assertEquals($result['status'], 1);

        $result = hs_get_import_status($batch_id, "192.168.0.2");
        $this->assertEquals($result['status'], 0);

        hs_set_server_ping_status($batch_id, "192.168.0.4", 2, 10);
        $result = hs_get_import_status($batch_id);
        $this->assertEquals(count($result), 4);
    }

    function testScanServer()
    {
        //scan_iprange('56bf56f0-a31e-4b8a-a27a-f6fc1954849e', '192.168.test', '192.168.99.10');
        //scan_iprange('56bf56f0-a31e-4b8a-a27a-f6fc1954849e', '192.168.99.1', '192.168.98.10');
        //scan_iprange('56bf56f0-a31e-4b8a-a27a-f6fc1954849e', '192.168.99.1', '192.168.120.10');
        //scan_iprange('56bf56f0-a31e-4b8a-a27a-f6fc1954849e', '192.168.99.1', '192.168.99.10');
        scan_iprange('56bf56f0-a31e-4b8a-a27a-f6fc1954849e', '192.168.31.1', '192.168.31.10');
        $this->assertEquals(true, true);
    }
}

function uuid() {
    if (function_exists ( 'com_create_guid' )) {
        return com_create_guid ();
    } else {
        mt_srand ( ( double ) microtime () * 10000 ); //optional for php 4.2.0 and up.随便数播种，4.2.0以后不需要了。
        $charid = strtoupper ( md5 ( uniqid ( rand (), true ) ) ); //根据当前时间（微秒计）生成唯一id.
        $hyphen = chr ( 45 ); // "-"
        $uuid = '' . //chr(123)// "{"
            substr ( $charid, 0, 8 ) . $hyphen . substr ( $charid, 8, 4 ) . $hyphen . substr ( $charid, 12, 4 ) . $hyphen . substr ( $charid, 16, 4 ) . $hyphen . substr ( $charid, 20, 12 );
        //.chr(125);// "}"
        return $uuid;
    }
}