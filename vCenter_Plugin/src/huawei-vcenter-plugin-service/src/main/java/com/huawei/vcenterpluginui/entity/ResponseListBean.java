package com.huawei.vcenterpluginui.entity;

/**
 * Created by hyuan on 2017/5/26.
 */
public class ResponseListBean extends ResponseBodyBean{
    private int totalNum;

    public ResponseListBean() {
    }
    
    public ResponseListBean(ResponseBodyBean responseBodyBean, int totalNum){
    	this.setCode(responseBodyBean.getCode());
    	this.setData(responseBodyBean.getData());
    	this.setDescription(responseBodyBean.getDescription());
    	this.totalNum = totalNum;
    }

	public int getTotalNum() {
		return totalNum;
	}

	public void setTotalNum(int totalNum) {
		this.totalNum = totalNum;
	}

}
