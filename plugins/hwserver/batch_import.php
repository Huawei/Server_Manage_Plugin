<?php
/***************************************************************************
 * Author  ......... Jack Zhang
 * Version ......... 1.0
 * History ......... 2017/3/27 Created
 * Purpose ......... Web page for batch import Huawei servers from csv template.
 ***************************************************************************/

chdir('../../');
include_once("./include/auth.php");
include_once("./include/config.php");
include_once("./lib/poller.php");
include_once("./lib/api_device.php");
include_once('./lib/api_data_source.php');
include_once("./plugins/hwserver/includes/functions.php");
include_once("./plugins/hwserver/includes/batch_form.php");
include_once("./plugins/hwserver/lib/safe_input.php");

$error = null;
$action = safe_get_request('action');
switch ($action) {
    case "upload":
        if (!(isset($_FILES) && isset($_FILES['batch_csv']))) {
            hs_ajax_result(false, "Please select the .csv file to upload.", null);
        }

        $whitelist = array("csv");
        $tmp_name = $_FILES['batch_csv']['tmp_name'];
        $name = basename($_FILES['batch_csv']['name']);
        $error = $_FILES['batch_csv']['error'];

        if ($error != UPLOAD_ERR_OK) {
            hs_ajax_result(false, "" .$error, null);
        }

        $extension = pathinfo($name, PATHINFO_EXTENSION);
        if (!in_array($extension, $whitelist)) {
            hs_ajax_result(false, "Invalid file type uploaded.", null);
        }

        //also generate batch_id at this step
        $result = parse_uploaded_csv($tmp_name);
        if (!is_array($result)) {
            hs_ajax_result(false, "No server found from uploaded csv file.", null);
        }

        hs_ajax_result(true, "success", $result);
        break;

    case "import":
        //validate form input
        $batch_id = get_batch_id();
        $overwrite_exist = safe_get_request('overwrite_exist', 0);
        $servers = $_REQUEST['servers'];
        if (!isset($_REQUEST['servers']) || count($servers) == 0) {
            hs_ajax_result(false, "invalid request params: servers", null);
        }

        //Now import server
        $server_ip_list = array();
        foreach ($servers as $server) {
            array_push($server_ip_list, $server["ip_address"]);
        }

        //save all to be imported servers
        hs_save_import_server_list($batch_id, $server_ip_list);

        //check skip exists
        $exist_hosts = hs_get_cacti_host_list_byips($server_ip_list);
        if ($overwrite_exist != 1) {
            for ($i=count($servers)-1; $i>=0; $i--) {
                $server = $servers[$i];
                if (array_key_exists($server["ip_address"], $exist_hosts)) {
                    unset($servers[$i]);
                    hs_set_server_import_status($batch_id, $server, 5); //5: skipped
                }
            }
        }

        if (count($servers) == 0) {
            hs_ajax_result(false, "", "0");
        }

        //save server to host table, to get the host_id
        //  also update import status to 0 in this step
        //  otherwise will not import
        save_host_list($batch_id, $servers, $exist_hosts);

        //get server information in background
        $debug_str = " -dxdebug.remote_enable=1 -dxdebug.remote_mode=req -dxdebug.remote_port=9000 -dxdebug.remote_host=192.168.63.2 ";
        $command_string = read_config_option("path_php_binary");
        exec_background($command_string, $debug_str. "\"" . $config["base_path"] . "/plugins/hwserver/includes/jobs.php\" --action=import_server --overwrite=" .$overwrite_exist. " --batch_id=" .$batch_id);
        hs_ajax_result(true, "server information still not import!", null);
        break;

    case "import_progress":
        $batch_id = get_batch_id();
        $server_list = hs_get_import_status($batch_id);
        $progress = 0;
        $total = 0;
        foreach($server_list as $server) {
            if ($server['import_status'] == -1)
                continue;
            $total++;
            if ($server['import_status'] > 0 && $server['import_status'] !=999)
                $progress++;
        }

        $import_result = array();
        $import_result['progress'] = $progress;
        $import_result['total'] = $total;
        hs_ajax_result(true, "", $import_result);
        break;

    default:
        hs_render_header();
        batch_upload_form();
        hs_render_footer();
        break;
}

