<?php
/*
 +-------------------------------------------------------------------------+
 | Copyright (C) 2004-2009 The Cacti Group                                 |
 |                                                                         |
 | This program is free software; you can redistribute it and/or           |
 | modify it under the terms of the GNU General Public License             |
 | as published by the Free Software Foundation; either version 2          |
 | of the License, or (at your option) any later version.                  |
 |                                                                         |
 | This program is distributed in the hope that it will be useful,         |
 | but WITHOUT ANY WARRANTY; without even the implied warranty of          |
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the           |
 | GNU General Public License for more details.                            |
 +-------------------------------------------------------------------------+
 | Cacti: The Complete RRDTool-based Graphing Solution                     |
 +-------------------------------------------------------------------------+
 | This code is designed, written, and maintained by the Cacti Group. See  |
 | about.php and/or the AUTHORS file for specific developer information.   |
 +-------------------------------------------------------------------------+
 | http://www.cacti.net/                                                   |
 +-------------------------------------------------------------------------+
*/

/***************************************************************************
 * Author  ......... Jack Zhang
 * Version ......... 1.0
 * History ......... 2017/3/27 Created
 * Purpose ......... Used by cacti plugin framework, to list/install/uninstall
 *                   this plugin
 ***************************************************************************/

define("Huawei_SERVER_PLUGIN_VERSION", "1.1.0205");

/**
 * put plugin init code here
 */
function plugin_init_hwserver()
{
    global $plugin_hooks;
}

/**
 * both plugin_hwserver_version and hwserver_version
 * will be called by cacti plugins.php
 */
function plugin_hwserver_version()
{
    return hwserver_version();
}

function hwserver_version()
{
    return array('name' => 'Huawei Server',
        'version' => Huawei_SERVER_PLUGIN_VERSION,
        'longname' => 'Huawei server plugin for cacti',
        'author' => 'Huawei Enterprise',
        'homepage' => 'http://enterprise.huawei.com'
    );
}

/**
 * currently there is no dependence, will depend on threshold plugin
 * if we support server warning notifications in the future
 */
function hwserver_check_dependencies()
{
    global $plugins, $config;
    return true;
}

/**
 * install function, called when user click install button from
 * cacti plugins list page
*/
function plugin_hwserver_install()
{
    global $config;

    //register all api hook functions
    //refer to http://docs.cacti.net/plugins:development.hook_api_ref for detail hook apis list
    api_plugin_register_hook('hwserver', 'top_header_tabs', 'hwserver_show_tab', "setup.php");
    api_plugin_register_hook('hwserver', 'top_graph_header_tabs', 'hwserver_show_tab', "setup.php");
    api_plugin_register_hook('hwserver', 'config_arrays', 'hwserver_config_arrays', "setup.php");
    api_plugin_register_hook('hwserver', 'draw_navigation_text', 'hwserver_draw_navigation_text', "setup.php");
    api_plugin_register_hook('hwserver', 'config_form', 'hwserver_config_form', "setup.php");
    api_plugin_register_hook('hwserver', 'config_settings', 'hwserver_config_settings', "setup.php");
    api_plugin_register_hook('hwserver', 'api_device_save', 'hwserver_device_save', 'setup.php');
    api_plugin_register_hook('hwserver', 'api_device_new', 'hwserver_device_new', 'setup.php');
    api_plugin_register_hook('hwserver', 'device_remove', 'hwserver_remove_server', "setup.php");
    api_plugin_register_hook('hwserver', 'body_style', 'hwserver_api_plugin_hook_function_body_style', "setup.php");

    //api_plugin_register_hook('hwserver', 'poller_bottom', 'hwserver_poller_bottom', 'server_poller.php');

    //register realms
    api_plugin_register_realm('hwserver', 'server_list.php,server_detail.php,server_info.php,server_shelf_info.php', 'Plugin -> Huawei Server: View', 1);
    api_plugin_register_realm('hwserver', 'batch_import.php,scan_server.php,import_result.php', 'Plugin -> Huawei Server: Manage', 1);

    //setup databases datables needed by this plugin
    include_once($config['base_path'] . '/include/global.php');
    include_once($config['base_path'] . '/plugins/hwserver/includes/database.php');
    hwserver_setup_database();

    //install huawei server templates
    hwserver_setup_templaes();

    cacti_log("Huawei Server plugin: plugin install success!", false, "HWSERVER");
}

/**
 * plugin uninstall - a generic uninstall routine.  Right now it will do nothing
 * if you don't want the tables removed from the system except let it empty.
 */
