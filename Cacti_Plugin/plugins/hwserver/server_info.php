<?php
/***************************************************************************
 * Author  ......... Ni Peng
 * Version ......... 1.0
 * History ......... 2017/3/27 Created
 * Purpose ......... View Huawei servers list, include iBMC and HMM
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

if (!defined("MAX_DISPLAY_PAGES")) define("MAX_DISPLAY_PAGES", 30);
define("MAX_SELECTED_SERVER", 5);

$server_info_cat = hs_get_server_info_category();
$action = safe_get_request('action');
$category_num = safe_get_request('drp_action');
$is_ajax = safe_get_request("is_ajax", 0);

input_validate_input_number($category_num);

switch ($action) {
    case "actions":
        if ($is_ajax == 0) {
            $host_array = array();
            $i = 0;
            while (list($var,$val) = each($_GET)) {
                if (preg_match("/^chk_([0-9]+)$/", $var, $matches)) {
                    input_validate_input_number($matches[1]);
                    $host_array[$i] = $matches[1];
                    $i++;
                }
            }

            return render_default_page();
        }

        form_actions();
        break;

    case "refresh":
        $host_id = safe_get_request('host_id');
        $host = hs_get_server_snmp_info($host_id);

        if (hs_snmp_update_server_info($host_id, $host, null, array($category_num)) == 1) {
            print("<success desc=\"This is a indicator\"/>\n");
            render_server_table($host_id, $host, true, false);
        } else {
            print("refresh server info failed!");
        }
        break;

    default:
        die("BAD request!");
        break;
}

function render_default_page() {
    hs_render_header();

    print("<div id=\"server_info\"></div>");
    refresh_form();
    render_return_box();

    hs_render_footer();
}

function form_actions() {
    global $MAX_SELECTED_SERVER;
    global $server_info_cat, $category_num;

    //input validation
    input_validate_input_regex($category_num, "^([a-zA-Z0-9_]+)$");

    $host_array_str = safe_get_request('host_list');
    if (!isset($host_array_str)) {
        die("BAD request!");
    }

    $host_array = explode(",", trim($host_array_str));
    for ($i=count($host_array)-1; $i>=0; $i--) {
        if (!is_numeric($host_array[$i])) {
            unset($host_array[$i]);
        }
    }

    $err_input = null;
    if (count($host_array) == 0)
        $err_input = "You must select at least one server.";
    elseif (count($host_array) > MAX_SELECTED_SERVER)
        $err_input = "You can select up to " .MAX_SELECTED_SERVER. " servers.";

    if ($err_input) {
        html_start_box("<strong>View " .$server_info_cat[$category_num][1]. " Information</strong>", "60%", "", "3", "center", "");
        print "<tr><td class='even'><span class='textError'>$err_input</span></td></tr>\n";
        print "
        <tr>
			<td colspan='2' align='right' class='saveRow'>
				<input type='button' value='Return' onClick='window.history.back()'>
			</td>
		</tr>";
        html_end_box();
        return;
    }

    print("<success desc=\"This is a indicator\"/>\n");
    page_title();

    $host_list = hs_get_server_list_byids($host_array);

    print("<div id=\"tabs_server\">");
    render_tab($host_list);

    foreach($host_array as $host_id) {
        $host = &$host_list[$host_id];

        //check if need connect snmp fill full detail
        if ($host["import_status"] == 0) {
            hs_snmp_update_server_info($host_id, $host, null, null);
        }

        print("<div id=\"tabs-" .$host_id. "\" style=\"display:\"none;\">");
        render_server_table($host_id, $host, false, true);
        print("</div>");
    }

    print("</div>");
}

function page_title($title=null) {
    if (!$title) {
        global $server_info_cat, $category_num;
        $title = $server_info_cat[$category_num][1] . " Information";
    }
?>
    <table width="100%" align="center" style="margin-bottom: 10px;">
        <tr>
            <td class="textInfo">
                <?php print $title;?>
            </td>
        </tr>
    </table>
<?php
}

function render_tab(&$host_list) {
    print("<ul>");
    foreach($host_list as $host) {
        print("<li><a href=\"#tabs-" .$host["host_id"]. "\">" .$host["ip_address"]. "</a></li>");
    }

    print("</ul>");
}

function render_server_table($host_id, &$host_info, $is_visible, $refresh_status=false) {
    global $server_info_cat, $category_num;

    global $hs_display_lables;
    $server_type = $host_info["hw_is_ibmc"] == 1 ? 1 : 2;
    $is_hmm = ($server_type == 2) ? true : false;

    $err_message = null;
    switch ($category_num) {
        case 1:
            if ($host_info["hw_is_ibmc"] == 1)
                $server_info = hs_get_ibmc_info($host_info, array($category_num), $refresh_status);
            elseif ($host_info["hw_is_hmm"] == 1)
                $server_info = hs_get_hmm_info($host_info, array(-1), array(1), $refresh_status);

            if (empty($server_info)) {
                $err_message = "Get iBMC/HMM info failed, please check if host has been removed.";
            } else {
                for ($index=1; $index<=32; $index++) {
                    $blade_index = "blade" . $index;
                    unset($server_info[$blade_index]);
                }
            }
            break;
        case 2: //mainboard
        case 3: //memory
        case 4: //cpu
        case 5: //disk
        case 6: //power
        case 7: //fan
        case 8: //raidcard
        case 9: //shelf
            $server_info = array();
            hs_get_server_details($server_type, $server_info, $host_id, array(-1), array($category_num), $refresh_status, $host_info);
            break;
    }

    _html_start_box("<strong>" .$host_info["hostname"]. " (" .$host_info["ip_address"]. ")</strong>", "100%", "", "3", "center", $host_id);

    $category = $server_info_cat[$category_num][0];
    if (isset($err_message) || !(isset($server_info) && ($category_num == 1 || array_key_exists($category, $server_info)))) {
        $err_message = "Get " . $server_info_cat[$category_num][1] ." information failed!";
    }

    if (isset($err_message)){
        print("<tr><td>$err_message</td></tr>");
        _html_end_box(false);
        return;
    }

    if ($category_num > 1) {
        render_server_info($hs_display_lables[$server_type][$category], $server_info[$category], $is_hmm);
    } else {
        render_server_info($hs_display_lables[$server_type][$category], $server_info, $is_hmm);
    }

    _html_end_box(false);
}

function render_server_info(&$category_keys, &$server_info, $is_hmm) {
    $class = array("even-alternate", "odd");
    $i = 0;
    foreach($category_keys as $key => $value) {
        if (array_key_exists($key, $server_info)) {
            render_server_info_line($key, $server_info[$key], $class[$i % 2], $is_hmm);
            $i++;
        }
    }

    foreach($server_info as $inst => $values) {
        if (is_array($values)) {
            render_instance_header($inst);
            foreach($category_keys as $key => $value) {
                if (array_key_exists($key, $values)) {
                    render_server_info_line($key, $values[$key], $class[$i % 2], $is_hmm);
                    $i++;
                }
            }
        } //END: if
    }
}

function render_instance_header($instance) {
    global $server_info_cat, $category_num;
    ?>
    <tr class="tableHeader">
        <td colspan="2" style="padding:5px;color:#fff;"><strong><?php echo $server_info_cat[$category_num][1] ?> <?php echo $instance ?></strong></td>
    </tr>
    <?php
}

function render_server_info_line($key, $value, $class, $is_hmm) {
    global $hs_server_lables, $hs_status_keys, $hs_value_keys;
    if (is_numeric($value) && $value == -1)
        $value = "Unknown";
    ?>
    <tr class="<?php echo $class ?> selectable">
        <td align="left" width="20%" style="padding:5px;padding-left:30px;white-space: nowrap;"><strong><?php echo $hs_server_lables[$key] ?></strong></td>
        <td>
            <span>
            <?php
            if (in_array($key, $hs_status_keys)) {
                echo (hs_get_status_html($key, $value, $is_hmm));
            } elseif (in_array($key, $hs_value_keys)) {
                echo (hs_get_value_html($key, $value));
            } else {
                echo (!isset($value) || $value == -1) ? "Unknown" : $value;
            }
            ?>
            </span>
        </td>
    </tr>
    <?php
}

function refresh_form() {
    global $config, $action, $category_num, $host_array;

    $tokens = csrf_get_tokens();
    $name = $GLOBALS['csrf']['input-name'];
    ?>
    <div id="refreshDialog" title="Please wait...">
        <div class="progress-label"></div>
        <div class="progressbar" style="margin-top: 20px;"></div>
    </div>

    <link href="<?php echo $config['url_path']; ?>plugins/hwserver/static/hwserver.css" type="text/css" rel="stylesheet">
    <script type="text/javascript" src="<?php echo $config['url_path']; ?>plugins/hwserver/static/custom-alert.js"></script>
    <script>
        $(function () {
            refreshLoader.initLoad(<?php echo("'$action', '" .join(",", $host_array). "'"); ?>);
        });

        var refreshLoader = new RefreshLoader("refreshDialog", <?php echo $category_num; ?>);
        function refreshServerInfo(hostId) {
            refreshLoader.startRefresh(hostId);
            return false;
        }

        function RefreshLoader(dialogId, drpAction, onStart, onStop) {
            var _this = this;
            this.refreshHostIds = [];
            this.drpAction = drpAction;
            this.isRefresing = false;
            this.finishedCount = 0;

            this.progressbar = $("#"+dialogId+" .progressbar");
            this.refreshDialog = $("#" + dialogId).dialog({
                autoOpen: false,
                modal: true,
                closeOnEscape: false,
                resizable: false,
                beforeClose: function () {
                    if (_this.onStop) _this.onStop();
                }
            });

            this.initLoad = function(action, host_list) {
                if(_this.onStart) _this.onStart();

                if (!_this.isRefresing)
                    _this.refreshDialog.dialog("open");
                _this.isRefresing = true;

                //submit ajax request to scan all ip address
                $.ajax({
                    url: 'server_info.php',
                    type: 'POST',
                    data: {
                        <?php echo $name ?> : '<?php echo $tokens ?>',
                        action: action,
                        drp_action: _this.drpAction,
                        host_list: host_list,
                        is_ajax: 1
                    },
                    dataType: 'html',
                    success: function (data, textStatus, jqXHR) {
                        if (data.indexOf("<success") < 0) {
                            $("#server_info").replaceWith(data);
                        } else {
                            $("#server_info").replaceWith(data).promise().done(function(elem) {
                                $("#tabs_server").tabs();
                            });
                        }
                        _this.stopRefresh();
                    },
                    error: function (xhr, textStatus) {
                        _this.stopRefresh();
                        customAlert("Unexpected error, please check your network!", "Alert", undefined);
                    }
                })
            }

            this.startRefresh = function(hostId) {
                if(_this.onStart) _this.onStart();

                _this.refreshHostIds.push(hostId);
                if (!_this.isRefresing)
                    _this.refreshDialog.dialog("open");
                _this.isRefresing = true;

                _this.nextRefreshRequest();
            }

            this.nextRefreshRequest = function() {
                if (_this.refreshHostIds.length == 0) {
                    _this.stopRefresh();
                    return;
                }

                if (_this.refreshHostIds.length > 1) {
                    var progress = Math.floor((_this.finishedCount * 100) / _this.refreshHostIds.length);
                    _this.progressbar.progressbar("value", progress);
                }

                var hostId = _this.refreshHostIds[0];
                _this.refreshHostIds.splice(0, 1);
                //submit ajax request to scan all ip address
                $.ajax({
                    url: 'server_info.php',
                    type: 'POST',
                    data: {
                        <?php echo $name ?> : '<?php echo $tokens ?>',
                        action: 'refresh',
                        host_id: hostId,
                        drp_action: _this.drpAction
                    },
                    dataType: 'html',
                    success: function (data, textStatus, jqXHR) {
                        _this.finishedCount++;
                        if (data.indexOf("<success") < 0) {
                            customAlert(data, "Alert", undefined, _this.nextRefreshRequest);
                        } else {
                            $("#host_" + hostId).replaceWith(data);
                            setTimeout(_this.nextRefreshRequest, 500);
                        }
                    },
                    error: function (xhr, textStatus) {
                        _this.finishedCount++;
                        customAlert("Unexpected error, please check your network!", "Alert", undefined, _this.nextRefreshRequest);
                    }
                })
            }

            this.stopRefresh = function() {
                _this.progressbar.progressbar("value", false);
                _this.finishedCount = 0;
                _this.refreshHostIds = [];
                _this.isRefresing = false;

                _this.refreshDialog.dialog("close");
            }

            this.progressbar.progressbar({
                value: false,
                complete: function () {
                    _this.stopRefresh();
                }
            });
        }
    </script>

    <?php
}

function render_return_box() {
    ?>
    <table align="center" width="100%" style="margin-top:5px;">
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

//private function only
function _html_start_box($title, $width, $background_color, $cell_padding, $align, $host_id) {
    global $config, $category_num;
?>
<table id="host_<?php print $host_id;?>" align="<?php print $align;?>" width="<?php print $width;?>" cellpadding=0 cellspacing=0 border=0 class="cactiTable">
    <tr>
        <td>
            <table class='tableBody' cellpadding=<?php print $cell_padding;?> cellspacing=0 border=0 width="100%">
                <?php if ($title != "") {?>
                    <tr class='odd'>
                    <td style="padding: 3px;" colspan="100">
                        <table width="100%" cellpadding="0" cellspacing="0">
                            <tr>
                                <td style="font-size:1.2em;padding:5px;"><?php print $title;?></td>
                                <?php if ($host_id) {?>
                                    <td class="textHeaderDark" align="right">
                                        <a href="#" onclick="return refreshServerInfo(<?php print $host_id;?>, <?php print $category_num;?>)"><img width="16" src="<?php echo $config['url_path']; ?>plugins/hwserver/static/images/refresh.png"></a>
                                    </td>
                                    <td width="1" nowrap="nowrap" class="textHeaderDark" align="right" valign="middle">
                                        <strong><a href="#" onclick="return refreshServerInfo(<?php print $host_id;?>, <?php print $category_num;?>)">Refresh</a>&nbsp;</strong>
                                    </td><?php
                                }?>
                            </tr>
                        </table>
                    </td>
                    </tr>
                <?php }?>
<?php }

function _html_end_box($trailing_br = true) { ?>
				</table>
			</td>
		</tr>
	</table>
	<?php if ($trailing_br == true) { print "<br>"; } ?>
<?php }
?>

