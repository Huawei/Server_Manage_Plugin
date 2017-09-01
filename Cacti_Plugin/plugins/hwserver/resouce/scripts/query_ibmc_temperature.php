<?php

/* do NOT run this script through a web browser */
if (!isset($_SERVER["argv"][0]) || isset($_SERVER['REQUEST_METHOD'])  || isset($_SERVER['REMOTE_ADDR'])) {
   die("<br><strong>This script is only meant to run at the command line.</strong>");
}

$no_http_headers = true;

include_once(dirname(__FILE__) . "/../../../../include/global.php");
include_once(dirname(__FILE__) . "/../../../../lib/snmp.php");

global $temperatrue_index_oid, $temperatrue_obj_oid, $temperatrue_oid;

$temperatrue_index_oid = "1.3.6.1.4.1.2011.2.235.1.1.26.50.1.1";
$temperatrue_obj_oid = "1.3.6.1.4.1.2011.2.235.1.1.26.50.1.2";
$temperatrue_oid = "1.3.6.1.4.1.2011.2.235.1.1.26.50.1.3";

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
	$indeies = cacti_snmp_walk($hostname, $snmp_community, $temperatrue_index_oid, $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries, 10, SNMP_POLLER);
	foreach ($indeies as $index) {
		$oid = $index["oid"];
		$index_val = $index["value"];

		$object_name = cacti_snmp_get($hostname, $snmp_community, $temperatrue_obj_oid.".".$index_val, $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries, SNMP_POLLER);
		if ($object_name == "Inlet" || $object_name == "inlet") {
			print $index_val . "\n";
			break;
		}
	}

/*
 * process NUM_INDEXES requests, only need one innet temperature
 */
} elseif ($cmd == "num_indexes") {
	print "1\n";
	
/*
 * process QUERY requests
 */
} elseif ($cmd == "query") {
	$arg = $_SERVER["argv"][5];

	$index_val = '';
	$find = false;
	$indeies = cacti_snmp_walk($hostname, $snmp_community, $temperatrue_index_oid, $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries, 10, SNMP_POLLER);
	foreach ($indeies as $index) {
		$oid = $index["oid"];
		$index_val = $index["value"];

		$object_name = cacti_snmp_get($hostname, $snmp_community, $temperatrue_obj_oid.".".$index_val, $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries, SNMP_POLLER);
		if ($object_name == "Inlet" || $object_name == "inlet") {
			$find = true;
			break;
		}
	}

	if (!$find) {
		$index_val = "1";
	}

	if ($arg == "temperature") {
		$temperature_val = cacti_snmp_get($hostname, $snmp_community, $temperatrue_oid.".".$index_val, $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries, SNMP_POLLER);
		if ($temperature_val > 1000 || $temperature_val < 0)
			$temperature_val = 'U';
		if (is_numeric($temperature_val))
			$temperature_val = $temperature_val / 10;

		print $index_val . "!" . $temperature_val . "\n";
	} elseif ($arg == "index") {
		print $index_val . "!" . $index_val . "\n";
	}

} elseif ($cmd == "get") {
	$arg = $_SERVER["argv"][5];
	$index_val = $_SERVER["argv"][6];

	$temperature_val = cacti_snmp_get($hostname, $snmp_community, $temperatrue_oid.".".$index_val, $snmp_version, $snmp_auth_username, $snmp_auth_password, $snmp_auth_protocol, $snmp_priv_passphrase, $snmp_priv_protocol, $snmp_context, $snmp_port, $snmp_timeout, $ping_retries, SNMP_POLLER);
	if ($temperature_val > 1000 || $temperature_val < 0)
		$temperature_val = 'U';
	if (is_numeric($temperature_val))
		$temperature_val = $temperature_val / 10;

	print($temperature_val);
}

?>
