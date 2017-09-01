<?php
/***************************************************************************
 * Author  ......... Jack Zhang
 * Version ......... 1.0
 * History ......... 2017/4/27 Created
 * Purpose ......... Batch import server result page
 ***************************************************************************/

chdir('../../');
include_once("./include/auth.php");
include_once("./include/config.php");
include_once("./lib/snmp.php");
include_once("./plugins/hwserver/includes/oid_tables.php");
include_once("./plugins/hwserver/includes/key_labels.php");
include_once("./plugins/hwserver/includes/snmp.php");
include_once("./plugins/hwserver/includes/functions.php");
include_once("./plugins/hwserver/lib/safe_input.php");

$batch_id = safe_get_request('batch_id');
if (!(isset($batch_id)
    && is_string($batch_id)
    && preg_match('/^\{?[a-z0-9]{8}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{12}\}?$/i', $batch_id))) {
    die("BAD request!");
}

hs_render_header();
render_import_result($batch_id);
hs_render_footer();


function render_import_result($batch_id) {
    page_title();
    html_start_box("", "100%", "", "3", "center", "", "");
    render_table_header();

    $i = 0;
    $host_list = hs_get_import_server_list($batch_id, false);
    $class = array("even-alternate", "odd");

    foreach($host_list as $host) {
        if ($host["import_status"] == -1)
            continue;
        render_host_result($host, $class[$i % 2]);
        $i++;
    }

    html_end_box();
    render_return_box();
}

function render_table_header() {
    ?>
    <tr class="tableHeader">
        <td style="padding:5px;color:#fff;"><strong>IP Address</strong></td>
        <td style="padding:5px;color:#fff;"><strong>Import Status</strong></td>
    </tr>
    <?php
}

function render_host_result($host, $class) {
    $import_result = "Unknown";

    switch((int)$host["import_status"]) {
        case -1:
            $import_result = "<span data=\"1\">User not select</span>";
            break;
        case 0:
            $import_result = "<span data=\"0\">Not Start</span>";
            break;
        case 1:
            $import_result = "<span data=\"1\" class=\"import_success\">Import success</span>";
            break;
        case 2:
            $import_result = "<span data=\"2\" class=\"import_fail\">Save database error</span>";
            break;
        case 3:
            $import_result = "<span data=\"3\" class=\"import_fail\">SNMP connect failed</span>";
            break;
        case 4:
            $import_result = "<span data=\"4\" class=\"import_fail\">SNMP read data error</span>";
            break;
        case 5:
            $import_result = "<span data=\"5\" >Server already exists, you have choose skipped</span>";
            break;
        case 99:
            $import_result = "<span data=\"99\" class=\"import_fail\">Unknown error</span>";
            break;
    }
    ?>
    <tr class="<?php echo $class ?> selectable">
        <td align="left" width="20%" style="padding:5px;padding-left:30px;white-space: nowrap;"><?php echo $host["ip_address"] ?></td>
        <td>
            <?php echo $import_result ?>
        </td>
    </tr>
    <?php
}

function page_title($title=null) {
    ?>
    <table width="100%" align="center" style="margin-bottom: 10px;">
        <tr>
            <td class="textInfo">
                Import Servers Result
            </td>
        </tr>
    </table>
    <?php
}

function render_return_box() {
    ?>
    <table align="center" width="100%">
        <tbody>
        <tr>
            <td align="right">
                <input type="button" value="Return" onClick="window.location.href='server_list.php'">
            </td>
        </tr>
        </tbody>
    </table>
    <br/>
    &nbsp;
    <?php
}
?>

<style>
    .import_success {
        color: green;
    }

    .import_fail {
        color: red;
    }
</style>

