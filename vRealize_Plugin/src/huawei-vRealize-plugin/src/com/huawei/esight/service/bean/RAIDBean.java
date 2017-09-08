package com.huawei.esight.service.bean;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.huawei.adapter.util.ConvertUtils;
import com.integrien.alive.common.adapter3.MetricData;
import com.integrien.alive.common.adapter3.MetricKey;
import com.integrien.alive.common.adapter3.ResourceKey;
import com.integrien.alive.common.adapter3.config.ResourceIdentifierConfig;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * RAID实体类
 * Created by harbor on 7/18/2017.
 */
@JsonIgnoreProperties(ignoreUnknown = true)
public class RAIDBean implements TreeNodeResource {
    private String name;
    private int healthState;
    private String raidType;
    private String interfaceType;
    private String bbuType;
    private String moId;
    private String uuid;
    
    public String getName() {
        return name;
    }
    
    public void setName(String name) {
        this.name = name;
    }
    
    public int getHealthState() {
        return healthState;
    }
    
    public void setHealthState(int healthState) {
        this.healthState = healthState;
    }
    
    public String getRaidType() {
        return raidType;
    }
    
    public void setRaidType(String raidType) {
        this.raidType = raidType;
    }
    
    public String getInterfaceType() {
        return interfaceType;
    }
    
    public void setInterfaceType(String interfaceType) {
        this.interfaceType = interfaceType;
    }
    
    public String getBbuType() {
        return bbuType;
    }
    
    public void setBbuType(String bbuType) {
        this.bbuType = bbuType;
    }
    
    public String getMoId() {
        return moId;
    }
    
    public void setMoId(String moId) {
        this.moId = moId;
    }
    
    public String getUuid() {
        return uuid;
    }
    
    public void setUuid(String uuid) {
        this.uuid = uuid;
    }
    
    @Override
    public ResourceKey convert2Resource(String dn, String adapterKind,
            Map<ResourceKey, List<MetricData>> metricsByResource) {
        
        //生成resource key
        ResourceKey resourceKey = new ResourceKey(this.name, Constant.KIND_RAID, adapterKind);
        //设定唯一的标识，保证同名的资源可以正常显示
        ResourceIdentifierConfig dnIdentifier = 
                new ResourceIdentifierConfig(Constant.ATTR_ID, dn + name, true);
        resourceKey.addIdentifier(dnIdentifier);
        long timestamp = System.currentTimeMillis();
        
        List<MetricData> metricDataList = new ArrayList<>();
        
        //写入resource属性
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_NAME), timestamp, this.name));
        metricDataList.add(new MetricData(
                new MetricKey(false, Constant.ATTR_HEALTHSTATE), timestamp, 
                ConvertUtils.convertHealthState4Raid(this.healthState)));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_RAID_TYPE), timestamp, this.raidType));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_INTERFACE_TYPE), timestamp, 
                ConvertUtils.covnertInterfaceType(this.interfaceType)));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_BBU_TYPE), timestamp, this.bbuType));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_MOID), timestamp, this.moId));
        
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_UUID), timestamp, this.uuid));
        
        //关联key和属性值
        metricsByResource.put(resourceKey, metricDataList);
        return resourceKey;
        
    }
}
