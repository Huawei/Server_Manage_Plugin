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
 * Memory实体类.
 * @author harbor
 *
 */
@JsonIgnoreProperties(ignoreUnknown = true)
public class MemoryBean implements TreeNodeResource {
    
    private String name;
    
    private String capacity;
    
    private String manufacture;
    
    private String frequency;
    
    private int healthState;
    
    private String moId;
    
    private String uuid;
    
    private int presentState;
    
    public String getName() {
        return name;
    }
    
    public void setName(String name) {
        this.name = name;
    }
    
    public String getCapacity() {
        return capacity;
    }
    
    public void setCapacity(String capacity) {
        this.capacity = capacity;
    }
    
    public String getManufacture() {
        return manufacture;
    }
    
    public void setManufacture(String manufacture) {
        this.manufacture = manufacture;
    }
    
    public int getHealthState() {
        return healthState;
    }
    
    public void setHealthState(int healthState) {
        this.healthState = healthState;
    }
    
    public String getFrequency() {
        return frequency;
    }
    
    public void setFrequency(String frequency) {
        this.frequency = frequency;
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
    
    public int getPresentState() {
        return presentState;
    }
    
    public void setPresentState(int presentState) {
        this.presentState = presentState;
    }
    
    @Override
    public ResourceKey convert2Resource(String id, String adapterKind, 
            Map<ResourceKey, List<MetricData>> metricsByResource) {
        
        ResourceKey resourceKey = new ResourceKey(this.name, Constant.KIND_MEMORY, adapterKind);
        //设定唯一的标识，保证同名的资源可以正常显示
        ResourceIdentifierConfig dnIdentifier = 
                new ResourceIdentifierConfig(Constant.ATTR_ID, id + name, true);
        resourceKey.addIdentifier(dnIdentifier);
        long timestamp = System.currentTimeMillis();
        
        List<MetricData> metricDataList = new ArrayList<>();
        //写入resource属性
        metricDataList.add(
                new MetricData(new MetricKey(true, Constant.ATTR_NAME), timestamp, this.name));
        metricDataList.add(
                new MetricData(new MetricKey(true, Constant.ATTR_CAPACITY), timestamp, this.capacity));
        metricDataList.add(
                new MetricData(
                        new MetricKey(true, Constant.ATTR_MANUFACTURE), timestamp, this.manufacture));
        metricDataList.add(
                new MetricData(new MetricKey(true, Constant.ATTR_FREQUENCY), timestamp, this.frequency));
        metricDataList.add(
                new MetricData(new MetricKey(false, Constant.ATTR_HEALTHSTATE), timestamp, 
                        ConvertUtils.convertHealthState(this.healthState)));
        
        metricDataList.add(
                new MetricData(new MetricKey(true, Constant.ATTR_MOID), timestamp, this.moId));
        metricDataList.add(
                new MetricData(new MetricKey(true, Constant.ATTR_UUID), timestamp, this.uuid));
        metricDataList.add(new MetricData(new MetricKey(true, Constant.ATTR_PRESENTSTATE), timestamp, 
                ConvertUtils.convertPresentState(this.presentState)));
        
        //关联key和属性值
        metricsByResource.put(resourceKey, metricDataList);
        return resourceKey;
    }
    
}
