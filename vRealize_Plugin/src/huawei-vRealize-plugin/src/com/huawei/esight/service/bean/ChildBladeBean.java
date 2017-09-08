package com.huawei.esight.service.bean;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;

/**
 * child blades实体类.
 * @author harbor
 *
 */
@JsonIgnoreProperties(ignoreUnknown = true)
public class ChildBladeBean {
    
    private String dn;
    
    private String ipAddress;
    
    private String version;
    
    private String location;
    
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
    
    public String getVersion() {
        return version;
    }
    
    public void setVersion(String version) {
        this.version = version;
    }
    
    public String getLocation() {
        return location;
    }
    
    public void setLocation(String location) {
        this.location = location;
    }
    
    
    
}
