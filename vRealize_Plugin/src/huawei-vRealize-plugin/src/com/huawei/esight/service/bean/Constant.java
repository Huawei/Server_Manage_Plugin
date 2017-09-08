package com.huawei.esight.service.bean;


/**
 * 全局常量类.
 * @author jiawei
 *
 */
public class Constant {
    
    //adapter
    public static final String KEY_SERVER_IP_ADDRESS = "serverIP";
    public static final String KEY_ESIGHT_SERVER_PORT = "serverPort";
    public static final int DEFAULT_ESIGHT_SERVER_PORT = 32102;
    public static final String KEY_ESIGHT_ACCOUNT = "username";
    public static final String KEY_ESIGHT_CODE = "esightCode";
    public static final String PAGE_SIZE = "100";
    
    //default collect interval in minutes
    public static final int DEFAULT_COLLECT_INTERVAL = 10;
    
    public static final String TREE_SERVER_TYPE_RACK = "rack";
    public static final String TREE_SERVER_TYPE_BLADE = "blade";
    public static final String TREE_SERVER_TYPE_HIGHDENSITY = "highdensity";
    
    public static final String KIND_SERVER_TYPES = "serverTypes";
    public static final String ATTR_ID = "id";
    public static final String KIND_HOST_INSTANCE = "hostInstance";
    
    //硬件信息
    public static final String KIND_BOARD = "board";
    public static final String KIND_CPU = "cpu";
    public static final String KIND_DISK = "disk";
    public static final String KIND_FAN = "fan";
    public static final String KIND_MEMORY = "memory";
    public static final String KIND_NETWORKCATD = "card";
    public static final String KIND_PCIE = "pcie";
    public static final String KIND_PSU = "psu";
    public static final String KIND_RAID = "raid";
    public static final String KIND_SERVER_DEVICE = "serverDevice";
    public static final String KIND_MEZZ = "mezz";
    
    //硬件属性
    public static final String ATTR_NAME = "name";
    public static final String ATTR_NETWORKCARD_NAME = "netWorkCardName";
    public static final String ATTR_TYPE = "type";
    public static final String ATTR_SN = "sn";
    public static final String ATTR_PART_NUMBER = "partNumber";
    public static final String ATTR_MANUFACTURE = "manufacture";
    public static final String ATTR_MANUTIME = "manuTime";
    public static final String ATTR_UUID = "uuid";
    public static final String ATTR_MOID = "moId";
    public static final String ATTR_HEALTHSTATE = "healthState";
    public static final String ATTR_PRESENTSTATE = "presentState";
    public static final String ATTR_FREQUENCY = "frequency";
    public static final String ATTR_MODEL = "model";
    public static final String ATTR_LOCATION = "location";
    public static final String ATTR_ROTATE = "rotate";
    public static final String ATTR_ROTATEPERCENT = "rotatePercent";
    public static final String ATTR_CONTROLMODEL = "controlModel";
    public static final String ATTR_CAPACITY = "capacity";
    public static final String ATTR_MACADRESS = "macAdress";
    public static final String ATTR_PCIESSDCARD_HEALTHSTATUS = "pcieSsdCardHealthStatus";
    public static final String ATTR_PCIESSDCARD_LIFELEFT = "pcieSsdCardLifeLeft";
    public static final String ATTR_PCIESSDCARD_MANYFACTURER = "pciecardManufacturer";
    public static final String ATTR_INPUTPOWER = "inputPower";
    public static final String ATTR_VERSION = "version";
    public static final String ATTR_INPUTMODE = "inputMode";
    public static final String ATTR_POWER_PROTOCOL = "powerProtocol";
    public static final String ATTR_RATE_POWER = "ratePower";
    public static final String ATTR_RAID_TYPE = "raidType";
    public static final String ATTR_INTERFACE_TYPE = "interfaceType";
    public static final String ATTR_BBU_TYPE = "bbuType";
    
    public static final String ATTR_MEZZ_HEALTH_STATUS = "mezzHealthStatus";
    public static final String ATTR_MEZZ_INFO = "mezzInfo";
    public static final String ATTR_MEZZ_LOCATION = "mezzLocation";
    public static final String ATTR_MEZZ_MAC = "mezzMac";
    
    
    
    //devices attributes
    public static final String ATTR_DN = "dn";
    public static final String ATTR_IP_ADDRESS = "ipAddress";
    public static final String ATTR_STATUS = "status";
    public static final String ATTR_DESC = "desc";
    public static final String ATTR_CPU_NUMS = "cpuNums";
    public static final String ATTR_CPU_CORES = "cpuCores";
    public static final String ATTR_MEMORY_CAPACITY = "memoryCapacity";
    public static final String ATTR_PRODUCT_SN = "productSn";
    public static final String ATTR_BMC_MAC_ADDR = "bmcMacAddr";
    
    //Group
    public static final String TREE_BOARD_GROUP = "BoardGroup";
    public static final String KIND_BOARD_GROUP = "boardGroup";
    
    public static final String TREE_CPU_GROUP = "CPUGroup";
    public static final String KIND_CPU_GROUP = "cpuGroup";
    
    public static final String TREE_DISK_GROUP = "DiskGroup";
    public static final String KIND_DISK_GROUP = "diskGroup";
    
    public static final String TREE_FAN_GROUP = "FanGroup";
    public static final String KIND_FAN_GROUP = "fanGroup";
    
    public static final String TREE_MEMORY_GROUP = "MemoryGroup";
    public static final String KIND_MEMORY_GROUP = "memoryGroup";
    
    public static final String TREE_PSU_GROUP = "PSUGroup";
    public static final String KIND_PSU_GROUP = "psuGroup";
    
    public static final String TREE_DEVICES_GROUP = "DevicesGroup";
    public static final String KIND_DEVICES_GROUP = "devicesGroup";
    
    public static final String TREE_PCIE_GROUP = "PCIEGroup";
    public static final String KIND_PCIE_GROUP = "pcieGroup";
    
    public static final String TREE_RAID_GROUP = "RAIDGroup";
    public static final String KIND_RAID_GROUP = "raidGroup";
    
    public static final String TREE_NETWORK_CARD_GROUP = "NetworkCardGroup";
    public static final String KIND_NETWORK_CARD_GROUP = "cardGroup";
    
    public static final String TREE_MEZZ_GROUP = "MezzGroup";
    public static final String KIND_MEZZ_GROUP = "mezzGroup";
    
}
