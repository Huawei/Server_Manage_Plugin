package com.huawei.esight.service.bean;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import com.huawei.adapter.util.ConvertUtils;
import com.integrien.alive.common.adapter3.MetricData;
import com.integrien.alive.common.adapter3.MetricKey;
import com.integrien.alive.common.adapter3.ResourceKey;
import com.integrien.alive.common.adapter3.config.ResourceIdentifierConfig;

public class MezzBean implements TreeNodeResource {
    
    private String name;
    
    private int mezzHealthStatus;
    
    private int presentState;
    
    private String mezzInfo;
    
    private String mezzLocation;
    
    private String mezzMac;
    
    private String moId;
    
    private String uuid;    
    

    public String getName() {
        return name;
    }



    public void setName(String name) {
        this.name = name;
    }



    public int getMezzHealthStatus() {
        return mezzHealthStatus;
    }



    public void setMezzHealthStatus(int mezzHealthStatus) {
        this.mezzHealthStatus = mezzHealthStatus;
    }



    public int getPresentState() {
        return presentState;
    }



    public void setPresentState(int presentState) {
        this.presentState = presentState;
    }



    public String getMezzInfo() {
        return mezzInfo;
    }



    public void setMezzInfo(String mezzInfo) {
        this.mezzInfo = mezzInfo;
    }



    public String getMezzLocation() {
        return mezzLocation;
    }



    public void setMezzLocation(String mezzLocation) {
        this.mezzLocation = mezzLocation;
    }



    public String getMezzMac() {
        return mezzMac;
    }



    public void setMezzMac(String mezzMac) {
        this.mezzMac = mezzMac;
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

    /**
     * 健康状态转换处理.
     * @param healthState 健康状态
     * @return 转换后的字符串
     */
    public String convertHealthState() {
       if (this.mezzHealthStatus == 1) {
    	   return "Normal";
       } else if (this.mezzHealthStatus == -2 || this.mezzHealthStatus == 5 || this.presentState == -2) {
    	   return "Unknown";
       } else {
    	   return "Faulty";
       } 
    }

    @Override
    public ResourceKey convert2Resource(String id, String adapterKind,
            Map<ResourceKey, List<MetricData>> metricsByResource) {
        
        ResourceKey resourceKey = new ResourceKey(this.name, Constant.KIND_MEZZ, adapterKind);
        //设定唯一的标识，保证同名的资源可以正常显示
        ResourceIdentifierConfig dnIdentifier = 
                new ResourceIdentifierConfig(Constant.ATTR_ID, id + name, true);
        resourceKey.addIdentifier(dnIdentifier);
        long timestamp = System.currentTimeMillis();
        
        List<MetricData> metricData = new ArrayList<>();
        
        //写入resource属性
        metricData.add(
                new MetricData(new MetricKey(true).add(Constant.ATTR_NAME), timestamp, this.name));
        metricData.add(
                new MetricData(new MetricKey(false).add(Constant.ATTR_MEZZ_HEALTH_STATUS), 
                        timestamp, convertHealthState()));
        metricData.add(
                new MetricData(new MetricKey(true).add(Constant.ATTR_PRESENTSTATE), timestamp, 
                        ConvertUtils.convertPresentState(this.presentState)));
        
        metricData.add(new MetricData(new MetricKey(true).add(Constant.ATTR_MEZZ_INFO), 
                timestamp, this.mezzInfo));
        
        metricData.add(
                new MetricData(new MetricKey(true).add(Constant.ATTR_MEZZ_LOCATION), 
                        timestamp, this.mezzLocation));
        metricData.add(
                new MetricData(new MetricKey(true).add(Constant.ATTR_MEZZ_MAC), 
                        timestamp, this.mezzMac));
        metricData.add(
                new MetricData(new MetricKey(true).add(Constant.ATTR_MOID), timestamp, this.moId));
        metricData.add(
                new MetricData(new MetricKey(true).add(Constant.ATTR_UUID), timestamp, this.uuid));
        
        //关联key和属性值
        metricsByResource.put(resourceKey, metricData);
        
        return resourceKey;
    }

    

}
