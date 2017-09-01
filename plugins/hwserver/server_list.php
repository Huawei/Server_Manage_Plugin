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
include_once("./plugins/hwserver/includes/functions.php");
include_once("./plugins/hwserver/lib/safe_input.php");

global $config;
if (!defined("MAX_DISPLAY_PAGES")) define("MAX_DISPLAY_PAGES", 30);

$tabs = array(
    1 => 'Rack Servers',
    2 => 'Blade Servers'
);

$action = safe_get_request('action');
switch ($action) {
    //no other action currently
    default:
        hs_render_header();

        server_list_form();

        hs_render_footer();
        break;
}

function server_list_form()
{
    global $config, $tabs, $item_rows;

    $filter_type = get_request_var_request("server_type", "");
    $filter_status = get_request_var_request("status");

    /* input validation */
    input_validate_input_number(get_request_var_request("page"));
    input_validate_input_number(get_request_var_request("rows"));
    input_validate_input_number($filter_status);
    input_validate_input_number($filter_type);

    /* clean up search string */
    if (isset($_REQUEST["filter"])) {
        $_REQUEST["filter"] = sanitize_search_string(get_request_var("filter"));
    }

    /* clean up sort_column */
    if (isset($_REQUEST["sort_column"])) {
        $_REQUEST["sort_column"] = sanitize_search_string(get_request_var("sort_column"));
    }

    /* clean up search string */
    if (isset($_REQUEST["sort_direction"])) {
        $_REQUEST["sort_direction"] = sanitize_search_string(get_request_var("sort_direction"));
    }

    /* if the user pushed the 'clear' button */
    if (isset($_REQUEST["clear_x"])) {
        kill_session_var("sess_hwservers_filter");
        kill_session_var("sess_hwservers_rows");
        kill_session_var("sess_hwservers_sort_column");
        kill_session_var("sess_hwservers_sort_direction");
        kill_session_var("sess_hwservers_current_page");
        kill_session_var("sess_hwservers_status");
        kill_session_var("sess_hwservers_server_type");

        unset($_REQUEST["page"]);
        unset($_REQUEST["rows"]);
        unset($_REQUEST["filter"]);
        unset($_REQUEST["sort_column"]);
        unset($_REQUEST["sort_direction"]);
        unset($_REQUEST["status"]);
        unset($_REQUEST["server_type"]);
        $_REQUEST["page"] = 1;
    }

    if ((!empty($_SESSION["sess_hwservers_status"])) && (!empty($filter_status))) {
        if ($_SESSION["sess_hwservers_status"] != $filter_status)
            $_REQUEST["page"] = 1;
    }

    if ((!empty($_SESSION["sess_hwservers_server_type"])) && (!empty($filter_type))) {
        if ($_SESSION["sess_hwservers_server_type"] != $filter_type)
            $_REQUEST["page"] = 1;
    }

    /* remember these search fields in session vars so we don't have to keep passing them around */
    load_current_session_value("filter", "sess_hwservers_filter", "");
    load_current_session_value("rows", "sess_hwservers_rows", "-1");
    load_current_session_value("sort_column", "sess_hwservers_sort_column", "hostname");
    load_current_session_value("sort_direction", "sess_hwservers_sort_direction", "ASC");
    load_current_session_value("page", "sess_hwservers_current_page", "1");
    load_current_session_value("status", "sess_hwservers_status", "-1");
    load_current_session_value("server_type", "sess_hwservers_server_type", "1");

    $rows = get_request_var_request('rows');
    if (empty($rows) || $rows < 0) {
        $rows = read_config_option('num_rows_device');
        if (empty($rows)) {
            $rows = read_config_option('num_rows_table');
        }
    }

    $result = hs_get_server_list(get_request_var_request("filter"),
                                 get_request_var_request("status", null),
                                 get_request_var_request("server_type", null),
                                 get_request_var_request("sort_column"),
                                 get_request_var_request("sort_direction"),
                                 get_request_var_request("page"),
                                 $rows);

    $total_rows = $result["total"];
    $server_list = $result["server_list"];
    ?>

    <style>
        td.tab {
            border-top: 0;
            border-right: 0;
            border-left: 0;
        }
    </style>
    <script type="text/javascript">
        <!--
        function applyViewServerFilterChange(objForm) {
            strURL = '?status=' + objForm.status.value;
            strURL = strURL + '&server_type=' + objForm.server_type.value;
            strURL = strURL + '&rows=' + objForm.rows.value;
            strURL = strURL + '&filter=' + objForm.filter.value;
            document.location = strURL;
        }
        -->
    </script>

    <table width="100%" style="margin-top:5px;margin-bottom:5px;">
        <tr>
            <td rowspan="2"><a href="http://enterprise.huawei.com">
                <img src="<?php echo $config['url_path']; ?>plugins/hwserver/static/images/logo_huawei.png" alt="Huawei Enterprise"></a></td>
            <td align="right">
                <a href="<?php echo $config['url_path']; ?>plugins/hwserver/batch_import.php" class="linkEditMain">Batch Import Servers</a>
            </td>
        </tr>
        <tr>
            <td align="right">
                <a href="<?php echo $config['url_path']; ?>plugins/hwserver/scan_server.php" class="linkEditMain">Auto Scan Servers</a>
            </td>
        </tr>
    </table>

    <form name="form_servers" action="server_list.php">
    <?php
    html_start_box("<strong>Huawei Servers List</strong>", "100%", "", "3", "center", "");
    ?>
        <tr>
            <!--
            <td nowrap style='white-space: nowrap;' width="50">
                Type:&nbsp;
            </td>
            <td width="1">
                <select id="server_type" onChange="applyViewServerFilterChange(document.form_servers)">
                    <option value="-1"<?php if ($filter_type == "-1") { ?> selected<?php } ?>>Any</option>
                    <option value="1"<?php if ($filter_type == "1") { ?> selected<?php } ?>>iBMC Servers</option>
                    <option value="2"<?php if ($filter_type == "2") { ?> selected<?php } ?>>HMM Servers</option>
                </select>
            </td>
            -->
            <td nowrap style='white-space: nowrap;' width="50">
                &nbsp;Status:&nbsp;
            </td>
            <td width="1">
                <select name="status" onChange="applyViewServerFilterChange(document.form_servers)">
                    <option value="-1"<?php if ($filter_status == "-1") { ?> selected<?php } ?>>Any</option>
                    <option value="-3"<?php if ($filter_status == "-3") { ?> selected<?php } ?>>Enabled</option>
                    <option value="-2"<?php if ($filter_status == "-2") { ?> selected<?php } ?>>Disabled</option>
                    <option value="-4"<?php if ($filter_status == "-4") { ?> selected<?php } ?>>Not Up</option>
                    <option value="3"<?php if ($filter_status == "3") { ?> selected<?php } ?>>Up</option>
                    <option value="1"<?php if ($filter_status == "1") { ?> selected<?php } ?>>Down</option>
                    <option value="2"<?php if ($filter_status == "2") { ?> selected<?php } ?>>Recovering</option>
                    <option value="0"<?php if ($filter_status == "0") { ?> selected<?php } ?>>Unknown</option>
                </select>
            </td>
            <td nowrap style='white-space: nowrap;' width="20">
                &nbsp;Search:&nbsp;
            </td>
            <td width="1">
                <input type="text" name="filter" size="20" value="<?php print htmlspecialchars(get_request_var_request("filter")); ?>">
            </td>
            <td nowrap style='white-space: nowrap;' width="50">
                &nbsp;Rows per Page:&nbsp;
            </td>
            <td width="1">
                <select name="rows" onChange="applyViewServerFilterChange(document.form_servers)">
                    <option value="-1"<?php if (get_request_var_request("rows") == "-1") { ?> selected<?php } ?>>
                        Default
                    </option>
                    <?php
                    if (sizeof($item_rows) > 0) {
                        foreach ($item_rows as $key => $value) {
                            print "<option value='" . $key . "'";
                            if (get_request_var_request("rows") == $key) {
                                print " selected";
                            }
                            print ">" . htmlspecialchars($value) . "</option>\n";
                        }
                    }
                    ?>
                </select>
            </td>
            <td nowrap style='white-space: nowrap;'>
                &nbsp;<input type="submit" value="Go" title="Set/Refresh Filters">
                &nbsp;<input type="submit" name="clear_x" value="Clear" title="Clear Filters">
                <input type="hidden" id="server_type" value="<?php print($_REQUEST["server_type"]) ?>">
            </td>
        </tr>
    <?php
    html_end_box();
    ?>
    </form>

    <?php

    /* draw the categories tabs on the top of the page */
    print "<table cellpadding='0' cellspacing='0' border='0'><tr><td>\n";
    print "<table class='tabs' cellspacing='0' cellpadding='3' align='left'><tr class='tabsMarginLeft'>\n";

    if (sizeof($tabs) > 0) {
        foreach (array_keys($tabs) as $tab_short_name) {
            print "<td " . (($tab_short_name == $_REQUEST["server_type"]) ? "class='tabSelected tab'" : "class='tabNotSelected tab'") .
                " align='center'><span class='textHeader'><a href='" . htmlspecialchars("server_list.php?server_type=$tab_short_name") .
                "'>$tabs[$tab_short_name]</a></span></td><td class='tabSpacer'></td>\n";
        }
    }

    print "</tr></table></td>\n";
    print "</tr></table><table width='100%' cellpadding='0' cellspacing='0' border='0'><tr><td>\n";

    ?>

    <form name="chk" method="GET" action="server_info.php">
    <?php
    $page_nav = html_nav_bar("server_list.php?filter=" . get_request_var_request("filter"), MAX_DISPLAY_PAGES, get_request_var_request("page"), $rows, $total_rows, $_REQUEST["server_type"] == 1 ? 10 : 8);

    html_start_box("", "100%", "", "3", "center", "");
    print $page_nav;
    html_end_box(false);

    html_start_box("", "100%", "", "3", "center", "");
    if ($_REQUEST["server_type"] == 1) {
        render_ibmc_server_list($server_list, $page_nav);
    } else {
        render_hmm_server_list($server_list, $page_nav);
    }

    ?>
    </form>
    <?php
}

