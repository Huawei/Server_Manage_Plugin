<?php
/***************************************************************************
 * Author  ......... Jack Zhang
 * Version ......... 1.0
 * History ......... 2017/4/19 Created
 * Purpose ......... Define all server key lables
 ***************************************************************************/

global $hs_display_lables, $hs_status_keys, $hs_def_status_texts;

//category definition refer to: functions.php -> hs_get_server_info_category

$hs_status_keys = array(
    "status",
    "mem_status",
    "power_status",
    "cpu_status",
    "disk_status",
    "net_health",
    "raid_status",
    "shelf_health",
    "fan_status",
    "blade_health",
    "smm_presence"
);

$hs_value_keys = array(
    "shelf_power_realtime",
    "shelf_power_capping_enable",
    "shelf_power_capping_value",
    "power_current",
    "power_peak",
    "power_average",
    "power_total_consumption",
    "power_is_limit",
    "power_limit_max",
    "fan_speed",
    "cpu_frequency",
    "mem_size",
    "disk_size",
    "disk_capacity",
    "disk_media_type",
    "disk_interface_type"
);

$hs_def_status_texts = array(
    1 => "OK",
    2 => "Minor",
    3 => "Major",
    4 => "Critical",
    5 => "Unknown");

$hs_display_lables = array(
    /**
     * iBMC rack server display items
     * array item value 1 means need update the information at RUNTIME!!
     */
    1 => array(
        "general" => array(
            "host_id" => 0,
            "sys_guid" => 0,
            "status" => 1,
            "model" => 0,
            "ibmc_ver" => 0,
            "ip_address" => 0,
            "hostname" => 0,
            "cpu_count" => 0,
            "mem_size" => 0,
            "disk_size" => 0,

            "cpld_ver" => 0,
            "uboot_ver" => 0,
            "fpga_ver" => 0,
            "bios_ver" => 0,
            //"vlan_setting" => 0,
            //"manufacturer" => 0,
            //"sn" => 0, //58.251.166.177 has this item

            //"import_status" => 0, //manual display
            //"last_update" => "Info Last Update Time"
        ),

        "mainboard" => array(
            "board_model" => 0,
            "board_id" => 0,
            "board_manufacturer" => 0,
            "board_external_name" => 0,
            "board_sn" => 0
        ),

        "memory" => array(
            "mem_count" => 0,
            "mem_slot" => 0,
            "mem_status" => 1,
            "mem_location" => 0,
            "mem_type" => 0,
            "mem_size" => 0,
            "mem_sn" => 0,
            "mem_manufacturer" => 0,
            "mem_bit_width" => 0
        ),

        "power" => array(
            "power_count" => 0,
            "power_current" => 1,
            "power_peak" => 1,
            "power_average" => 1,
            "power_total_consumption" => 1,
            "power_is_limit" => 0,
            "power_limit_max" => 0,

            "power_manufacturer" => 0,
            "power_status" => 1,
            //"power_presence" => 0, -- if not presence will not display
            "power_model" => 0,
            "power_location" => 0
        ),

        "cpu" => array(
            "cpu_count" => 0,
            "cpu_location" => 0,
            "cpu_manufacturer" => 0,
            "cpu_family" => 0,
            "cpu_status" => 1,
            "cpu_type" => 0,
            "cpu_frequency" => 0,
            "cpu_core_count" => 0,
            "cpu_thread_count" => 0
        ),

        "disk" => array(
            "disk_count" => 0,
            "disk_slot" => 0,
            "disk_capacity" => 0,
            "disk_manufacturer" => 0,
            "disk_model" => 0,
            "disk_sn" => 0,
            "disk_status" => 1,
            //"disk_presence" => 1,
            "disk_media_type" => 0,
            "disk_interface_type" => 0,
            "disk_capable_speed" => 0,
            "disk_negotiated_speed" => 0
        ),

        "raidcard" => array(
            "raid_count" => 0,
            "raid_model" => 0,
            "raid_firmware_ver" => 0,
            "raid_status" => 1
        ),

        "fan" => array(
            "fan_count" => 0,
            "fan_location" => 0, //refer to 0519 version test result
            "fan_status" => 1,
            "fan_speed" => 1
            //"fan_presence" => 1
        )
    ),

    /**
     * HMM blade server display items
     */
    2 => array(
        "general" => array(
            "host_id" => 0,
            "status" => 1,
            "model" => 0,
            "smm_ver" => 0,
            "ip_address" => 0,
            "hostname" => 0,
            "blade_count" => 0,

            "uboot_ver" => 0,
            "cpld_ver" => 0,
            "pcb_ver" => 0,
            "bom_ver" => 0,
            "fpga_ver" => 0,
            "device_id" => 0,
            "smm_presence" => 0
        ),

        "shelf" => array(
            "shelf_health" => 1,
            "shelf_chassis_name" => 0,
            "shelf_power_realtime" => 0,
            "shelf_power_capping_enable" => 0,
            "shelf_power_capping_value" => 0,
            "shelf_location" => 0,
            "shelf_chassis_id" => 0,
            "shelf_backplane_type" => 0
        ),

        "power" => array(
            "power_count" => 0,
            "power_current" => 1,
            "power_status" => 1
        ),

        "fan" => array(
            "fan_count" => 0,
            //"fan_location" => 0,
            "fan_status" => 1,
            "fan_speed" => 1
        )
    ),

    /**
     * Blade item display items
     */
    3 => array(
        "general" => array(
            "blade_health" => 1,
            "bmc_ip" => 0,
            "sys_guid" => 0,
            "ip_address" => 0,
            "cpu_count" => 0,
            "mem_size" => 0,
            "disk_size" => 0
        ),

        //"mainboard" => array(
        //),

        "memory" => array(
            "mem_count" => 0,
            "mem_status" => 1,
            "mem_manufacturer" => 0,
            "mem_location" => 0,
            "mem_type" => 0,
            "mem_size" => 0
        ),

        "cpu" => array(
            "cpu_count" => 0,
            "cpu_location" => 0,
            "cpu_manufacturer" => 0,
            "cpu_status" => 1,
            "cpu_type" => 0,
            "cpu_frequency" => 0,
            "cpu_core_count" => 0,
            "cpu_thread_count" => 0
        ),

        "disk" => array(
            "disk_count" => 0,
            "disk_capacity" => 0,
            "disk_manufacturer" => 0,
            "disk_model" => 0,
            "disk_sn" => 0,
            "disk_interface_type" => 0,
            "disk_status" => 1,
            "disk_location" => 0 //HMM only
        ),

        "network" => array(
            "net_index" => 0,
            "net_health" => 1,
            "net_mark" => 0,
            "net_info" => 0,
            "net_location" => 0,
            "net_mac" => 0
        ),

        "raidcard" => array(
            "raid_count" => 0,
            "raid_model" => 0,
            "raid_firmware_ver" => 0,
            "raid_status" => 1 //HMM only
        ),
    )
);

