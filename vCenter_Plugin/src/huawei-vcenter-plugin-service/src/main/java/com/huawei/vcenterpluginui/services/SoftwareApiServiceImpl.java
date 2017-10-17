package com.huawei.vcenterpluginui.services;

import java.io.IOException;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.servlet.http.HttpSession;
import com.huawei.esight.api.rest.softwaresource.DeleteSoftwareApi;
import com.huawei.esight.api.rest.softwaresource.GetSoftwareListApi;
import com.huawei.esight.api.rest.softwaresource.GetSoftwareProgressApi;
import com.huawei.esight.api.rest.softwaresource.PostSoftwareUploadApi;
import com.huawei.esight.exception.EsightException;
import com.huawei.esight.utils.JsonUtil;
import com.huawei.vcenterpluginui.constant.ErrorPrefix;
import com.huawei.vcenterpluginui.constant.SyncStatus;
import com.huawei.vcenterpluginui.constant.TaskType;
import com.huawei.vcenterpluginui.dao.TaskDao;
import com.huawei.vcenterpluginui.entity.ESight;
import com.huawei.vcenterpluginui.entity.Task;
import com.huawei.vcenterpluginui.provider.SessionOpenIdProvider;

public class SoftwareApiServiceImpl extends ESightOpenApiService implements SoftwareApiService {

	private TaskDao taskDao;

	@Override
	public List<Map<String, Object>> upload(String data, HttpSession session) throws IOException, SQLException {
		Map<String, Object> reqMap = JsonUtil.readAsMap(data);
        List<String> eSightIPs = (List<String>)reqMap.get("esights");
        Map<String,Object> condition = (Map<String,Object>)reqMap.get("data");
		List<Map<String, Object>> dataMapList = new ArrayList<Map<String, Object>>();
		for (String ip : eSightIPs) {
			Map<String, Object> responseData = new HashMap<String, Object>();
			responseData.put("esight", ip);
			try {
				ESight esight = getESightByIp(ip);
				if (esight != null) {
					Map<String, Object> dataMap = new PostSoftwareUploadApi<Map>(esight, new SessionOpenIdProvider(esight, session)).doCall(condition, Map.class);
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
		//任务信息保存本地
		setUploadTask((Map) reqMap.get("data"), dataMapList);

		return dataMapList;
	}

	@Override
	public List<Task> progressList(HttpSession session) throws SQLException {
		Map<String, Object> taskParam = new HashMap<String, Object>();
		taskParam.put("incompleted", true);
		taskParam.put("taskType", TaskType.TASK_TYPE_SOFTWARE.name());

		List<Task> taskList = taskDao.getIncompletedTaskList(taskParam);
		// 同步上传任务进度
		syncTaskProgress(taskList, session);

		// 同步时会更新状态，需要重新拿
		taskParam.remove("incompleted");
		taskParam.put("unSuccess", true);
		taskParam.put("order", "taskDate");
		taskParam.put("orderDesc", "true");
		return taskDao.getIncompletedTaskList(taskParam);
	}

	@Override
	public String list(String esightIp,int pageNo, int pageSize, HttpSession session) throws SQLException {
		ESight esight = getESightByIp(esightIp);
		String response = new GetSoftwareListApi<String>(esight, new SessionOpenIdProvider(esight, session)).doCall(String.valueOf(pageNo), String.valueOf(pageSize), String.class);
		return response;
	}

	@Override
	public int deleteUploadFailed() throws SQLException {
		return taskDao.deleteFailedTask(TaskType.TASK_TYPE_SOFTWARE.name());
	}

	@Override
	public String deleteSoftResource(String softwareName, String ip, HttpSession session) throws SQLException {
		ESight esight = getESightByIp(ip);
		String response = new DeleteSoftwareApi<String>(esight, new SessionOpenIdProvider(esight, session)).doCall(softwareName, String.class);
		return response;
	}

	private void setUploadTask(Map data, List<Map<String, Object>> dataMapList) throws SQLException {
		for (Map<String, Object> dataMap : dataMapList) {
			if (RESULT_SUCCESS_CODE == (int) dataMap.get("code")) {
				ESight esight = eSightDao.getESightByIp(dataMap.get("esight").toString());
				Task task = new Task();
				task.setHwEsighthostId(esight.getId());
				task.setSoftwareSourceName(data.get("softwareName").toString());
				task.setTaskProgress(0);
				task.setSyncStatus(SyncStatus.STATUS_CREATED);
				task.setTaskType(TaskType.TASK_TYPE_SOFTWARE.name());
				task.setTaskName(((Map) dataMap.get("data")).get("taskName").toString());

				taskDao.saveTask(task);
			}
		}
	}

	private void syncTaskProgress(List<Task> taskList, HttpSession session) throws SQLException {
		for (Task task : taskList) {
			try {
				int id = task.getHwEsighthostId();
				ESight esight = eSightDao.getESightById(id);

				Map<String, Object> reqMap = new GetSoftwareProgressApi<Map>(esight, new SessionOpenIdProvider(esight, session)).doCall(task.getTaskName(), Map.class);
				Map<String, Object> dataMap = (Map<String, Object>) reqMap.get("data");

				String syncStatus = SyncStatus.getStatus(task.getSyncStatus(), (String) dataMap.get("taskStatus"), (String) dataMap.get("taskResult"), String.valueOf(dataMap.get("taskCode")));
				task.setSyncStatus(syncStatus);
				task.setTaskProgress((int) dataMap.get("taskProgress"));
				task.setTaskResult((String) dataMap.get("taskResult"));
				if (dataMap.get("taskCode") != null && !String.valueOf(dataMap.get("taskCode")).isEmpty()) {
					task.setTaskCode(ErrorPrefix.SOURCEWARE_ERROR_PREFIX + String.valueOf(dataMap.get("taskCode")));
				}
				task.setErrorDetail((String) dataMap.get("errorDetail"));
				taskDao.saveTaskStatus(task);
			} catch (Exception e) {
				LOGGER.error(e.getMessage());
			}
		}
	}

	public void setTaskDao(TaskDao taskDao) {
		this.taskDao = taskDao;
	}

}
