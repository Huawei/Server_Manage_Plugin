package com.huawei.esight.service.bean;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.ObjectWriter;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;

/**
 * 服务器列表对象类.
 * @author harbor
 *
 */
@JsonIgnoreProperties(ignoreUnknown = true)
public class ServerDeviceBean implements Comparator<ServerDeviceBean>, Serializable {
	
	private static final long serialVersionUID = -2982418617289654754L;

    private String dn;
    private String ipAddress;
    private String serverName;//name
    private String serverModel;//type
    private String manufacturer;
    private String productSn;
    private String location;
    private String uuid;
    private int status;
    private String description;
    private String version;
    private boolean childBlade = false;
    
    private List<ChildBladeBean> childBlades = new ArrayList<ChildBladeBean>();
    
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
    
    public String getServerName() {
        return serverName;
    }
    
    public void setServerName(String serverName) {
        this.serverName = serverName;
    }
    
    public String getServerModel() {
        return serverModel;
    }
    
    public void setServerModel(String serverModel) {
        this.serverModel = serverModel;
    }
    
    public String getManufacturer() {
        return manufacturer;
    }
    
    public void setManufacturer(String manufacturer) {
        this.manufacturer = manufacturer;
    }
    
    @JsonProperty("productSN")
    public String getProductSn() {
        return productSn;
    }
    
    @JsonProperty("productSN")
    public void setProductSn(String productSn) {
        this.productSn = productSn;
    }
    
    public String getLocation() {
        return location;
    }
    
    public void setLocation(String location) {
        this.location = location;
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
    
    public String getDescription() {
        return description;
    }
    
    public void setDescription(String description) {
        this.description = description;
    }
    
    
    public List<ChildBladeBean> getChildBlades() {
        return childBlades;
    }
    
    public void setChildBlades(List<ChildBladeBean> childBlades) {
        this.childBlades = childBlades;
    }
    
    public boolean isChildBlade() {
        return childBlade;
    }
    
    public void setChildBlade(boolean childBlade) {
        this.childBlade = childBlade;
    }
    
    public String getVersion() {
        return version;
    }
    
    public void setVersion(String version) {
        this.version = version;
    }
    
    /**
     * 检查是否为child blade.
     * @param dn 设备名称
     * @return 检查结果(true/false)
     */
    public boolean hasChild(String dn){
        
        for (ChildBladeBean childBladeBean : this.childBlades) {
        	if (childBladeBean.getDn().equals(dn)) {
                return true;
            }
        }
        
        return false;
    }
    
    @Override
    public int compare(ServerDeviceBean src, ServerDeviceBean target) {
        
        //对象排序规则：子服务必须要在机框服务器前面      
        if (src.hasChild(target.getDn())) {
            //target server is child
            target.setChildBlade(true);
            return 1;
        }
        
        if (target.hasChild(this.dn)) {
            //target server is parent
        	src.setChildBlade(true);
            return -1;
        }
        
        return 0;
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

}
