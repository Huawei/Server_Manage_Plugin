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
 * PSU实体类.
 * @author harbor
 *
 */
@JsonIgnoreProperties(ignoreUnknown = true)
public class PSUBean implements TreeNodeResource {
    
    private String name;
    
    private String inputPower;
    
    private String manufacture;
    
    private String version;
    
    private int healthState;
    
    private int inputMode;
    
    private String moId;
    
    private String uuid;
    
    private int presentState;
    
    private int powerProtocol;
    
    private String ratePower;
    
    private String model;
    
    public String getName() {
        return name;
    }
    
    public void setName(String name) {
        this.name = name;
    }
    
    public String getInputPower() {
        return inputPower;
    }
    
    public void setInputPower(String inputPower) {
        this.inputPower = inputPower;
    }
    
    public String getManufacture() {
        return manufacture;
    }
    
    public void setManufacture(String manufacture) {
        this.manufacture = manufacture;
    }
    
    public String getVersion() {
        return version;
    }
    
    public void setVersion(String version) {
        this.version = version;
    }
    
    public int getHealthState() {
        return healthState;
    }
    
    public void setHealthState(int healthState) {
        this.healthState = healthState;
    }
    
    public int getInputMode() {
        return inputMode;
    }
    
    public void setInputMode(int inputMode) {
        this.inputMode = inputMode;
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
    
    public int getPowerProtocol() {
        return powerProtocol;
    }
    
    public void setPowerProtocol(int powerProtocol) {
        this.powerProtocol = powerProtocol;
    }
    
    public String getRatePower() {
        return ratePower;
    }
    
    public void setRatePower(String ratePower) {
        this.ratePower = ratePower;
    }
    
    public String getModel() {
        return model;
    }
    
    public void setModel(String model) {
        this.model = model;
    }
    
    @Override
    public ResourceKey convert2Resource(String id, String adapterKind, 
            Map<ResourceKey, List<MetricData>> metricsByResource) {
        
        ResourceKey resourceKey = new ResourceKey(this.name, Constant.KIND_PSU, adapterKind);
        
        //设定唯一的标识，保证同名的资源可以正常显示
        ResourceIdentifierConfig dnIdentifier = new ResourceIdentifierConfig("id", id + this.uuid, true);
        resourceKey.addIdentifier(dnIdentifier);
        long timestamp = System.currentTimeMillis();
        
        List<MetricData> metricDataList = new ArrayList<>();
        //写入resource属性
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_NAME), timestamp, this.name));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_INPUTPOWER), timestamp, 
                ConvertUtils.convertPower(this.inputPower)));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_MANUFACTURE), timestamp, this.manufacture));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_VERSION), timestamp, this.version));
        metricDataList.add(new MetricData(
                new MetricKey(false, Constant.ATTR_HEALTHSTATE), timestamp, 
                ConvertUtils.convertHealthState(this.healthState)));
        metricDataList.add(new MetricData(new MetricKey(true, Constant.ATTR_INPUTMODE), timestamp, 
                ConvertUtils.convertInputMode(this.inputMode)));
        
        metricDataList.add(new MetricData(
                new MetricKey(true,Constant.ATTR_MOID), timestamp, this.moId));
        metricDataList.add(new MetricData(
                new MetricKey(true,Constant.ATTR_UUID), timestamp, this.uuid));
        metricDataList.add(new MetricData(new MetricKey(true,Constant.ATTR_PRESENTSTATE), timestamp, 
                ConvertUtils.convertPresentState(this.presentState)));
        metricDataList.add(new MetricData(new MetricKey(true, Constant.ATTR_POWER_PROTOCOL), timestamp,
                ConvertUtils.convertPowerProtocol(this.powerProtocol)));
        metricDataList.add(new MetricData(new MetricKey(true, Constant.ATTR_RATE_POWER), timestamp, 
                ConvertUtils.convertPower(this.ratePower)));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_MODEL), timestamp, this.model));
        
        //关联key和属性值
        metricsByResource.put(resourceKey, metricDataList);
        return resourceKey;
    }
    
}
