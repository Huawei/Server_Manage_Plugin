package com.huawei.vcenterpluginui.constant;

public final class SyncStatus {
	public static final String STATUS_FINISHED = "FINISHED";
	public static final String STATUS_HW_FAILED = "HW_FAILED";
	public static final String STATUS_CREATED = "CREATED";
	public static final String STATUS_SYNC_FAILED = "SYNC_FAILED";
	public static final String STATUS_HW_PARTION_FAILED = "PARTION_FAILED";

	private SyncStatus() {
	}
	
	public static String getStatus(String oldStatus, String taskStatus,String taskResult, String taskCode){
		if ("Success".equals(taskResult)) {
			return STATUS_FINISHED;
		}
        if ("Failed".equals(taskResult)) {
			return STATUS_HW_FAILED;
		}
        if ("Partion Success".equals(taskResult)) {
			return STATUS_HW_PARTION_FAILED;
		}
        if ("Complete".equals(taskStatus)) {
			return STATUS_FINISHED;
		}
        if ("Running".equals(taskStatus)) {
			return STATUS_CREATED;
		}
		if (taskCode != null && !taskCode.isEmpty() && !"0".equals(taskCode)) {
			return STATUS_HW_FAILED;
		}

        return oldStatus;
	}
}
