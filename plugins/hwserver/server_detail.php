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

$server_info_cat = hs_get_server_info_category();
$server_info_cat_num = array();
foreach ($server_info_cat as $index => $category) {
    $server_info_cat_num[$category[0]] = $index;
}

global $server_type;
$server_type = -1;

$action = safe_get_request("action");
$host_id = safe_get_request("host_id");
$blade = safe_get_request("blade", -1);
$is_ajax = safe_get_request("is_ajax", 0);

input_validate_input_regex($host_id, "^([0-9_]+)$");

switch ($action) {
    case "details":
        if ($is_ajax == 0) {
            return render_default_page();
        }

        $host_info = hs_get_server_byid($host_id);
        if (!isset($host_info)) die("BAD host!");

        //check if need connect snmp fill full detail
        if ($host_info["import_status"] == 0) {
            hs_snmp_update_server_info($host_id, $host_info, null, null);
        }

        if ($host_info["hw_is_ibmc"] == 1) {
            $server_detail = hs_get_ibmc_info($host_info, null, true);
            $server_type = 1;
        } elseif ($host_info["hw_is_hmm"] == 1) {
            $server_detail = hs_get_hmm_info($host_info, null, array(1, 6, 7, 9, 10), true);
            $server_type = 2;
        } else {
            die("BAD request!");
        }

        if ($server_detail == null) {
            die("get blade information failed!");
        }

        print("<success desc=\"This is a indicator\"/>\n");
        render_page_title($server_detail["hostname"]. " (" .$server_detail["ip_address"]. ")");
        render_server_basic_info($server_detail, $server_type);
        render_server_detail($server_detail, $server_type);

        if ($host_info["hw_is_hmm"] == 1) {
            render_server_blades($server_detail);
        }

        render_return_box();
        break;

    case "blade_details":
        if ($is_ajax == 0) {
            return render_default_page();
        }

        $server_type = 3;
        $host_info = hs_get_server_byid($host_id);
        if (!(isset($host_info) && $host_info["hw_is_hmm"] == 1
              && isset($blade) && $blade > 0)) {
            die("Please choose a server with hmm installed!");
        }

        $blade_detail = hs_get_hmm_info($host_info, array($blade), null, true);
        if ($blade_detail == null || !array_key_exists("blade" .$blade, $blade_detail)) {
            die("get blade information failed!");
        }

        $blade_detail = &$blade_detail["blade" .$blade];
        $blade_detail = array_merge($blade_detail, $blade_detail["general"]);
        unset($blade_detail["general"]);

        print("<success desc=\"This is a indicator\"/>\n");
        render_page_title("Blade $blade (" .$blade_detail["bmc_ip"]. ")");
        render_server_basic_info($blade_detail, 3 /*3 for hmm blade*/);
        render_server_detail($blade_detail, 3);

        render_return_box();
        break;

    case "refresh":
        $category_num = safe_get_request('category');
        if (!(isset($host_id) && isset($host_id)
            && isset($category_num) && array_key_exists($category_num, $server_info_cat))) {
            die("BAD request!");
        }

        $host_info = hs_get_server_snmp_info($host_id);
        if (hs_snmp_update_server_info($host_id, $host_info, array($blade), array($category_num)) == 1) {
            if ($host_info["hw_is_ibmc"] == 1) {
                $server_detail = hs_get_ibmc_info($host_info, array($category_num), true);
                $server_type = 1;
            } elseif ($host_info["hw_is_hmm"] == 1) {
                $server_detail = hs_get_hmm_info($host_info, array($blade), array($category_num), false);
                $server_type = 2;
                if ($blade > 0 && array_key_exists("blade" .$blade, $server_detail)) {
                    $server_detail = &$server_detail["blade" . $blade];
                    $server_type = 3;
                }
            } else {
                die("BAD request!");
            }

            if (!isset($server_detail)) {
                die("refresh server info failed!");
            }

            print("<success desc=\"This is a indicator\"/>\n");
            $category = $server_info_cat[$category_num][0];
            $keys = &$hs_display_lables[$server_type][$category];
            render_server_table($category_num, $server_detail[$category], $keys, false);
        } else {
            print("snmp refresh server info failed!");
        }

        die();
        break;

    default:
        die("BAD request!");
        break;
}

function render_default_page() {
    hs_render_header();

    print("<div id=\"server_info\"></div>");
    refresh_form();

    hs_render_footer();
}

