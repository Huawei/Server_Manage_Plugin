package com.huawei.vcenterpluginui.services;

import java.io.IOException;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.servlet.http.HttpSession;

import com.huawei.esight.api.rest.firmware.DeleteFirmwareApi;
import com.huawei.esight.api.rest.firmware.GetFirmwareDetailApi;
import com.huawei.esight.api.rest.firmware.GetFirmwareListApi;
import com.huawei.esight.api.rest.firmware.GetFirmwareProgressApi;
import com.huawei.esight.api.rest.firmware.GetFirmwareTaskDetailApi;
import com.huawei.esight.api.rest.firmware.GetFirmwareTaskDeviceDetailApi;
import com.huawei.esight.api.rest.firmware.PostFirmwareTaskApi;
import com.huawei.esight.api.rest.firmware.PostFirmwareUploadApi;
import com.huawei.esight.exception.EsightException;
import com.huawei.esight.utils.JsonUtil;
import com.huawei.vcenterpluginui.constant.ErrorPrefix;
import com.huawei.vcenterpluginui.constant.SyncStatus;
import com.huawei.vcenterpluginui.constant.TaskType;
import com.huawei.vcenterpluginui.dao.TaskDao;
import com.huawei.vcenterpluginui.entity.ESight;
import com.huawei.vcenterpluginui.entity.Task;
import com.huawei.vcenterpluginui.provider.SessionOpenIdProvider;

public class FirmwareApiServiceImpl extends ESightOpenApiService implements FirmwareApiService {
	
	private TaskDao taskDao;

	@Override
	public List<Map<String, Object>> upload(String data, HttpSession session) throws IOException, SQLException{		
		Map<String, Object> reqMap = JsonUtil.readAsMap(data);
        List<String> eSightIPs = (List<String>)reqMap.get("esights");
        Map<String, Object> condition = (Map)reqMap.get("data");
		List<Map<String, Object>> dataMapList = new ArrayList<Map<String, Object>>();
		for (String ip : eSightIPs) {
			Map<String, Object> responseData = new HashMap<String, Object>();
			responseData.put("esight", ip);
			try {
				ESight esight = getESightByIp(ip);
				if (esight != null) {
					Map<String, Object> dataMap = new PostFirmwareUploadApi<Map>(esight, new SessionOpenIdProvider(esight, session)).doCall(condition, Map.class);
					responseData.putAll(dataMap);
				} else {
					responseData.putAll(getNoEsightMap());
				}
			} catch (EsightException e) {
				LOGGER.error(e.getMessage());
				responseData.putAll(getEsightExceptionMap(e));
			} catch (Exception e) {
				LOGGER.error(e.getMessage());
				responseData.putAll(getExceptionMap());
			}
			dataMapList.add(responseData);
		}
		//保存任务到本地
		setUploadTask((Map) reqMap.get("data"), dataMapList);
		return dataMapList;
	}

	@Override
	public String list(String esightIp, int pageNo, int pageSize, HttpSession session) throws SQLException{
		ESight esight = getESightByIp(esightIp);
		String response = new GetFirmwareListApi<String>(esight,new SessionOpenIdProvider(esight,session)).doCall(String.valueOf(pageNo), String.valueOf(pageSize), String.class);
		return response;
	}

	@Override
	public String detail(String ip, String basepackageName, HttpSession session) throws SQLException{
		ESight esight = getESightByIp(ip);
		String response = new GetFirmwareDetailApi<String>(esight,new SessionOpenIdProvider(esight,session)).doCall(basepackageName, String.class);
		return response;
	}

	private void setUploadTask(Map data, List<Map<String, Object>> dataMapList) throws SQLException{
		for (Map<String, Object> dataMap : dataMapList) {
			if (dataMap.get("data") != null) {
				ESight esight = eSightDao.getESightByIp(dataMap.get("esight").toString());
				Task task = new Task();
				task.setHwEsighthostId(esight.getId());
				task.setSoftwareSourceName((String)data.get("basepackageName"));
				task.setTaskProgress(0);
				task.setSyncStatus(SyncStatus.STATUS_CREATED);
				task.setTaskType(TaskType.TASK_TYPE_FIRMWARE.name());
				task.setReservedStr1((String)data.get("basepackageType"));
				task.setTaskName((String)((Map) dataMap.get("data")).get("taskName"));

				taskDao.saveTask(task);
			}
		}
	}

	@Override
	public List<Task> progressList(HttpSession session) throws SQLException{
		Map<String, Object> taskParam = new HashMap<String, Object>();
		taskParam.put("incompleted", true);
		taskParam.put("taskType", TaskType.TASK_TYPE_FIRMWARE.name());
		List<Task> taskList = taskDao.getIncompletedTaskList(taskParam);
		//同步任务进度
		for (Task task : taskList) {
			try {
				int id = task.getHwEsighthostId();
				ESight esight = eSightDao.getESightById(id);
				Map<String, Object> reqMap = new GetFirmwareProgressApi<Map>(esight, new SessionOpenIdProvider(esight, session)).doCall(task.getTaskName(), Map.class);
				Map<String, Object> dataMap = (Map<String, Object>) reqMap.get("data");
				String syncStatus = SyncStatus.getStatus(task.getSyncStatus(), (String) dataMap.get("taskStatus"), (String) dataMap.get("taskResult"), String.valueOf(dataMap.get("taskCode")));
				task.setSyncStatus(syncStatus);
				task.setTaskProgress((int) dataMap.get("taskProgress"));
				if (dataMap.get("taskCode") != null && !String.valueOf(dataMap.get("taskCode")).isEmpty()) {
					task.setTaskCode(ErrorPrefix.FIRMWARE_ERROR_PREFIX + String.valueOf(dataMap.get("taskCode")));
				}
				task.setTaskResult((String) dataMap.get("taskResult"));
				task.setErrorDetail((String) dataMap.get("errorDetail"));
				taskDao.saveTaskStatus(task);
			} catch (Exception e) {
				LOGGER.error(e.getMessage());
			}
		}
		// 同步时会更新状态，需要重新拿
		taskParam.remove("incompleted");
		taskParam.put("unSuccess", true);
		taskParam.put("order", "taskDate");
		taskParam.put("orderDesc", "true");
		return taskDao.getIncompletedTaskList(taskParam);
	}

