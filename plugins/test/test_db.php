<?php
require_once 'PHPUnit/Autoload.php';

chdir('../../');
include_once("./include/auth.php");
include_once("./include/config.php");
include_once('./lib/plugins.php');
include_once('./plugins/hwserver/includes/database.php');

/**
 * Created by IntelliJ IDEA.
 * User: Jack Zhang
 * Date: 2017/4/15
 * Time: 13:56
 */
class DatabaseTester extends PHPUnit_Framework_TestCase
{
    function testCreateDatabase()
    {
        hwserver_setup_database();
        $this->assertEquals(true);
    }
}