function render_server_basic_info(&$server_detail, $server_type) {
    global $hs_server_lables, $hs_display_lables, $hs_status_keys, $hs_value_keys, $host_id, $blade;
    _html_start_box("<strong>Basic Information</strong>", "100%", $host_id, $blade, null);

    $class = array("even-alternate", "odd");
    $i = 0;
    $row = 0;
    $keys = &$hs_display_lables[$server_type]["general"];
    foreach($keys as $key => $val) {
        if (!array_key_exists($key, $server_detail)) {
            continue;
        }

        $display_key = $key;
        if ($server_type == 2 && $display_key == "hostname")
            $display_key = "chassis_name";
        $value = $server_detail[$key];
        if ($i % 3 == 0) {
            if ($i > 0) {
                print("<td></td></tr>");
                $row++;
            }
            print("<tr class=\"" .$class[$row % 2]. "\">");
        }
        ?>
            <td align="left" width="15%" style="padding:5px;padding-left:30px;white-space: nowrap;">
                <span style="color:#666;"><?php echo $hs_server_lables[$display_key] ?></span>
                </br>
                <span>
                <?php
                if (in_array($key, $hs_status_keys)) {
                    echo (hs_get_status_html($key, $value, ($server_type == 2 || $server_type == 3) ? true : false));
                } elseif (in_array($key, $hs_value_keys)) {
                    echo (hs_get_value_html($key, $value));
                } else {
                    echo (!isset($value) || $value == -1) ? "Unknown" : $value;
                }
                ?>
                </span>
            </td>
        <?php
        $i++;
    }

    $col_spans = array(1, 3, 2);
    print("<td colspan=\"" .$col_spans[$i % 3]. "\"></td></tr>");

    _html_end_box();
}

