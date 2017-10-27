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
 * Board 实体类.
 * @author harbor
 *
 */
@JsonIgnoreProperties(ignoreUnknown = true)
public class BoardBean implements TreeNodeResource {
    
    private String name;
    
    private int type;
    
    private String sn;
    
    private String partNumber;
    
    private String manufacture;
    
    private String manuTime;
    
    private int healthState;
    
    private String uuid;
    
    private String moId;
    
    private int presentState = 1;
    
    public int getPresentState() {
        return presentState;
    }
    
    public void setPresentState(int presentState) {
        this.presentState = presentState;
    }
    
    
    public String getName() {
        return name;
    }
    
    public void setName(String name) {
        this.name = name;
    }
    
    
    public int getType() {
        return type;
    }
    
    public void setType(int type) {
        this.type = type;
    }
    
    public String getSn() {
        return sn;
    }
    
    public void setSn(String sn) {
        this.sn = sn;
    }
    
    public String getPartNumber() {
        return partNumber;
    }
    
    public void setPartNumber(String partNumber) {
        this.partNumber = partNumber;
    }
    
    public String getManufacture() {
        return manufacture;
    }
    
    public void setManufacture(String manufacture) {
        this.manufacture = manufacture;
    }
    
    public String getManuTime() {
        return manuTime;
    }
    
    public void setManuTime(String manuTime) {
        this.manuTime = manuTime;
    }
    
    public int getHealthState() {
        return healthState;
    }
    
    public void setHealthState(int healthState) {
        this.healthState = healthState;
    }
    
    public String getUuid() {
        return uuid;
    }
    
    public void setUuid(String uuid) {
        this.uuid = uuid;
    }
    
    public String getMoId() {
        return moId;
    }
    
    public void setMoId(String moId) {
        this.moId = moId;
    }
    
    @Override
    public ResourceKey convert2Resource(String id, String adapterKind, Map<ResourceKey, 
            List<MetricData>> metricsByResource) {
        
        ResourceKey resourceKey = new ResourceKey(this.name, Constant.KIND_BOARD, adapterKind);
        //设定唯一的标识，保证同名的资源可以正常显示
        ResourceIdentifierConfig dnIdentifier = 
                new ResourceIdentifierConfig(Constant.ATTR_ID, id + this.uuid, true);
        resourceKey.addIdentifier(dnIdentifier);
        long timestamp = System.currentTimeMillis();
        
        List<MetricData> metricDataList = new ArrayList<>();
        
        //写入resource属性
        metricDataList.add(
                new MetricData(new MetricKey(true).add(Constant.ATTR_NAME), timestamp, this.name));
        metricDataList.add(
                new MetricData(new MetricKey(true).add(Constant.ATTR_TYPE), timestamp, 
                        ConvertUtils.convertBoardType(this.type)));
        metricDataList.add(
                new MetricData(new MetricKey(true).add(Constant.ATTR_SN), timestamp, this.sn));
        metricDataList.add(new MetricData(
                new MetricKey(true).add(Constant.ATTR_PART_NUMBER), timestamp, this.partNumber));
        metricDataList.add(new MetricData(
                new MetricKey(true).add(Constant.ATTR_MANUFACTURE), timestamp, this.manufacture));
        metricDataList.add(
                new MetricData(new MetricKey(true).add(Constant.ATTR_MANUTIME), timestamp, this.manuTime));
        
        metricDataList.add(
                new MetricData(new MetricKey(true).add(Constant.ATTR_UUID), timestamp, this.uuid));
        metricDataList.add(
                new MetricData(new MetricKey(true).add(Constant.ATTR_MOID), timestamp, this.moId));
        metricDataList.add(
                new MetricData(new MetricKey(true).add(Constant.ATTR_PRESENTSTATE), timestamp, 
                        ConvertUtils.convertPresentState(this.presentState)));
        
        metricDataList.add(
                new MetricData(new MetricKey(false).add(Constant.ATTR_HEALTHSTATE), timestamp, 
                        ConvertUtils.convertHealthState(this.healthState)));
        
        
        //关联key和属性值
        metricsByResource.put(resourceKey, metricDataList);
        
        return resourceKey;
    }
    
    
    
}
