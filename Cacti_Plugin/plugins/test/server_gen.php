<?php
chdir('../../');
include_once("./include/global.php");
include_once("./include/config.php");
include_once("./lib/snmp.php");
include_once("./plugins/hwserver/includes/oid_tables.php");
include_once("./plugins/hwserver/includes/key_labels.php");
include_once("./plugins/hwserver/includes/snmp.php");
include_once("./plugins/hwserver/includes/functions.php");
include_once("./plugins/hwserver/lib/safe_input.php");

$action = safe_get_request("action");
$host_id = safe_get_request("host_id");

switch ($action) {
    case "get_info":
        $host_info = hs_get_server_byid($host_id);
        if (empty($host_info))
            die("get host info failed!");

        file_put_contents('./log/snmp_get.txt', "========== START " .$host_info["ip_address"]. " ==========".PHP_EOL , FILE_APPEND | LOCK_EX);

        $server_type = 0;
        $oid_key = "";
        if ($host_info["hw_is_ibmc"] == 1) {
            $server_info = hs_snmp_get_ibmc_info($host_info, null);
            $server_type = 1;
            $oid_key = "ibmc";
        } elseif ($host_info["hw_is_hmm"] == 1) {
            $server_info = hs_snmp_get_hmm_info($host_info, null, null);
            $server_type = 2;
            $oid_key = "hmm";
        } else {
            die("The server you selected is not a Huawei server!");
        }

        file_put_contents('./log/snmp_get.txt', "========== END " .$host_info["ip_address"]. " ==========".PHP_EOL , FILE_APPEND | LOCK_EX);

        if (empty($server_info)) {
            die("snmp get server info failed!");
        }

        $file = "./log/" .str_replace(".", "-", $host_info["ip_address"]). "_" .$server_type. ".txt";
        file_put_contents($file, json_encode($server_info));
        print("<h1>SUCCESS!</h1>");
        print("<a href=\"server_gen.php\">Go Back</a>");
        break;

    default:
        form();
        break;
}

function form() {
    $result = hs_get_server_list("",
        -1,
        -1,
        "hostname",
        "",
        1,
        10000);

    $total_rows = $result["total"];
    $server_list = $result["server_list"];

    print("<form method=\"GET\" action=\"server_gen.php\">");
    html_start_box("", "100%", "", "3", "center", "");
    print("<tr><td>Please Choose one Server: <select name=\"host_id\" id=\"host_id\">");
    foreach ($server_list as $server) {
        print("<option value=\"" .$server["host_id"]. "\">" .$server["ip_address"]. " ("  .$server["hostname"]. ")</option>");
    }

    print("</select></td></tr>");
    html_end_box(true);

    print("<input type=\"submit\" value=\"Get Server Info\">");
    print("<input type=\"hidden\" id=\"action\" name=\"action\" value=\"get_info\">");
    print("<form>");
}
?>