function render_server_blades($server_detail) {
    global $host_id;

    _html_start_box("<strong>Blades List</strong>", "100%", null);
    print("
    <tr class=\"tableHeader\">
        <td class=\"row-header\">Blade</td>
        <td class=\"row-header\">Health</td>
        <td class=\"row-header\">IP Address</td>
        <td class=\"row-header\">Memory Size (GB)</td>
        <td class=\"row-header\"># of CPU</td>
    </tr>");

    $class = array("even-alternate", "odd");

    $row = 0;
    for ($index=1; $index<=32; $index++) {
        $blade_index = "blade" .$index;
        if (!array_key_exists($blade_index, $server_detail))
            continue;
        $row++;
        $blade = $server_detail[$blade_index]["general"];
        ?>
        <tr class="<?php echo $class[$row % 2] ?> selectable">
            <td align="left" width="20%" style="padding:5px;padding-left:30px;white-space: nowrap;">
                <strong><a href="server_detail.php?action=blade_details&host_id=<?php echo $host_id ?>&blade=<?php echo $index ?>"><?php echo $blade_index ?></a></strong>
            </td>
            <td><?php echo hs_get_status_html("blade_health", $blade["blade_health"], true) ?></td>
            <td><?php echo $blade["bmc_ip"] ?></td>
            <td><?php echo array_key_exists("mem_size", $blade) ? $blade["mem_size"] : "Unknown" ?></td>
            <td><?php echo array_key_exists("cpu_count", $blade) ? $blade["cpu_count"] : "Unknown" ?></td>
        </tr>
        <?php
    }

    _html_end_box();
}

function render_server_detail(&$server_detail, $server_type) {
    global $server_info_cat, $server_info_cat_num, $blade, $hs_display_lables;

    //print("<div class=\"cactiTable\" style=\"padding:5px;background-color:#fff;margin-bottom:15px;\">");
    _html_start_box("<strong>Components</strong>", "100%", null);
    print("<tr><td style=\"background-color:#fff;padding:5px;\"><div id=\"tabs_server\">");

    render_tabs($server_detail, $server_type);

    //other detail informations
    $keys = &$hs_display_lables[$server_type];
    foreach($keys as $category => &$category_keys) {
        if ($category == "general")
            continue;
        $category_num = $server_info_cat_num[$category];
        print("<div id=\"tabs-" .$category_num. "\" style=\"display:none;\">");
        if (array_key_exists($category, $server_detail)) {
            render_server_table($category_num, $server_detail[$category], $category_keys);
        } else {
            $empty = null;
            render_server_table($category_num, $empty, $empty);
        }
        print("</div>");
    }

    print("</div></td></tr>");
    _html_end_box();
}

function render_page_title($title) {
?>
    <table width="100%" align="center" style="margin-bottom: 10px;">
        <tr>
            <td class="textInfo">
                <?php print $title;?> Information
            </td>
        </tr>
    </table>
<?php
}

function render_tabs(&$server_detail, $server_type) {
    global $server_info_cat, $server_info_cat_num, $hs_display_lables;

    print("<ul>");

    $keys = &$hs_display_lables[$server_type];
    foreach($keys as $key => &$val) {
        if ($key == "general")
            continue;
        $category_num = $server_info_cat_num[$key];
        print("<li><a href=\"#tabs-" .$category_num. "\">" .$server_info_cat[$category_num][1]. "</a></li>");
    }

    print("</ul>");
}

function render_server_table($category_num, &$values, &$category_keys, $visible=true) {
    global $host_id, $blade, $server_info_cat;

    _html_start_box("<strong>" .$server_info_cat[$category_num][1]. " Information</strong>", "100%", $host_id, $blade, $category_num, "");

    if (!isset($values)) {
        $err_message = "Currently there is no " .$server_info_cat[$category_num][1]. " information!";
    }

    if (isset($err_message)){
        print("<tr><td>$err_message</td></tr>");
        _html_end_box(false);
        return;
    }

    render_server_info($category_num, $category_keys, $values);
    _html_end_box(false);
}

function render_server_info($category_num, &$category_keys, &$server_info) {
    global $server_info_cat;

    $class = array("even-alternate", "odd");
    $i = 0;

    foreach($category_keys as $key => $value) {
        if (array_key_exists($key, $server_info)) {
            render_server_info_line($key, $server_info[$key], $class[$i % 2]);
            $i++;
        }
    }

    foreach($server_info as $inst => $values) {
        if (is_array($values)) {
            render_instance_header($inst, $category_num);
            foreach($category_keys as $key => $value) {
                if (array_key_exists($key, $values)) {
                    render_server_info_line($key, $values[$key], $class[$i % 2]);
                    $i++;
                }
            }
        }
    }
}

function render_instance_header($instance, $category_num) {
    global $server_info_cat;
    ?>
    <tr class="tableHeader">
        <td colspan="2" style="padding:5px;color:#fff;"><strong><?php echo $server_info_cat[$category_num][1] ?> <?php echo $instance ?></strong></td>
    </tr>
    <?php
}

function render_server_info_line($key, $value, $class) {
    global $hs_server_lables, $hs_status_keys, $hs_value_keys, $server_type;
    if (is_numeric($value) && $value == -1)
        $value = "Unknown";
    ?>
    <tr class="<?php echo $class ?> selectable">
        <td align="left" width="20%" style="padding:5px;padding-left:30px;white-space: nowrap;"><strong><?php echo $hs_server_lables[$key] ?></strong></td>
        <td>
            <span>
            <?php

            if (in_array($key, $hs_status_keys)) {
                echo (hs_get_status_html($key, $value, ($server_type == 2 || $server_type == 3) ? true : false));
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
    global $config, $action, $host_id, $blade;

    $tokens = csrf_get_tokens();
    $name = $GLOBALS['csrf']['input-name'];
    ?>
    <div id="refreshDialog" title="Please wait while loading server information...">
        <div class="progress-label"></div>
        <div class="progressbar" style="margin-top: 20px;"></div>
    </div>

    <link href="<?php echo $config['url_path']; ?>plugins/hwserver/static/hwserver.css" type="text/css" rel="stylesheet">
    <script type="text/javascript" src="<?php echo $config['url_path']; ?>plugins/hwserver/static/custom-alert.js"></script>
    <script>
        $(function () {
            loadServerInfo(<?php echo("'$action',$blade"); ?>);
        });

        function loadServerInfo(action, blade) {
            refreshLoader.initLoad(action, blade);
        }

        $("#tabs_server").tabs();
        var refreshLoader = new RefreshLoader("refreshDialog", <?php echo $host_id; ?>);

        function refreshServerInfo(hostId, blade, category_num) {
            refreshLoader.startRefresh(hostId, blade, category_num);
            return false;
        }

        function RefreshLoader(dialogId, hostId) {
            var _this = this;
            this.hostId = hostId;
            this.isRefresing = false;

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

            this.initLoad = function(action, blade) {
                if(_this.onStart) _this.onStart();

                if (!_this.isRefresing)
                    _this.refreshDialog.dialog("open");
                _this.isRefresing = true;

                //submit ajax request to scan all ip address
                $.ajax({
                    url: 'server_detail.php',
                    type: 'POST',
                    data: {
                        <?php echo $name ?> : '<?php echo $tokens ?>',
                        action: action,
                        host_id: this.hostId,
                        blade: blade,
                        is_ajax: 1
                    },
                    dataType: 'html',
                    success: function (data, textStatus, jqXHR) {
                        if (data.indexOf("<success") < 0) {
                            var errHtml = _this.getErrorHtml(data);
                            $("#server_info").replaceWith(errHtml);
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

            this.startRefresh = function(hostId, blade, category_num) {
                if(_this.onStart) _this.onStart();

                if (!_this.isRefresing)
                    _this.refreshDialog.dialog("open");
                _this.isRefresing = true;

                //submit ajax request to scan all ip address
                $.ajax({
                    url: 'server_detail.php',
                    type: 'POST',
                    data: {
                        <?php echo $name ?> : '<?php echo $tokens ?>',
                        action: 'refresh',
                        host_id: this.hostId,
                        blade: blade,
                        category: category_num
                    },
                    dataType: 'html',
                    success: function (data, textStatus, jqXHR) {
                        if (data.indexOf("<success") < 0) {
                            customAlert(data, "Alert", undefined);
                        } else {
                            $("#table_" + category_num).replaceWith(data);
                        }
                        _this.stopRefresh();
                    },
                    error: function (xhr, textStatus) {
                        _this.stopRefresh();
                        customAlert("Unexpected error, please check your network!", "Alert", undefined);
                    }
                })
            }

            this.stopRefresh = function() {
                _this.progressbar.progressbar("value", false);
                _this.isRefresing = false;

                _this.refreshDialog.dialog("close");
            }

            this.getErrorHtml = function(data) {
                return ' \
                <table align="center" width="60%" cellpadding="0" cellspacing="0" border="0" class="cactiTable"> \
                    <tbody> \
                    <tr> \
                        <td> \
                            <table class="tableBody" cellpadding="3" cellspacing="0" border="0" width="100%"> \
                                <tbody> \
                                <tr class="cactiTableTitle"> \
                                    <td style="padding: 3px;" colspan="100"> \
                                        <table width="100%" cellpadding="0" cellspacing="0"> \
                                            <tbody><tr> \
                                                <td class="textHeaderDark"><strong>Get Server Information Error!</strong></td> \
                                            </tr> \
                                            </tbody> \
                                        </table> \
                                    </td> \
                                </tr> \
                                <tr> \
                                    <td class="even" height="35"><span class="textError">' +data+ '</span></td> \
                                </tr> \
                                <tr> \
                                    <td colspan="2" align="right" class="saveRow"> \
                                        <input type="button" value="Return" onclick="window.location.href=\'server_list.php\'"> \
                                    </td>\
                                </tr> \
                                </tbody> \
                            </table> \
                        </td> \
                    </tr> \
                    </tbody> \
                </table>';
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

//private function only
function _html_start_box($title, $width, $host_id, $blade=-1, $category_num=null, $css_class="cactiTable") {
    global $config;
?>
<table id="table_<?php print $category_num;?>" align="center" width="<?php print $width;?>" cellpadding=0 cellspacing=0 border=0 class="<?php print $css_class;?>">
    <tr>
        <td>
            <table cellpadding=3 cellspacing=0 border=0 width="100%">
                <?php if ($title != "") {?>
                    <tr class='odd'>
                    <td style="padding: 3px;" colspan="100">
                        <table width="100%" cellpadding="0" cellspacing="0">
                            <tr>
                                <td style="font-size:1.2em;padding:5px;"><?php print $title;?>&nbsp;</td>
                                <?php if ($host_id && $category_num) {?>
                                    <td class="textHeaderDark" align="right">
                                        <a href="#" onclick="return refreshServerInfo(<?php print ("$host_id,$blade,$category_num");?>);"><img width="16" src="<?php echo $config['url_path']; ?>plugins/hwserver/static/images/refresh.png"></a>
                                    </td>
                                    <td width="1" nowrap="nowrap" class="textHeaderDark" align="right" valign="middle">
                                        <strong><a href="#" onclick="return refreshServerInfo(<?php print ("$host_id,$blade,$category_num");?>);">Refresh</a>&nbsp;</strong>
                                    </td><?php
                                }?>
                            </tr>
                        </table>
                    </td>
                    </tr>
                <?php }?>
<?php }

/**
 * cacti 1.x html_start_box and html_end_box is not compatible
 */
function _html_end_box($trailing_br = true) { ?>
				</table>
			</td>
		</tr>
	</table>
	<?php if ($trailing_br == true) { print "<br>"; } ?>
<?php }
?>