/**
 * get batch_id param from request form
 */
function get_batch_id() {
    global $_REQUEST;
    if (!(isset($_REQUEST['batch_id'])
        && is_string($_REQUEST['batch_id'])
        && preg_match('/^\{?[a-z0-9]{8}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{12}\}?$/i', $_REQUEST['batch_id']))) {
        hs_ajax_result(false, "hack attach!", null);
    }

    return $_REQUEST['batch_id'];
}

function parse_uploaded_csv($name) {
    $server_list = array();
    $row = 0;
    if (($handle = fopen($name, "r")) !== FALSE) {
        //first two lines are not we needed
        $data = fgetcsv($handle);
        $data = fgetcsv($handle);
        if ($data[0] != "ip" || $data[1] != "snmp_ver")
            return null;

        $batch_id = hs_uuid();
        while (($data = fgetcsv($handle)) !== FALSE) {
            $num = count($data);
            if ($num < 3)
                continue;
            //ip,snmp_ver,community,user_name,sec_level,auth_protocol,auth_passphrase,priv_protocol,priv_passphrase
            $server = array();
            //$server["batch_id"] = $batch_id;
            $server["ip_address"] = $data[0];
            $server["snmp_version"] = $data[1];
            $server["snmp_community"] = $data[2];
            if ($num > 3)
                $server["snmp_username"] = $data[3];
            if ($num > 5)
                $server["snmp_auth_protocol"] = $data[5];
            if ($num > 6)
                $server["snmp_password"] = $data[6];
            if ($num > 7)
                $server["snmp_priv_protocol"] = $data[7];
            if ($num > 8)
                $server["snmp_priv_passphrase"] = $data[8];
            $server["snmp_port"] = 161;
            $server["snmp_timeout"] = 500;
            $server_list[$row] = $server;
            $row++;
        }

        fclose($handle);
    }

    if(count($server_list) == 0)
        return null;

    $result = array("batch_id" => $batch_id, "result" => $server_list);
    return $result;
}

/**
 * save servers to host table, init with disabled status
 */
