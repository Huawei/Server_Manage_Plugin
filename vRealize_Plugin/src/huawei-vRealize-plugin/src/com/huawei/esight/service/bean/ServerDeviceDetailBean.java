package com.huawei.esight.service.bean;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.ObjectWriter;
import com.huawei.adapter.util.ConvertUtils;
import com.integrien.alive.common.adapter3.MetricData;
import com.integrien.alive.common.adapter3.MetricKey;
import com.integrien.alive.common.adapter3.ResourceKey;
import com.integrien.alive.common.adapter3.config.ResourceIdentifierConfig;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * 服务器详细信息对象.
 * @author harbor
 *
 */
@JsonIgnoreProperties(ignoreUnknown = true)
public class ServerDeviceDetailBean implements TreeNodeResource {
    
    private String dn;
    
    private String ipAddress;
    
    private String name;
    
    private String type;
    
    private String uuid;
    
    private int status;
    
    private String desc;
    
    private int cpuNums;
    
    private int cpuCores;
    
    private String productSn;
    
    private String bmcMacAddr;
    
    private String mode;
    
    private String realTimePower;
    
    public String getRealTimePower() {
        return realTimePower;
    }
    
    public void setRealTimePower(String realTimePower) {
        this.realTimePower = realTimePower;
    }
    
    @JsonProperty("MemoryCapacity")
    private String memoryCapacity;
    
    //added by harbor on 2017.08.07
    private String location;
    
    private String manufacturer;
    
    private String version;
    
    @JsonProperty("CPU")
    private List<CPUBean> cpu = new ArrayList<CPUBean>();
    
    @JsonProperty("Memory")
    private List<MemoryBean> memory = new ArrayList<MemoryBean>();
    
    @JsonProperty("Disk")
    private List<DiskBean> disk = new ArrayList<DiskBean>();
    
    @JsonProperty("PSU")
    private List<PSUBean> psu = new ArrayList<PSUBean>();
    
    @JsonProperty("Fan")
    private List<FanBean> fan = new ArrayList<FanBean>();
    
    private List<BoardBean> board = new ArrayList<BoardBean>();
    
    @JsonProperty("RAID")
    private List<RAIDBean> raid = new ArrayList<>();
    
    @JsonProperty("NetworkCard")
    private List<NetworkCardBean> networkCard = new ArrayList<>();
    
    @JsonProperty("PCIE")
    private List<PCIEBean> pcie = new ArrayList<>();
    
    @JsonProperty("Mezz")
    private List<MezzBean> mezz = new ArrayList<>();
    
    public String getDn() {
        return dn;
    }
    
    public void setDn(String dn) {
        this.dn = dn;
    }
    
    public String getIpAddress() {
        return ipAddress;
    }
    
    public void setIpAddress(String ipAddress) {
        this.ipAddress = ipAddress;
    }
    
    public String getName() {
        return name;
    }
    
    public void setName(String name) {
        this.name = name;
    }
    
    public String getType() {
        return type;
    }
    
    public void setType(String type) {
        this.type = type;
    }
    
    public String getUuid() {
        return uuid;
    }
    
    public void setUuid(String uuid) {
        this.uuid = uuid;
    }
    
    public int getStatus() {
        return status;
    }
    
    public void setStatus(int status) {
        this.status = status;
    }
    
    public String getDesc() {
        return desc;
    }
    
    public void setDesc(String desc) {
        this.desc = desc;
    }
    
    public String getMemoryCapacity() {
        return memoryCapacity;
    }
    
    public void setMemoryCapacity(String memoryCapacity) {
        this.memoryCapacity = memoryCapacity;
    }
    
    public int getCpuNums() {
        return cpuNums;
    }
    
    public void setCpuNums(int cpuNums) {
        this.cpuNums = cpuNums;
    }
    
    public int getCpuCores() {
        return cpuCores;
    }
    
    public void setCpuCores(int cpuCores) {
        this.cpuCores = cpuCores;
    }
    
    @JsonProperty("CPU")
    public List<CPUBean> getCPU() {
        return this.cpu;
    }
    
    @JsonProperty("CPU")
    public void setCPU(List<CPUBean> cpu) {
        this.cpu = cpu;
    }
    
    @JsonProperty("Memory")
    public List<MemoryBean> getMemory() {
        return memory;
    }
    
    @JsonProperty("Memory")
    public void setMemory(List<MemoryBean> memory) {
        this.memory = memory;
    }
    
    @JsonProperty("Disk")
    public List<DiskBean> getDisk() {
        return disk;
    }
    
    @JsonProperty("Disk")
    public void setDisk(List<DiskBean> disk) {
        this.disk = disk;
    }
    
    @JsonProperty("PSU")
    public List<PSUBean> getPSU() {
        return this.psu;
    }
    
    @JsonProperty("PSU")
    public void setPSU(List<PSUBean> psu) {
        this.psu = psu;
    }
    
    @JsonProperty("Fan")
    public List<FanBean> getFan() {
        return this.fan;
    }
    
    @JsonProperty("Fan")
    public void setFan(List<FanBean> fan) {
        this.fan = fan;
    }
    
    public List<BoardBean> getBoard() {
        return board;
    }
    
    public void setBoard(List<BoardBean> board) {
        this.board = board;
    }
    
    public String getProductSn() {
        return productSn;
    }
    
    public void setProductSn(String productSn) {
        this.productSn = productSn;
    }
    
    public String getBmcMacAddr() {
        return bmcMacAddr;
    }
    
    public void setBmcMacAddr(String bmcMacAddr) {
        this.bmcMacAddr = bmcMacAddr;
    }
    
    public String getMode() {
        return mode;
    }
    
    public void setMode(String mode) {
        this.mode = mode;
    }
    
