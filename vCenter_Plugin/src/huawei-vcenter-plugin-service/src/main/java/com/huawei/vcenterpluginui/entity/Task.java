package com.huawei.vcenterpluginui.entity;

import java.io.Serializable;
import java.util.List;

public class Task implements Serializable{
	private static final long serialVersionUID = -9063117479397179009L;
	
	private int id;
	private int hwEsighthostId;
	private String taskName;
	private String softwareSourceName;
	private String templates;
	private String deviceIp;
	private String taskStatus;
	private int taskProgress;  //进度
	private String taskResult;
	private String taskCode;
	private String errorDetail;
	private String syncStatus;    //状态
	private String taskType;
	private int reservedInt1;
	private int reservedInt2;
	private String reservedStr1; //升级包类型
	private String reservedStr2;
	private String lastModify;    //时间
	private String createTime;
	
	private List<TaskResource> deviceDetails;
	public int getId() {
		return id;
	}
	public void setId(int id) {
		this.id = id;
	}
	public int getHwEsighthostId() {
		return hwEsighthostId;
	}
	public void setHwEsighthostId(int hwEsighthostId) {
		this.hwEsighthostId = hwEsighthostId;
	}
	public String getTaskName() {
		return taskName;
	}
	public void setTaskName(String taskName) {
		this.taskName = taskName;
	}
	public String getSoftwareSourceName() {
		return softwareSourceName;
	}
	public void setSoftwareSourceName(String softwareSourceName) {
		this.softwareSourceName = softwareSourceName;
	}
	public String getTemplates() {
		return templates;
	}
	public void setTemplates(String templates) {
		this.templates = templates;
	}
	public String getDeviceIp() {
		return deviceIp;
	}
	public void setDeviceIp(String deviceIp) {
		this.deviceIp = deviceIp;
	}
	public String getTaskStatus() {
		return taskStatus;
	}
	public void setTaskStatus(String taskStatus) {
		this.taskStatus = taskStatus;
	}
	public int getTaskProgress() {
		return taskProgress;
	}
	public void setTaskProgress(int taskProgress) {
		this.taskProgress = taskProgress;
	}
	public String getTaskResult() {
		return taskResult;
	}
	public void setTaskResult(String taskResult) {
		this.taskResult = taskResult;
	}
	public String getTaskCode() {
		return taskCode;
	}
	public void setTaskCode(String taskCode) {
		this.taskCode = taskCode;
	}
	public String getErrorDetail() {
		return errorDetail;
	}
	public void setErrorDetail(String errorDetail) {
		this.errorDetail = errorDetail;
	}
	public String getSyncStatus() {
		return syncStatus;
	}
	public void setSyncStatus(String syncStatus) {
		this.syncStatus = syncStatus;
	}
	public String getTaskType() {
		return taskType;
	}
	public void setTaskType(String taskType) {
		this.taskType = taskType;
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
	public String getLastModify() {
		return lastModify;
	}
	public void setLastModify(String lastModify) {
		this.lastModify = lastModify;
	}
	public String getCreateTime() {
		return createTime;
	}
	public void setCreateTime(String createTime) {
		this.createTime = createTime;
	}
	public List<TaskResource> getDeviceDetails() {
		return deviceDetails;
	}
	public void setDeviceDetails(List<TaskResource> deviceDetails) {
		this.deviceDetails = deviceDetails;
	}
	@Override
	public String toString() {
		return "Task [id=" + id + ", hwEsighthostId=" + hwEsighthostId + ", taskName=" + taskName + ", softwareSourceName=" + softwareSourceName + ", templates=" + templates + ", deviceIp=" + deviceIp
				+ ", taskStatus=" + taskStatus + ", taskProgress=" + taskProgress + ", taskResult=" + taskResult + ", taskCode=" + taskCode + ", errorDetail=" + errorDetail + ", syncStatus="
				+ syncStatus + ", taskType=" + taskType + ", reservedInt1=" + reservedInt1 + ", reservedInt2=" + reservedInt2 + ", reservedStr1=" + reservedStr1 + ", reservedStr2=" + reservedStr2
				+ ", lastModify=" + lastModify + ", createTime=" + createTime + "]";
	}
	
}