function plugin_hwserver_uninstall()
{
    //currently do nothing
    cacti_log("Huawei Server plugin: plugin uninstall success!", false, "HWSERVER");
}

function hwserver_setup_templaes() {
    global $config;
    include_once($config['base_path'] . '/lib/import.php');

    $template_files = array(
        $config['base_path'] . '/plugins/hwserver/templates/cacti_host_template_huawei_blade_server.xml',
        $config['base_path'] . '/plugins/hwserver/templates/cacti_host_template_huawei_rack_server.xml'
    );

    foreach($template_files as $template_file) {
        $fp = fopen($template_file, "r");
        $xml_data = fread($fp, filesize($template_file));
        fclose($fp);

        $rra_array = array();

        /* obtain debug information if it's set */
        import_xml_data($xml_data, true, $rra_array);
    }
}

/**
 * here we will check to ensure everything is configured
 * and all configurations have been saved correctly
 */
function plugin_hwserver_check_config()
{
    return true;
}

/**
 * here we will upgrade to the newest version if needed
 */
function plugin_hwserver_upgrade()
{
    hwserver_check_upgrade();
    return false;
}

function hwserver_check_upgrade () {
    global $config;

    $files = array('index.php', 'server_list.php');
    if (isset($_SERVER['PHP_SELF']) && !in_array(basename($_SERVER['PHP_SELF']), $files)) {
        return;
    }

    $current = plugin_hwserver_version();
    $current = $current['version'];
    $old     = db_fetch_row("SELECT * FROM plugin_config WHERE directory='hwserver'");
    if (sizeof($old) && $current != $old["version"]) {
        /* if the plugin is installed and/or active */
        if ($old["status"] == 1 || $old["status"] == 4) {
            /* re-register the hooks */
            plugin_hwserver_install();

            /* perform a database upgrade */
            hwserver_database_upgrade();
        }

        /* update the plugin information */
        $info = plugin_hwserver_version();
        $id   = db_fetch_cell("SELECT id FROM plugin_config WHERE directory='hwserver'");
        db_execute("UPDATE plugin_config
			SET name='" . $info["longname"] . "',
			author='"   . $info["author"]   . "',
			webpage='"  . $info["homepage"] . "',
			version='"  . $info["version"]  . "'
			WHERE id='$id'");
    }
}

/***************************************************************************
 * The above functions are auto called by cacti plugin frameworks
 * Below is our registered api hook functions
 ***************************************************************************/

/**
 * add additional huswei server options to device edit page
 */
function hwserver_config_form()
{
    global $fields_host_edit;

    $fields_host_edit2 = $fields_host_edit;
    $fields_host_edit3 = array();
    foreach ($fields_host_edit2 as $f => $a) {
        $fields_host_edit3[$f] = $a;
        if ($f == 'disabled') {
            $fields_host_edit3['hw_is_huawei_server'] = array(
                'method' => 'checkbox',
                'friendly_name' => 'This is Huawei server with iBMC or HMM installed',
                'description' => 'Check to mark this device as Huawei server',
                'value' => '|arg1:hw_is_huawei_server|',
                'default' => '',
                'form_id' => false
            );
        }
    }

    $fields_host_edit = $fields_host_edit3;
}

/**
 * these options will appear at cacti settings page
 */
function hwserver_config_settings()
{
    global $tabs, $settings, $page_refresh_interval, $graph_timespans;

    if (isset($_SERVER['PHP_SELF']) && basename($_SERVER['PHP_SELF']) != 'settings.php')
        return;
}

/**
 * API hook fro device new
 */
function hwserver_device_new($save)  {
    //when create new device, hwserver_device_save first called,
    //  this time no id, then hwserver_device_new called, this time has id

    //if (!empty($save["id"]) && $save["id"] > 0) {
    //    return $save;
    //}

    hwserver_device_save($save);
}

/**
 * API hook for device save
 */
function hwserver_device_save($save) {
    if (isset($_POST['hw_is_huawei_server'])) {
        $save['hw_is_huawei_server'] = form_input_validate($_POST['hw_is_huawei_server'], 'hw_is_huawei_server', '', true, 3);
    } else {
        $save['hw_is_huawei_server'] = form_input_validate('', 'hw_is_huawei_server', '', true, 3);
    }

    if (!isset($save["id"]) || $save["id"] == 0) {
        return $save;
    }

    global $config;
    include_once($config['base_path'] . "/plugins/hwserver/includes/functions.php");

    //start a background task to retrive server informations
    $exist_server = hs_get_server_byid($save["id"]);
    if ($save['hw_is_huawei_server'] == "on") {
        include_once($config['base_path'] . "/plugins/hwserver/includes/oid_tables.php");
        include_once($config['base_path'] . "/plugins/hwserver/includes/snmp.php");
        if (empty($exist_server)) {
            global $hs_oids;

            $server = $save;
            $server["ip_address"] = $save["hostname"];
            $server_type = hs_snmp_get_server_type($server);
            if ($server_type > -1) {
                hs_init_server_info($server, $save["id"], 0);
                hs_update_cacti_host_type($save["id"], $server_type, $server);
            } else {
                $save['hw_is_huawei_server'] = "";
            }
        } else {
            //update server ip address, mark 0 to make it refresh next time
            $server_info = array("import_status" => 0, "ip_address" => $save["hostname"]);
            hs_update_server_info($save["id"], $server_info);

            $server = $save;
            $server["ip_address"] = $save["hostname"];
            $server_type = hs_snmp_get_server_type($server);
            if (($server_type == 1 && $server["hw_is_ibmc"] != 1)
                || ($server_type == 2 && $server["hw_is_hmm"] != 1)) {
                hs_update_cacti_host_type($save["id"], $server_type, $server);
            }
        }
    } elseif (!empty($exist_server)) {
        hs_remove_server($save["id"]);
    }

    return $save;
}

/**
 * show huawei server tab on cacti top navbar
 */
function hwserver_show_tab()
{
    global $config, $user_auth_realms, $user_auth_realm_filenames;
    $realm_id2 = 0;

    //check user permissions
    if (isset($user_auth_realm_filenames{basename('server_list.php')})) {
        $realm_id2 = $user_auth_realm_filenames{basename('server_list.php')};
    }

    if ((db_fetch_assoc("select user_auth_realm.realm_id
        from user_auth_realm where user_auth_realm.user_id='" . $_SESSION["sess_user_id"] . "'
        and user_auth_realm.realm_id='$realm_id2'")) || (empty($realm_id2))) {

        if (preg_match("/hwserver\\/.*\\.php/i", $_SERVER["REQUEST_URI"])) {
            print '<a href="' . $config['url_path'] . 'plugins/hwserver/server_list.php"><img src="' . $config['url_path'] . 'plugins/hwserver/static/images/tab_hwserver-green.png" alt="hwserver" align="absmiddle" border="0"></a>';
        } else {
            print '<a href="' . $config['url_path'] . 'plugins/hwserver/server_list.php"><img src="' . $config['url_path'] . 'plugins/hwserver/static/images/tab_hwserver.png" alt="hwserver" align="absmiddle" border="0"></a>';
        }
    }
}

/**
 * add a custom menu item to cacti left menu
 */
function hwserver_config_arrays()
{
    //global $menu, $messages, $hwserver_menu;
    //$menu['Utilities']['plugins/hwserver/hwserver.php'] = 'Menu hwserver';
}

/**
 * this is for cacti bread crumbs text under top navbar
 */
function hwserver_draw_navigation_text($nav)
{
    /* insert all your PHP functions that are accessible */
    $nav["batch_import.php:"] = array("title" => "Batch Import Huawei Server", "mapping" => "index.php:", "url" => "batch_import.php", "level" => "1");
    $nav["scan_server.php:"] = array("title" => "Scan Huawei Server", "mapping" => "index.php:", "url" => "scan_server.php", "level" => "1");
    $nav["import_result.php:"] = array("title" => "Import Huawei Server", "mapping" => "index.php:", "url" => "import_result.php", "level" => "1");
    $nav["server_shelf_info.php:"] = array("title" => "Huawei Server", "mapping" => "index.php:", "url" => "server_shelf_info.php", "level" => "1");
    $nav["server_info.php:"] = array("title" => "Huawei Server", "mapping" => "index.php:", "url" => "server_info.php", "level" => "1");
    $nav["server_list.php:"] = array("title" => "Huawei Server", "mapping" => "index.php:", "url" => "server_list.php", "level" => "1");
    return $nav;
}

/**
 * put additional css style here, it will output to the pages related to this plugin
 */
function hwserver_api_plugin_hook_function_body_style()
{
    print "";
}

function hwserver_remove_server($devices_to_act_on) {
    global $config;
    include_once($config['base_path'] . "/plugins/hwserver/includes/functions.php");

    foreach($devices_to_act_on as $host_id) {
        hs_remove_server($host_id);
    }
}

?>