function save_host_list($batch_id, &$server_list, &$exist_hosts) {
    global $config;

    $result = array();
    foreach($server_list as $server) {
        $is_v3 = $server["snmp_version"] == 3 ? true : false;
        if (array_key_exists($server["ip_address"], $exist_hosts)) {
            $host_id = $exist_hosts[$server["ip_address"]]["id"];

            $sql = "UPDATE host SET "
                . "host_template_id=0,"
                . "description='Huawei Server - " . $server['ip_address'] . "',"
                . "snmp_community='" . $server['snmp_community'] . "',"
                . "snmp_version=" . $server["snmp_version"] . ","
                . "snmp_username='" . ($is_v3 ? $server['snmp_username'] : '') . "',"
                . "snmp_password='" . ($is_v3 ? $server['snmp_password'] : '') . "',"
                . "snmp_auth_protocol='" . ($is_v3 ? $server['snmp_auth_protocol'] : '') . "',"
                . "snmp_priv_passphrase='" . ($is_v3 ? $server['snmp_priv_passphrase'] : '') . "',"
                . "snmp_priv_protocol='" . ($is_v3 ? $server['snmp_priv_protocol'] : '') . "',"
                . "snmp_context='',"
                . "snmp_port=" . (isset($server["snmp_port"]) ? $server["snmp_port"] : 161) . ","
                . "snmp_timeout=5000"
                . " WHERE id=" . $host_id;
            if (db_execute($sql) == 0) {
                $host_id = -1;
            }
        }
        else
        {
            if (strpos($config['cacti_version'], "0.") === 0) {
                $host_id = api_device_save(
                    0 /*id*/,
                    0 /*host template id*/,
                    "Huawei Server - " . $server['ip_address'],
                    $server['ip_address'],
                    $server['snmp_community'],
                    $server["snmp_version"],
                    ($is_v3 ? $server['snmp_username'] : ''),
                    ($is_v3 ? $server['snmp_password'] : ''),
                    (isset($server["snmp_port"]) ? $server["snmp_port"] : 161),
                    5000 /* snmp timeout */,
                    "on" /* init with disabled*/,
                    2 /* availability_method */,
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
                    2 /* device_threads */);
            } else {
                $host_id = api_device_save(
                    '0' /*id*/,
                    1 /*host template id*/,
                    "Huawei Server - " . $server['ip_address'],
                    $server['ip_address'],
                    $server['snmp_community'],
                    $server["snmp_version"],
                    ($is_v3 ? $server['snmp_username'] : ''),
                    ($is_v3 ? $server['snmp_password'] : ''),
                    (isset($server["snmp_port"]) ? $server["snmp_port"] : 161),
                    5000 /* snmp timeout */,
                    "on" /* init with disabled*/,
                    2 /* availability_method */,
                    2 /* ping_method */,
                    23 /* ping_port */,
                    400 /* ping_timeout */,
                    1 /* ping_retries */,
                    "" /* notes */,
                    ($is_v3 ? $server['snmp_auth_protocol'] : ''),
                    ($is_v3 ? $server['snmp_priv_passphrase'] : ''),
                    ($is_v3 ? $server['snmp_priv_protocol'] : ''),
                    "" /* snmp_context */,
                    "" /* engine id */,
                    10 /* max_oids */,
                    2 /* device_threads */);
            }
        }

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

/**
 * render web form
 */
function batch_upload_form()
{
    global $config;

    print("<form method=\"post\" action=\"batch_import.php?action=upload\" enctype=\"multipart/form-data\">");
    html_start_box("<strong>Batch Import Servers</strong>","100%","","3","center","");
    ?>
    <tr class="even noprint">
        <td width="100" nowrap style="padding: 10px;">
            Choose file
        </td>
        <td width="350" nowrap>
            <input type="file" id="batch_csv" name="batch_csv" style="width:350px;padding-left: 5px;">
            <div id="uploads">
            </div>
        </td>
        <td nowrap>
            <!--<input type="submit" value="Upload...">-->
            <a href="<?php echo $config['url_path']; ?>plugins/hwserver/static/batch-import-template-v3.csv">
                Download Batch Import Template</a>
        </td>
    </tr>
    <tr class="odd noprint">
        <td width="100">
        </td>
        <td colspan="2">
            <table cellspacing="0">
                <tr>
                    <td width="1">
                        <input type="checkbox" id="overwrite_exist" value="1" title="Overwrite exist item(s)">
                    </td>
                    <td>
                        <label for="overwrite_exist">Overwrite exist item(s)</label>
                    </td>
                </tr>
            </table>
        </td>
    </tr>

    <?php
    $empty_message = "To start batch upload, Choose your .csv file, then click \"Upload\" button.";

    html_end_box();
    print("</form>");

    server_batch_form("File Parse Result", $empty_message, false);
    ?>

    <script type="text/javascript" src="<?php echo $config['url_path']; ?>plugins/hwserver/static/custom-alert.js"></script>
    <script type="text/javascript" src="<?php echo $config['url_path']; ?>plugins/hwserver/static/jquery.ajaxfileupload.js"></script>
    <script type="text/javascript">
        $(function () {
            $("#batch_csv").AjaxFileUpload({
                action: "batch_import.php?action=upload",
                onComplete: function(filename, response) {
                    if (!response.success)
                        return alert("upload file error: " + response.message);
                    batchResultTable(response.data.batch_id, response.data.result);
                },
                onSubmit: function(filename) {
                    <?php
                    $tokens = csrf_get_tokens();
                    $name = $GLOBALS['csrf']['input-name'];
                    $endslash = $GLOBALS['csrf']['xhtml'] ? ' /' : '';
                    print('return {"' .$name. '":"' .$tokens. '"}');
                    ?>
                }
            });
        });
    </script>
<?php
}
?>