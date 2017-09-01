<?php
/***************************************************************************
 * Author  ......... Jack Zhang
 * Version ......... 1.0
 * History ......... 2017/3/27 Created
 * Purpose ......... Auto scan Huawei server in given subnet
 ***************************************************************************/

chdir('../../');
include_once("./include/auth.php");
include_once("./include/config.php");
include_once("./lib/poller.php");
include_once("./plugins/hwserver/includes/functions.php");
include_once("./plugins/hwserver/includes/batch_form.php");
include_once("./plugins/hwserver/lib/safe_input.php");

$action = safe_get_request('action');
switch ($action) {
    case 'scan':
        //validate form input
        $batch_id = get_batch_id();
        $start_ip = safe_get_request('start_ip');
        $end_ip = safe_get_request('end_ip');
        if (!($start_ip && $end_ip)) {
            hs_ajax_result(false, "BAD input!", null);
        }

        //Now scan
        scan_iprange($batch_id, $start_ip, $end_ip);
        break;

    case 'scan_progress':
        $batch_id = get_batch_id();
        $server_list = hs_get_import_status($batch_id);
        $progress = 0;
        foreach($server_list as $server) {
            if ($server['status'] != 0)
                $progress++;
        }

        $scan_result = array();
        $scan_result['progress'] = $progress;
        $scan_result['total'] = count($server_list);
        hs_ajax_result(true, "", $scan_result);
        break;

    case 'server_list':
        $batch_id = get_batch_id();
        $server_list = hs_get_import_server_list($batch_id);
        hs_ajax_result(true, "", $server_list);
        break;

    default:
        hs_render_header();
        scan_server_form();
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

/**
 * scan all live ip address in given range
 */
function scan_iprange($batch_id, $start_ip, $end_ip) {
    global $config;

    //validate input
    $pattern = '/\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b/';
    if (!preg_match($pattern, $start_ip))
        hs_ajax_result(false, "BAD start ip.", null);
    if (!preg_match($pattern, $end_ip))
        hs_ajax_result(false, "BAD start ip.", null);

    //check ip address range
    $ip_list = explode( '.', $start_ip, 4 );
    unset($ip_list[3]);
    $ip_range_start = bindec(decbin(ip2long(join('.', $ip_list ) . '.1')));
    $ip_range_end = bindec(decbin(ip2long(join('.', $ip_list ) . '.255')));
    $ip_end_long = bindec(decbin(ip2long($end_ip)));

    if ($ip_end_long < $ip_range_start || $ip_end_long > $ip_range_end) {
        hs_ajax_result(false, "BAD ip address, start & end ip address must in same subnet.", null);
    }

    //first save to database with init status
    $start_ip_num = explode( '.', $start_ip, 4)[3];
    $end_ip_num = explode( '.', $end_ip, 4)[3];
    $server_list = array();
    for ($i=$start_ip_num; $i<=$end_ip_num; $i++) {
        array_push($server_list, join('.', $ip_list) . '.' . $i);
    }

    if (count($server_list) == 0) {
        hs_ajax_result(false, "BAD ip address range.", null);
    }

    //save to database, status 0
    hs_save_import_server_list($batch_id, $server_list);

    //ping server in background
    $debug_str = ""; //" -dxdebug.remote_enable=1 -dxdebug.remote_mode=req -dxdebug.remote_port=9000 -dxdebug.remote_host=192.168.31.240 ";
    $command_string = read_config_option("path_php_binary");
    exec_background($command_string, $debug_str. "\"" . $config["base_path"] . "/plugins/hwserver/includes/jobs.php\" --action=scan_server --batch_id=" .$batch_id);
    hs_ajax_result(true, "please waiting scan finish", null);
}

/**
 * render web form
 */
function scan_server_form()
{
    global $config;

    html_start_box("<strong>Auto Scan Huawei Server</strong>", "100%", "", "3", "center", "");
    ?>
    <tr class="even noprint">
        <td width="100" nowrap style="padding: 10px;">
            IP range Start:
        </td>
        <td width="1" nowrap>
            <input id="startIp" type="text" placeholder="example: 192.168.1.1" required
                   style="padding-left: 5px;">
        </td>
        <td width="50" nowrap style="padding-left: 15px;">
            End:
        </td>
        <td width="1" nowrap>
            <input id="endIp" type="text" placeholder="example: 192.168.1.5" required
                   style="padding-left: 5px;">
        </td>
        <td nowrap>
            <input id="btnStartScan" type="button" value="Start Scan">
        </td>
    </tr>
    <tr class="odd noprint">
        <td width="100">
        </td>
        <td colspan="4">
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
    $empty_message = "To discover servers in a subnet, input Start and End IP address, click \"Start Scan\".";

    html_end_box();
    server_batch_form("Scan Server Result", $empty_message);

    $tokens = csrf_get_tokens();
    $name = $GLOBALS['csrf']['input-name'];
    ?>

    <div id="scanDialog" title="Scanning is in progress, please wait...">
        <div class="progress-label"></div>
        <div class="progressbar" style="margin-top: 20px;"></div>
    </div>

    <script type="text/javascript" src="<?php echo $config['url_path']; ?>plugins/hwserver/static/custom-alert.js"></script>
    <script>
        $(function () {
            var scanButton = $("#btnStartScan");
            scanButton.button().on("click", function () {
                var startIp = $('#startIp').val();
                var endIp = $('#endIp').val();

                if (!(startIp && startIp.match(/\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b/)))
                    return alert('Please input valid start IP address!');
                if (!(endIp && endIp.match(/\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b/)))
                    return alert('Please input valid end IP address!');

                var serverScaner = new ServerScaner("scanDialog",
                    function(){
                        scanButton.button("option", {
                            disabled: true,
                            label: "Scanning..."
                        });
                    },
                    function() {
                        scanButton.button("option", {
                            disabled: false,
                            label: "Start Scan"
                        });
                        scanButton.trigger("focus");
                    });

                serverScaner.startScanning(startIp, endIp);
            });
        });

        function ServerScaner(dialogId, onStart, onStop) {
            var _this = this;
            this.onStart = onStart;
            this.onStop = onStop;
            this.batchId = undefined;

            this.progressTimer = undefined;
            this.progressbar = $("#"+dialogId+" .progressbar");
            this.progressLabel = $("#"+dialogId+" .progress-label");

            this.scanDialog = $("#" + dialogId).dialog({
                autoOpen: false,
                modal: true,
                closeOnEscape: false,
                resizable: false,
                beforeClose: function () {
                    if (_this.onStop) _this.onStop();
                }
            });

            this.startScanning = function(startIp, endIp) {
                if(_this.onStart) _this.onStart();

                //submit ajax request to scan all ip address
                var s4 = _this._s4;
                _this.batchId = (s4() + s4() + "-" + s4() + "-" + s4() + "-" + s4() + "-" + s4() + s4() + s4());
                $.ajax({
                    url: 'scan_server.php',
                    type: 'POST',
                    data: {
                        <?php echo $name ?> : '<?php echo $tokens ?>',
                        action: 'scan',
                        batch_id: _this.batchId,
                        start_ip: startIp,
                        end_ip: endIp
                    },
                    dataType: 'json',
                    success: function (data, textStatus, jqXHR) {
                        if (!data.success) {
                            _this.stopScanning();
                            alert(data.message);
                        }
                    },
                    error: function (xhr, textStatus) {
                        _this.stopScanning();
                        alert("Unexpected error, please check your network!");
                    }
                })

                _this.scanDialog.dialog("option", "buttons", [{
                    text: "Cancel",
                    click: function() { _this.stopScanning(); }
                }]);

                _this.scanDialog.dialog("open");
                _this.progressTimer = setInterval(_this.scanProgress, 3000);
            }

            this.scanProgress = function() {
                $.ajax({
                    url:'scan_server.php',
                    type:'POST',
                    data:{
                        <?php echo $name ?> : '<?php echo $tokens ?>',
                        action: 'scan_progress',
                        batch_id: _this.batchId
                    },
                    dataType:'json',
                    success:function(data,textStatus,jqXHR){
                        if (!data.success)
                            return alert("get scan process error: " + data.message);

                        var progress = Math.floor((data.data.progress * 100) / data.data.total);
                        _this.progressbar.progressbar("value", progress);
                        if (data.data.progress >= data.data.total) {
                            clearInterval(_this.progressTimer);
                            _this.getScanResult(_this.batchId);
                        }
                    },
                    error:function(xhr,textStatus){
                        alert("Unexpected error, please check your network!");
                        _this.stopScanning();
                    }
                })
            }

            this.stopScanning = function() {
                if (_this.progressTimer)
                    clearInterval(_this.progressTimer);

                _this.scanDialog.dialog("close");
                _this.progressbar.progressbar("value", false);
                _this.progressLabel.text("Starting Scan");
            }

            this.getScanResult = function() {
                $.ajax({
                    url: 'scan_server.php',
                    type: 'POST',
                    data: {
                        <?php echo $name ?> : '<?php echo $tokens ?>',
                        action: 'server_list',
                        batch_id: _this.batchId
                    },
                    dataType: 'json',
                    success: function (data, textStatus, jqXHR) {
                        if (!data.success)
                            return alert("get scan result error: " + data.message);
                        batchResultTable(_this.batchId, data.data);
                    },
                    error: function (xhr, textStatus) {
                        alert("Unexpected error, please check your network!");
                    }
                })
            }

            this._s4 = function() {
                return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
            }

            this.progressbar.progressbar({
                value: false,
                change: function () {
                    _this.progressLabel.text("Current Progress: " + _this.progressbar.progressbar("value") + "%");
                },
                complete: function () {
                    _this.progressLabel.text("Complete!");
                    _this.stopScanning();
                    /*
                     scanDialog.dialog("option", "buttons", [{
                        text: "Close",
                        click: stopScanning
                    }]);
                    $(".ui-dialog button").last().trigger("focus");
                    */
                }
            });
        }
    </script>
<?php
}
?>