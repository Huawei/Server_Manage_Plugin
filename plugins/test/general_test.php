<?php
/**
 * Created by IntelliJ IDEA.
 * User: Jack
 * Date: 2017/4/24
 * Time: 9:10
 */

if ("1 " == 1) {
    print("yes");
}

//example "Samsung, 1333 MHz, 8192 MB"
$info_str = "Hynix, 2400 MHz, 16384 MB,DDR4,0x115B83F0,1200 mV,2 rank,72 bits,Synchronous| Registered (Buffered)";
//$info_str = "Hynix, 2400 MHz, 16384 MB";
//$info_str = "Unknown, Unknown,Unknown,DDR4,NO DIMM,Unknown,2 rank,Unknown,Synchronous| Registered (Buffered)";
//$info_str = "Samsung, 2133 MHz, 8192 MB,DDR4,0x314C9D82,1200 mV,1 rank,72 bits,Synchronous| Registered (Buffered)";
$matches = array();
$mem_info = array();

$matches = explode (",", $info_str);
if (sizeof($matches) > 3) {
    $mem_info["mem_manufacturer"] = $matches[1];
    $mem_info["mem_size"] = $matches[3] / 1024;
}

$value = "123 Watt";
if (preg_match("/watt/i", $value, $matches)) {
    $value = preg_replace("/watt/i", "W", $value);
}
print($value);

$a = (int)" 8192 MB";
$b = is_numeric(" 8192 MB");
print($b);
print($a);
$cpu_info = "Intel(R) Corporation,Xeon,3300 MHz";
$matches = array();

if (preg_match("/(.*?),\\s*?(.*?),\\s*?(\\d+).*/i", $cpu_info, $matches)) {
    print($matches);
}

$mem_info = "Samsung, 1333 MHz, 8192 MB";
$matches = array();
if (preg_match("/(.*?),*?(\\d+).*?,*?(\\d+).*/i", $mem_info, $matches)) {
    print($matches);
}

$ip_list = array();
array_push($ip_list, "192.168.0.1");
array_push($ip_list, "192.168.0.2");
foreach($ip_list as &$ip) {
    $ip = "123";
}

$s = join("','", $ip_list);
print(s);

$ip = "he  Straight  BMC  IP:  192.168.100.14,  the  Straight BMC Mask: 255.255.255.0";
$matches = array();
if(preg_match("/IP: (.*),.*Mask: (.*)/i", $ip, $matches)){
    var_dump($matches);
}

$s = " 16384 MB ";
$i = (int)$s;
print($i);

$guid = uuid();
print($guid);

function uuid() {
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