    @JsonProperty("RAID")
    public List<RAIDBean> getRAID() {
        return this.raid;
    }
    
    @JsonProperty("RAID")
    public void setRAID(List<RAIDBean> raid) {
        this.raid = raid;
    }
    
    public List<NetworkCardBean> getNetworkCard() {
        return networkCard;
    }
    
    public void setNetworkCard(List<NetworkCardBean> networkCard) {
        this.networkCard = networkCard;
    }
    
    @JsonProperty("PCIE")
    public List<PCIEBean> getPCIE() {
        return this.pcie;
    }
    
    @JsonProperty("PCIE")
    public void setPCIE(List<PCIEBean> pcie) {
        this.pcie = pcie;
    }
    
    
    @JsonProperty("Mezz")
    public List<MezzBean> getMezz() {
        return mezz;
    }

    @JsonProperty("Mezz")
    public void setMezz(List<MezzBean> mezz) {
        this.mezz = mezz;
    }

    public String getLocation() {
        return location;
    }
    
    public void setLocation(String location) {
        this.location = location;
    }
    
    public String getManufacturer() {
        return manufacturer;
    }
    
    public void setManufacturer(String manufacturer) {
        this.manufacturer = manufacturer;
    }
    
    public String getVersion() {
        return version;
    }
    
    public void setVersion(String version) {
        this.version = version;
    }
    
    /**
     * 默认的构造方法.
     */
    public ServerDeviceDetailBean(){
        
    }
    
    /**
     * 根据列表对象构造一个的服务详细信息对象.
     * @param bean 服务器实体类
     */
    public ServerDeviceDetailBean(ServerDeviceBean bean) {
        this.dn = bean.getDn();
        this.name = bean.getServerName();
        this.type = bean.getServerModel();
        this.ipAddress = bean.getIpAddress();
        this.desc = bean.getDescription();
        this.manufacturer = bean.getManufacturer();
        this.productSn = bean.getProductSn();
        this.location = bean.getLocation();
        this.uuid = bean.getUuid();
        this.status = bean.getStatus();
        this.version = bean.getVersion();
    }
    
    
    @Override
    public String toString() {
        try {
            //转换为json字符串
            ObjectWriter ow = new ObjectMapper().writer();//.withDefaultPrettyPrinter()
            String json = ow.writeValueAsString(this);
            return json;
        } catch (JsonProcessingException e) {
            return ""; 
        }
        
    }
    
    @Override
    public ResourceKey convert2Resource(String id, String adapterKind, 
            Map<ResourceKey, List<MetricData>> metricsByResource) {
        
        ResourceKey resourceKey = 
                new ResourceKey(this.ipAddress, Constant.KIND_SERVER_DEVICE, adapterKind);
        ResourceIdentifierConfig dnIdentifier = 
                new ResourceIdentifierConfig(Constant.ATTR_ID, id + name, true);
        resourceKey.addIdentifier(dnIdentifier);
        long timestamp = System.currentTimeMillis();
        
        List<MetricData> metricDataList = new ArrayList<>();
        //写入资源属性
        metricDataList.add(new MetricData(
                new MetricKey(true,Constant.ATTR_DN), timestamp, this.dn));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_IP_ADDRESS), timestamp, this.ipAddress));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_NAME), timestamp, this.name));
        metricDataList.add(new MetricData(
                new MetricKey(true,Constant.ATTR_TYPE), timestamp, this.type));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_UUID), timestamp, this.uuid));
        metricDataList.add(new MetricData(
                new MetricKey(false, Constant.ATTR_STATUS), timestamp, 
                ConvertUtils.convertHealthState(this.status)));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_DESC), timestamp, this.desc));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_PRODUCT_SN), timestamp, this.productSn));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_MANUFACTURE), timestamp, this.manufacturer));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_VERSION), timestamp, this.version));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_LOCATION), timestamp, this.location));
        
        //非机框服务器不需要写入以下属性
        if (this.memoryCapacity != null) {
            metricDataList.add(new MetricData(
                    new MetricKey(true, Constant.ATTR_MEMORY_CAPACITY), timestamp, this.memoryCapacity));
            metricDataList.add(new MetricData(
                    new MetricKey(true, Constant.ATTR_MODEL), timestamp, this.mode));
            metricDataList.add(new MetricData(
                    new MetricKey(true, Constant.ATTR_BMC_MAC_ADDR), timestamp, this.bmcMacAddr));
            metricDataList.add(new MetricData(
                    new MetricKey(true, Constant.ATTR_CPU_NUMS), timestamp, this.cpuNums));
            metricDataList.add(new MetricData(
                    new MetricKey(true, Constant.ATTR_CPU_CORES), timestamp, this.cpuCores));
        }
        
        metricsByResource.put(resourceKey, metricDataList);
        
        return resourceKey;
    }
    
    /**
     * 创建分组对象resource key.
     * @param groupName 分组名字
     * @param kindKey 类型
     * @param childKeyList 下级资源列表
     * @param relationshipsByResource 关系列表
     * @param adapterKind adapter名字
     * @return 资源key
     */
    public ResourceKey createGroupKey(String groupName, String kindKey, 
            List<ResourceKey> childKeyList, 
            Map<ResourceKey, List<ResourceKey>> relationshipsByResource,
            String adapterKind) {
        
        ResourceKey groupKey = new ResourceKey(groupName,kindKey,adapterKind);
        //设定唯一的标识，保证同名的资源可以正常显示
        ResourceIdentifierConfig dnIdentifier = 
                new ResourceIdentifierConfig(Constant.ATTR_ID, this.dn + groupName, true);
        groupKey.addIdentifier(dnIdentifier);
        //关联resource和下级的child resource
        relationshipsByResource.put(groupKey, childKeyList);
        return groupKey;
    }
    
}
