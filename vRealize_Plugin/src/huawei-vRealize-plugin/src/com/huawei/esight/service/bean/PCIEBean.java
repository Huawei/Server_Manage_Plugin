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
 * PCIE实体类.
 * Created by harbor on 7/18/2017.
 */
@JsonIgnoreProperties(ignoreUnknown = true)
public class PCIEBean implements TreeNodeResource {
    
    private String name;
    
    private int pcieSsdCardHealthStatus;
    
    private String pcieSsdCardLifeLeft;
    
    private String uuid;
    
    private String pciecardManufacturer;
    
    private String moId;
    
    public String getName() {
        return name;
    }
    
    public void setName(String name) {
        this.name = name;
    }
    
    public int getPcieSsdCardHealthStatus() {
        return pcieSsdCardHealthStatus;
    }
    
    public void setPcieSsdCardHealthStatus(int pcieSsdCardHealthStatus) {
        this.pcieSsdCardHealthStatus = pcieSsdCardHealthStatus;
    }
    
    public String getPcieSsdCardLifeLeft() {
        return pcieSsdCardLifeLeft;
    }
    
    public void setPcieSsdCardLifeLeft(String pcieSsdCardLifeLeft) {
        this.pcieSsdCardLifeLeft = pcieSsdCardLifeLeft;
    }
    
    public String getUuid() {
        return uuid;
    }
    
    public void setUuid(String uuid) {
        this.uuid = uuid;
    }
    
    public String getPciecardManufacturer() {
        return pciecardManufacturer;
    }
    
    public void setPciecardManufacturer(String pciecardManufacturer) {
        this.pciecardManufacturer = pciecardManufacturer;
    }
    
    public String getMoId() {
        return moId;
    }
    
    public void setMoId(String moId) {
        this.moId = moId;
    }
    
    @Override
    public ResourceKey convert2Resource(String dn, String adapterKind, 
            Map<ResourceKey, List<MetricData>> metricsByResource) {
        
        ResourceKey resourceKey = new ResourceKey(this.name, Constant.KIND_PCIE, adapterKind);
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
                new MetricKey(false, Constant.ATTR_PCIESSDCARD_HEALTHSTATUS), timestamp, 
                ConvertUtils.convertHealthState(this.pcieSsdCardHealthStatus)));
        metricDataList.add(new MetricData(new MetricKey(true, 
                Constant.ATTR_PCIESSDCARD_LIFELEFT), timestamp, this.pcieSsdCardLifeLeft));
        metricDataList.add(new MetricData(new MetricKey(true, 
                Constant.ATTR_UUID), timestamp, this.uuid));
        metricDataList.add(new MetricData(new MetricKey(true, 
                Constant.ATTR_PCIESSDCARD_MANYFACTURER), timestamp, this.pciecardManufacturer));
        metricDataList.add(new MetricData(
                new MetricKey(true, Constant.ATTR_MOID), timestamp, this.moId));
        //关联key和属性值
        metricsByResource.put(resourceKey, metricDataList);
        return resourceKey;
    }
}
