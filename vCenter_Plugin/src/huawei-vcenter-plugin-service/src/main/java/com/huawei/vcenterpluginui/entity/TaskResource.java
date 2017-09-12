package com.huawei.vcenterpluginui.entity;

import java.io.Serializable;

public class TaskResource implements Serializable{

	private static final long serialVersionUID = -9063117479397179009L;
	
	private String hwESightTaskID;
	private String dn;
	private String ipAddress;
	private String syncStatus;
	private String deviceResult;
	private String errorDetail;
	private int deviceProgress;
	private String taskType;
	private String errorCode;
	private int reservedInt1;
	private int reservedInt2;
	private String reservedStr1; 
	private String reservedStr2;
	private String lastModifyTime;
	private String createTime;
	
	public String getHwESightTaskID() {
		return hwESightTaskID;
	}
	public void setHwESightTaskID(String hwESightTaskID) {
		this.hwESightTaskID = hwESightTaskID;
	}
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
	public String getSyncStatus() {
		return syncStatus;
	}
	public void setSyncStatus(String syncStatus) {
		this.syncStatus = syncStatus;
	}
	public String getDeviceResult() {
		return deviceResult;
	}
	public void setDeviceResult(String deviceResult) {
		this.deviceResult = deviceResult;
	}
	public String getErrorDetail() {
		return errorDetail;
	}
	public void setErrorDetail(String errorDetail) {
		this.errorDetail = errorDetail;
	}
	public int getDeviceProgress() {
		return deviceProgress;
	}
	public void setDeviceProgress(int deviceProgress) {
		this.deviceProgress = deviceProgress;
	}
	public String getTaskType() {
		return taskType;
	}
	public void setTaskType(String taskType) {
		this.taskType = taskType;
	}
	public String getLastModifyTime() {
		return lastModifyTime;
	}
	public void setLastModifyTime(String lastModifyTime) {
		this.lastModifyTime = lastModifyTime;
	}
	public String getCreateTime() {
		return createTime;
	}
	public void setCreateTime(String createTime) {
		this.createTime = createTime;
	}
	
	public String getErrorCode() {
		return errorCode;
	}
	public void setErrorCode(String errorCode) {
		this.errorCode = errorCode;
	}
	public int getReservedInt1() {
		return reservedInt1;
	}
	public void setReservedInt1(int reservedInt1) {
		this.reservedInt1 = reservedInt1;
	}
	public int getReservedInt2() {
		return reservedInt2;
	}
	public void setReservedInt2(int reservedInt2) {
		this.reservedInt2 = reservedInt2;
	}
	public String getReservedStr1() {
		return reservedStr1;
	}
	public void setReservedStr1(String reservedStr1) {
		this.reservedStr1 = reservedStr1;
	}
	public String getReservedStr2() {
		return reservedStr2;
	}
	public void setReservedStr2(String reservedStr2) {
		this.reservedStr2 = reservedStr2;
	}
	@Override
	public String toString() {
		return "TaskResource [hwESightTaskID=" + hwESightTaskID + ", dn=" + dn + ", ipAddress=" + ipAddress + ", syncStatus=" + syncStatus + ", deviceResult=" + deviceResult + ", errorDetail="
				+ errorDetail + ", deviceProgress=" + deviceProgress + ", taskType=" + taskType + ", errorCode=" + errorCode + ", reservedInt1=" + reservedInt1 + ", reservedInt2=" + reservedInt2
				+ ", reservedStr1=" + reservedStr1 + ", reservedStr2=" + reservedStr2 + ", lastModifyTime=" + lastModifyTime + ", createTime=" + createTime + "]";
	}
	
	
}