$hs_server_lables = array(
    "host_id" => "Host Id",
    "sys_guid" => "System GUID",
    "status" => "Health Status",
    "import_status" => "Import Status",
    "last_update" => "Info Last Update Time",
    "model" => "Model",
    "ibmc_ver" => "iBMC Version",
    "smm_ver" => "HMM Version",
    "ip_address" => "IP Address",
    "hostname" => "Host Name",
    "chassis_name" => "Chassis Name",
    "cpu_count" => "Total # of CPUs",
    "mem_size" => "Memory Capacity",
    "disk_size" => "Hard Disk Capacity",
    "blade_count" => "# of Blades",

    "cpld_ver" => "CPLD Version",
    "uboot_ver" => "UBOOT Version",
    "fpga_ver" => "FPGA Version",
    "bios_ver" => "BIOS Version",
    "lcd_ver" => "LCD Version",
    "vlan_setting" => "VLan Settings",

    "bom_ver" => "BOM Version",
    "pcb_ver" => "PCB Version",
    "device_id" => "Device Id",
    "smm_presence" => "SMM Health Status",
    "manufacturer" => "Manufacturer",
    "sn" => "Serial Number",

    "shelf_chassis_name" => "Chassis Name",
    "shelf_health" => "Health Status",
    "shelf_power_realtime" => "Power Realtime Value",
    "shelf_power_capping_enable" => "Power Capping Enabled",
    "shelf_power_capping_value" => "Power Capping Value",
    "shelf_location" => "Shelf Location",
    "shelf_chassis_id" => "Chassis Id",
    "shelf_backplane_type" => "Shelf Type",

    "board_model" => "Model",
    "board_id" => "Mainboard Id",
    "board_manufacturer" => "Manufacturer",
    "board_external_name" => "External Name",
    "board_sn" => "Serial Number",

    "mem_count" => "Total # of Memories",
    "mem_slot" => "Memory Slot",
    "mem_status" => "Memory Status",
    "mem_location" => "Memory Location",
    "mem_type" => "Memory Type",
    "mem_size" => "Memory Capacity",
    "mem_sn" => "Serial Number",
    "mem_manufacturer" => "Manufacturer",
    "mem_bit_width" => "Bit Width",
    "mem_presence" => "Presence Status",

    "power_count" => "Total # of Powers",
    "power_current" => "Current Power",
    "power_peak" => "Peak Power",
    "power_average" => "Average Power",
    "power_total_consumption" => "Total Power Consumption",
    "power_is_limit" => "Capping Enabled",
    "power_limit_max" => "Capping Value",

    "power_manufacturer" => "Manufacturer",
    "power_status" => "Power Status",
    "power_presence" => "Presence Status",
    "power_model" => "Power Model",
    "power_location" => "Power Location",

    "cpu_count" => "Total # of CPUs",
    "cpu_presence" => "Presence Status",
    "cpu_location" => "CPU Location",
    "cpu_manufacturer" => "Manufacturer",
    "cpu_family" => "Family",
    "cpu_status" => "CPU Status",
    "cpu_type" => "CPU Type",
    "cpu_frequency" => "Frequency",
    "cpu_core_count" => "Core Count",
    "cpu_thread_count" => "Thread Count",

    "disk_count" => "Total # of Hardisks ",
    "disk_capacity" => "Disk Capacity",
    "disk_manufacturer" => "Manufacturer",
    "disk_model" => "Disk Model",
    "disk_sn" => "Serial Number",
    "disk_status" => "Disk Status",
    "disk_presence" => "Presence Status",
    "disk_slot" => "Disk Slot",
    "disk_location" => "Disk Location", //HMM only
    "disk_type" => "Disk Type", //duplicate
    "disk_media_type" => "Media Type",
    "disk_interface_type" => "Interface Type",
    "disk_capable_speed" => "Capable Speed",
    "disk_negotiated_speed" => "Negotiated Speed",

    "net_health" => "Mezz Health",
    "net_index" => "Mezz Index",
    "net_mark" => "Mezz Mark",
    "net_info" => "Mezz Info",
    "net_presence" => "Mezz Presence",
    "net_location" => "Mezz Location",
    "net_mac" => "Mezz MAC",

    "raid_count" => "Total # of RAID Controllers",
    "raid_model" => "RAID Controller Model",
    "raid_firmware_ver" => "Firmware Version",
    "raid_status" => "RAID Controller Status", //HMM only

    "fan_count" => "Total # of Fans",
    "fan_location" => "Fan Location",
    "fan_status" => "Fan Status",
    "fan_speed" => "Fan Speed",
    "fan_presence" => "Presence Status",

    "blade_health" => "Health Status",
    "bmc_ip" => "BMC IP Address"
);

