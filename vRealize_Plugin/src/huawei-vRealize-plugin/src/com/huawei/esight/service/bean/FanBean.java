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
 * Fan实体类.
 * @author harbor
 *
 */
@JsonIgnoreProperties(ignoreUnknown = true)
public class FanBean implements TreeNodeResource {
    
    private String name;
    
    private String rotate;
    
    private int rotatePercent;
    
    private int controlModel;
    
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
    
    public String getRotate() {
        return rotate;
    }
    
    public void setRotate(String rotate) {
        this.rotate = rotate;
    }
    
    public int getRotatePercent() {
        return rotatePercent;
    }
    
    public void setRotatePercent(int rotatePercent) {
        this.rotatePercent = rotatePercent;
    }
    
    public int getControlModel() {
        return controlModel;
    }
    
    public void setControlModel(int controlModel) {
        this.controlModel = controlModel;
    }
    
    public int getHealthState() {
        return healthState;
    }
    
    public void setHealthState(int healthState) {
        this.healthState = healthState;
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
        
        ResourceKey resourceKey = new ResourceKey(this.name, Constant.KIND_FAN, adapterKind);
        //设定唯一的标识，保证同名的资源可以正常显示
        ResourceIdentifierConfig dnIdentifier = 
                new ResourceIdentifierConfig(Constant.ATTR_ID, id + this.uuid, true);
        resourceKey.addIdentifier(dnIdentifier);
        
        long timestamp = System.currentTimeMillis();
        
        List<MetricData> metricDataList = new ArrayList<>();
        //写入resource属性
        metricDataList.add(
                new MetricData(new MetricKey(true, Constant.ATTR_NAME), timestamp, this.name));
        metricDataList.add(
                new MetricData(new MetricKey(false, Constant.ATTR_HEALTHSTATE), timestamp, 
                        ConvertUtils.convertHealthState(this.healthState)));
        metricDataList.add(
                new MetricData(new MetricKey(true, Constant.ATTR_ROTATE), timestamp, this.rotate));
        metricDataList.add(new MetricData(new MetricKey(true, Constant.ATTR_ROTATEPERCENT), timestamp, 
                ConvertUtils.convertRotatePercent(this.rotatePercent)));
        metricDataList.add(new MetricData(new MetricKey(true, Constant.ATTR_CONTROLMODEL), timestamp, 
                ConvertUtils.convertControlModel(this.controlModel)));
        
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