function render_ibmc_server_list(&$server_list, &$page_nav) {
    $display_text = array(
        "hostname" => array("Host Name", "ASC"),
        "nosort1" => array("ID", "ASC"),
        "ip_address" => array("IP Address", "ASC"),
        "model" => array("Model", "ASC"),
        "nosort2" => array("iBMC Version", "ASC"),
        "status" => array("Status", "ASC"),
        "nosort3" => array("# of CPU", "ASC"),
        "nosort4" => array("Memory (GB)", "DESC"),
        "nosort5" => array("Disk (GB)", "DESC"));

    html_header_sort_checkbox($display_text, get_request_var_request("sort_column"), get_request_var_request("sort_direction"), false);

    $i = 0;
    if (sizeof($server_list) > 0) {
        foreach ($server_list as $server) {
            form_alternate_row('line' . $server["host_id"], true);
            $highlight_text = strlen(get_request_var_request("filter"))
                ? preg_replace("/(" . preg_quote(get_request_var_request("filter"), "/") . ")/i", "<span style='background-color: #F8D93D;'>\\1</span>", htmlspecialchars($server["hostname"]))
                : htmlspecialchars($server["hostname"]);
            form_selectable_cell("<a class='linkEditMain' href='" . htmlspecialchars("server_detail.php?action=details&host_id=" . $server["host_id"]) . "'>" . $highlight_text . "</a>", $server["host_id"], 250);
            form_selectable_cell($server["host_id"], $server["host_id"]);
            form_selectable_cell($server["ip_address"], $server["host_id"]);
            form_selectable_cell((isset($server["model"]) ? $server["model"] : "Unknown"), $server["host_id"]);
            form_selectable_cell((isset($server["ibmc_ver"]) ? $server["ibmc_ver"] : "Unknown"), $server["host_id"]);
            form_selectable_cell(get_colored_device_status(($server["disabled"] == "on" ? true : false), $server["host_status"]).hs_get_import_status_html($server["import_status"]), $server["host_id"]);
            form_selectable_cell($server["cpu_count"], $server["host_id"]);
            form_selectable_cell((($server["mem_size"] > 0) ? $server["mem_size"] . "G" : "Unknown"), $server["host_id"]);
            form_selectable_cell((($server["disk_size"] > 0) ? $server["disk_size"] . "G" : "Unknown"), $server["host_id"]);
            form_checkbox_cell($server["hostname"], $server["host_id"]);
            form_end_row();
        }
    } else {
        print "<tr><td colspan=\"10\"><em>No server found, try <a href=\"batch_import.php\">Batch Import Servers</a> or <a href=\"scan_server.php\">Auto Scan Servers</a></em></td></tr>";
    }

    if (count($server_list) > 0) {
        html_end_box(false);
        html_start_box("", "100%", "", "3", "center", "");
        print $page_nav;
        html_end_box(false);
        print("<div style=\"margin-bottom:5px;\"></div>");
    } else {
        html_end_box(false);
    }


    /* draw the dropdown containing a list of available actions for this form */
    $server_actions = array(
        1 => "View Basic Info",
        2 => "View Mainboard Info",
        3 => "View Memory Info",
        4 => "View CPU Info",
        5 => "View Hard disk Info",
        6 => "View Power info",
        7 => "View Fan info",
        8 => "View RAID card info"
    );

    draw_actions_dropdown($server_actions);
}