	@Override
	public int deleteUploadFailed() throws SQLException{
		return taskDao.deleteFailedTask(TaskType.TASK_TYPE_FIRMWARE.name());
	}

	@Override
	public String postUpgradeTask(String data, HttpSession session) throws IOException, SQLException{
		Map<String, Object> reqMap = JsonUtil.readAsMap(data);
        String ip = (String)reqMap.get("esight");
		ESight esight = getESightByIp(ip);
		String response = new PostFirmwareTaskApi<String>(esight,new SessionOpenIdProvider(esight,session)).doCall((Map<String,Object>)reqMap.get("param"), String.class);

		setUpgradeTask(esight.getId(), ((Map) reqMap.get("param")).get("basepackageName").toString(), ((Map) reqMap.get("param")).get("firmwareList").toString(), ((Map) reqMap.get("param")).get("dn").toString(), response);
		return response;
	}

	@Override
	public int deleteUpgradeTask(int taskId) throws SQLException{
		return taskDao.deleteTaskById(taskId);
	}

	@Override
	public Map<String,Object> upgradeTaskList(String esightIp, String taskName, String taskStatus, int pageNo, int pageSize, String order, String orderDesc, HttpSession session) throws SQLException, IOException{
		Map<String, Object> taskParam = new HashMap<String, Object>();
		taskParam.put("incompleted", true);
		taskParam.put("taskType", TaskType.TASK_TYPE_DEPLOYFIRMWARE.name());

		List<Task> taskList = taskDao.getIncompletedTaskList(taskParam);

		for (Task task : taskList) {
			try{
			int id = task.getHwEsighthostId();
			ESight esight = eSightDao.getESightById(id);
			String response = new GetFirmwareTaskDetailApi<String>(esight,new SessionOpenIdProvider(esight,session)).doCall(task.getTaskName(), String.class);

			Map<String, Object> reqMap = JsonUtil.readAsMap(response);
			Map<String, Object> dataMap = (Map<String, Object>) reqMap.get("data");
			String syncStatus = SyncStatus.getStatus(task.getSyncStatus(), (String)dataMap.get("taskStatus"), (String)dataMap.get("taskResult"), (String)dataMap.get("taskCode"));
			task.setSyncStatus(syncStatus);
			task.setTaskProgress((int) dataMap.get("taskProgress"));
			task.setTaskCode((String)dataMap.get("taskCode"));
			task.setTaskResult((String)dataMap.get("taskResult"));
			taskDao.saveTaskStatus(task);
			}catch (Exception e) {
				LOGGER.error(e.getMessage());
//				task.setSyncStatus(SyncStatus.STATUS_SYNC_FAILED);
//				taskDao.saveTaskStatus(task);
			}
		}
		// 同步时会更新状态，需要重新拿
		taskParam.put("pageNo", pageNo);
		taskParam.put("pageSize", pageSize);
		if (taskName != null && !taskName.isEmpty()) {
			taskParam.put("taskName", taskName);
		}
		if (taskStatus != null && !taskStatus.isEmpty()) {
			taskParam.put("taskStatus", taskStatus);
		}
		if (order != null && !order.isEmpty()) {
			taskParam.put("order", order);
			taskParam.put("orderDesc", orderDesc);
		}
		taskParam.remove("incompleted");
		taskParam.put("esightIp", esightIp);
		List<Task> data = taskDao.getIncompletedTaskList(taskParam);
		int count = taskDao.getCountTaskList(taskParam);
		
		Map<String,Object> dataMap = new HashMap<String,Object>();
		dataMap.put("data", data);
		dataMap.put("count", count);
		
		return dataMap;
	}

	private void setUpgradeTask(int esightId, String taskName, String firmwareList,String dn, String responseBody) throws IOException, SQLException {
		Map<String, Object> reqMap = JsonUtil.readAsMap(responseBody);
		if ((int) reqMap.get("code") == RESULT_SUCCESS_CODE) {
			Task task = new Task();
			task.setHwEsighthostId(esightId);
			task.setSoftwareSourceName(taskName);
			task.setTaskProgress(0);
			task.setSyncStatus(SyncStatus.STATUS_CREATED);
			task.setTaskType(TaskType.TASK_TYPE_DEPLOYFIRMWARE.name());
			task.setTaskName(((Map) reqMap.get("data")).get("taskName").toString());
			task.setReservedStr1(firmwareList);
			task.setDeviceIp(dn);

			taskDao.saveTask(task);
		}
	}

	public void setTaskDao(TaskDao taskDao) {
		this.taskDao = taskDao;
	}

	@Override
	public String deviceDetail(String esightIp, String taskName, String dn, HttpSession session) throws SQLException {
		ESight esight = getESightByIp(esightIp);
		String response = new GetFirmwareTaskDeviceDetailApi<String>(esight,new SessionOpenIdProvider(esight,session)).doCall(taskName, dn, String.class);
		return response;
	}

	@Override
	public String deleteBasepackage(String esightIp, String basepackageName, HttpSession session) throws SQLException {
		ESight esight = getESightByIp(esightIp);
		String response = new DeleteFirmwareApi<String>(esight,new SessionOpenIdProvider(esight,session)).doCall(basepackageName,String.class);
		return response;
	}
}
