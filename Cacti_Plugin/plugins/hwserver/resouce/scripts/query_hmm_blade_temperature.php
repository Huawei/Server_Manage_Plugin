<?php

/* do NOT run this script through a web browser */
if (!isset($_SERVER["argv"][0]) || isset($_SERVER['REQUEST_METHOD'])  || isset($_SERVER['REMOTE_ADDR'])) {
   die("<br><strong>This script is only meant to run at the command line.</strong>");
}

$no_http_headers = true;

include_once(dirname(__FILE__) . "/../../../../include/global.php");
include_once(dirname(__FILE__) . "/../../../../lib/snmp.php");
include_once(dirname(__FILE__) . "/../../../../lib/database.php");

global $temperatrue_oid;
$temperatrue_oid = "1.3.6.1.4.1.2011.2.82.1.82.4.#.2012.1.2.1";


$hostname 	= $_SERVER["argv"][1];
$host_id 	= $_SERVER["argv"][2];
$snmp_auth 	= $_SERVER["argv"][3];
$cmd 		= $_SERVER["argv"][4];

/* support for SNMP V2 and SNMP V3 parameters */
$snmp = explode(":", $snmp_auth);
$snmp_version 	= $snmp[0];
$snmp_port    	= $snmp[1];
$snmp_timeout 	= $snmp[2];
$ping_retries 	= $snmp[3];
$max_oids		= $snmp[4];

$snmp_auth_username   	= "";
$snmp_auth_password   	= "";
$snmp_auth_protocol  	= "";
$snmp_priv_passphrase 	= "";
$snmp_priv_protocol   	= "";
$snmp_context         	= "";
$snmp_community 		= "";

if ($snmp_version == 3) {
	$snmp_auth_username   = $snmp[6];
	$snmp_auth_password   = $snmp[7];
	$snmp_auth_protocol   = $snmp[8];
	$snmp_priv_passphrase = $snmp[9];
	$snmp_priv_protocol   = $snmp[10];
	$snmp_context         = $snmp[11];
}else{
	$snmp_community = $snmp[5];
}

/*
 * process INDEX requests
 */
if ($cmd == "index") {
	$total_rows = db_fetch_assoc("SELECT DISTINCT blade FROM `huawei_server_detail` WHERE host_id=$host_id and blade > -1 ORDER BY blade");
	if (empty($total_rows)) {
		$total_rows = snmp_get_blade_list($hostname, $snmp_community, $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries);
	}
	foreach ($total_rows as $row) {
		print $row["blade"] . "\n";
	}

/*
 * process NUM_INDEXES requests
 */
} elseif ($cmd == "num_indexes") {
	$total_rows = db_fetch_cell("SELECT COUNT(DISTINCT blade) FROM `huawei_server_detail` WHERE host_id=$host_id and blade > -1");
	if (empty($total_rows)) {
		$all_rows = snmp_get_blade_list($hostname, $snmp_community, $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries);
		$total_rows = sizeof($all_rows);
	}

	print $total_rows . "\n";
	
/*
 * process QUERY requests
 */
} elseif ($cmd == "query") {
	$arg = $_SERVER["argv"][5];

	$arr_blades = array();
	$rows = db_fetch_assoc("SELECT DISTINCT blade FROM `huawei_server_detail` WHERE host_id=$host_id and blade > -1 ORDER BY blade");
	if (empty($rows)) {
		$rows = snmp_get_blade_list($hostname, $snmp_community, $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries);
	}
	foreach($rows as $row) {
		array_push($arr_blades, $row["blade"]);
	}
	$arr = get_blade_temperature($arr_blades, $hostname, $snmp_community, $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries, $max_oids);

	for ($i=0;($i<sizeof($arr_blades)); $i++) {
		if ($arg == "temperature") {
			print $arr_blades[$i] . "!" . $arr[$i] . "\n";
		} elseif ($arg == "index") {
			print $arr_blades[$i] . "!" . $arr_blades[$i] . "\n";
		}
	}

} elseif ($cmd == "get") {
	$arg = $_SERVER["argv"][5];
	$index = $_SERVER["argv"][6];

	$temperatrue_oid = str_replace("#", $index, $temperatrue_oid);
	$temperature = cacti_snmp_get($hostname, $snmp_community, $temperatrue_oid, $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries, SNMP_POLLER);
	if ($temperature > 1000 || $temperature < 0)
		$temperature = 'U';
	//if(empty($temperature) || $temperature == 'U')
	//	$temperature = 'U';

	print($temperature);
}

function get_blade_temperature(&$arr_blades, $hostname, $snmp_community, $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries, $max_oids) {
	global $temperatrue_oid;
	$return_arr = array();

	foreach ($arr_blades as $blade) {
		$temperatrue_oid = str_replace("#", $blade, $temperatrue_oid);
		$temperature = cacti_snmp_get($hostname, $snmp_community, $temperatrue_oid, $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries, SNMP_POLLER);
		if ($temperature > 1000 || $temperature < 0)
			$temperature = 'U';
		//$return_arr[$blade] = $temperature;
		//if(empty($temperature))
		//	$temperature = rand(0,100); //'U';
		array_push($return_arr, $temperature);
	}

	return $return_arr;
}

function snmp_get_blade_list($hostname, $snmp_community, $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries) {
	$return_arr = array();

	$blade_oid = "1.3.6.1.4.1.2011.2.82.1.82.4.#.6.0";
	for ($index = 1; $index <= 32; $index++) {
		$blade_presence = cacti_snmp_get($hostname, $snmp_community, str_replace("#", $index . "", $blade_oid), $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries, SNMP_POLLER);
		if ($blade_presence == 0) //not presence
			continue;
		array_push($return_arr, array("blade" => $index));
	}

	return $return_arr;
}
?>