function render_hmm_server_list(&$server_list, &$page_nav) {
    $display_text = array(
        "hostname" => array("Chassis Name", "ASC"),
        "nosort1" => array("ID", "ASC"),
        "ip_address" => array("IP Address", "ASC"),
        "model" => array("Model", "ASC"),
        "nosort2" => array("SMM Version", "ASC"),
        "status" => array("Status", "ASC"),
        "nosort3" => array("Blade Count", "DESC"));

    html_header_sort_checkbox($display_text, get_request_var_request("sort_column"), get_request_var_request("sort_direction"), false);

    $i = 0;
    if (sizeof($server_list) > 0) {
        foreach ($server_list as $server) {
            form_alternate_row('line' . $server["host_id"], true);
            $highlight_text = strlen(get_request_var_request("filter"))
                ? preg_replace("/(" . preg_quote(get_request_var_request("filter"), "/") . ")/i", "<span style='background-color: #F8D93D;'>\\1</span>", htmlspecialchars($server["hostname"]))
                : htmlspecialchars($server["hostname"]);
            form_selectable_cell("<a class='linkEditMain' href='" . htmlspecialchars("server_detail.php?action=details&host_id=" . $server["host_id"]) . "'>" . $highlight_text . "</a>", $server["host_id"], 250);
            form_selectable_cell($server["host_id"], $server["host_id"]);
            form_selectable_cell($server["ip_address"], $server["host_id"]);
            form_selectable_cell((isset($server["model"]) ? $server["model"] : "Unknown"), $server["host_id"]);
            form_selectable_cell((isset($server["smm_ver"]) ? $server["smm_ver"] : "Unknown"), $server["host_id"]);
            form_selectable_cell(get_colored_device_status(($server["disabled"] == "on" ? true : false), $server["host_status"]) . hs_get_import_status_html($server["import_status"]), $server["host_id"]);
            form_selectable_cell(($server["blade_count"] > 0 ? $server["blade_count"] : "Unknown"), $server["host_id"]);
            form_checkbox_cell($server["hostname"], $server["host_id"]);
            form_end_row();
        }
    } else {
        print "<tr><td colspan=\"8\"><em>No server found, try <a href=\"batch_import.php\">Batch Import Servers</a> or <a href=\"scan_server.php\">Auto Scan Servers</a></em></td></tr>";
    }

    if (count($server_list) > 0) {
        html_end_box(false);
        html_start_box("", "100%", "", "3", "center", "");
        print $page_nav;
        html_end_box(false);
        print("<div style=\"margin-bottom:5px;\"></div>");
    } else {
        html_end_box(false);
    }

    /* draw the dropdown containing a list of available actions for this form */
    $server_actions = array(
        1 => "View Basic Info",
        9 => "View Shelf Info",
        6 => "View Power info",
        7 => "View Fan info"
    );

    draw_actions_dropdown($server_actions);
}

?>
