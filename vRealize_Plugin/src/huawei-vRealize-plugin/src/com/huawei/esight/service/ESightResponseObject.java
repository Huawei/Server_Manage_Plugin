package com.huawei.esight.service;

/**
 * eSight REST API 返加结果对象， data为字符串 .
 * @author harbor
 *
 */
public class ESightResponseObject {
    
    /**
     * 状态标码，0为正常，其他值表示错误.
     */
    private int code;
    
    /**
     * 返回数据.
     */
    private String data;
    
    /**
     * 结果描述.
     */
    private String description;
    
    public int getCode() {
        return code;
    }
    
    public void setCode(int code) {
        this.code = code;
    }
    
    public String getData() {
        return data;
    }
    
    public void setData(String data) {
        this.data = data;
    }
    
    public String getDescription() {
        return description;
    }
    
    public void setDescription(String description) {
        this.description = description;
    }
    
    
    
}
