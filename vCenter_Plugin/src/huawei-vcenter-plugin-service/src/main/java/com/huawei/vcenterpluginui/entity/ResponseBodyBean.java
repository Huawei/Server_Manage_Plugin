package com.huawei.vcenterpluginui.entity;

/**
 * Created by hyuan on 2017/5/26.
 */
public class ResponseBodyBean {
    private String code;
    private Object data;
    private String description;

    public ResponseBodyBean() {
    }

    public ResponseBodyBean(String code, Object data, String description) {
        this.code = code;
        this.data = data;
        this.description = description;
    }

    public Object getData() {
        return data;
    }

    public void setData(Object data) {
        this.data = data;
    }

    public String getDescription() {
        return description;
    }

    public void setDescription(String description) {
        this.description = description;
    }

    public String getCode() {
        return code;
    }

    public void setCode(String code) {
        this.code = code;
    }

    @Override
    public String toString() {
        return "ResponseBodyBean{" +
                "code='" + code + '\'' +
                ", data='" + data + '\'' +
                ", description='" + description + '\'' +
                '}';
    }
}
