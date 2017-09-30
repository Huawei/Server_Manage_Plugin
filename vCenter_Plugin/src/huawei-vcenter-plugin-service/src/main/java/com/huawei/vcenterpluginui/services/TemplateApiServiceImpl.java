package com.huawei.vcenterpluginui.services;

import java.io.IOException;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.servlet.http.HttpSession;

import com.huawei.esight.api.rest.template.DeleteTemplateApi;
import com.huawei.esight.api.rest.template.GetTemplateDetailApi;
import com.huawei.esight.api.rest.template.GetTemplateListApi;
import com.huawei.esight.api.rest.template.PostDeployTaskApi;
import com.huawei.esight.api.rest.template.PostDeployTaskDetailApi;
import com.huawei.esight.api.rest.template.PostTemplateApi;
import com.huawei.esight.bean.Esight;
import com.huawei.esight.exception.EsightException;
import com.huawei.esight.utils.JsonUtil;
import com.huawei.vcenterpluginui.constant.ErrorPrefix;
import com.huawei.vcenterpluginui.constant.SyncStatus;
import com.huawei.vcenterpluginui.constant.TaskType;
import com.huawei.vcenterpluginui.dao.TaskDao;
import com.huawei.vcenterpluginui.dao.TaskResourceDao;
import com.huawei.vcenterpluginui.entity.ESight;
import com.huawei.vcenterpluginui.entity.Task;
import com.huawei.vcenterpluginui.entity.TaskResource;
import com.huawei.vcenterpluginui.provider.SessionOpenIdProvider;

public class TemplateApiServiceImpl extends ESightOpenApiService implements TemplateApiService {

	private TaskDao taskDao;
	
	private TaskResourceDao taskResourceDao;

	@Override
	public List<Map<String, Object>> templatePost(String data, HttpSession session) throws IOException {
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
					Map<String, Object> dataMap = new PostTemplateApi<Map>(esight, new SessionOpenIdProvider(esight, session)).doCall(condition, Map.class);
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
		return dataMapList;
	}

	@Override
	public String list(String ip, HttpSession session, String templateType, String pageNo, String pageSize) throws SQLException {
		ESight esight = getESightByIp(ip);
		String response = new GetTemplateListApi<String>(esight, new SessionOpenIdProvider(esight, session)).doCall(templateType, pageNo, pageSize, String.class);
		return response;
	}

	@Override
	public String delete(String ip, String templateName, HttpSession session) throws SQLException {
		ESight esight = getESightByIp(ip);
		String response = new DeleteTemplateApi<String>(esight, new SessionOpenIdProvider(esight, session)).doCall(templateName, String.class);
		return response;
	}

	@Override
	public Map<String, Object> templateTaskList(String esightIp, String taskName, String taskStatus, int pageNo, int pageSize, String order, String orderDesc, HttpSession session) throws SQLException {
		Map<String, Object> taskParam = new HashMap<String, Object>();
		taskParam.put("incompleted", true);
		taskParam.put("taskType", TaskType.TASK_TYPE_DEPLOY.name());

		List<Task> taskList = taskDao.getIncompletedTaskList(taskParam);

		// 同步未完成任务状态
		for (Task task : taskList) {
			try {
				int id = task.getHwEsighthostId();
				Esight esight = eSightDao.getESightById(id);
				Map<String, Object> reqMap = new PostDeployTaskDetailApi<Map>(esight, new SessionOpenIdProvider(esight, session)).doCall(task.getTaskName(), Map.class);

				if ((int) reqMap.get("code") == RESULT_SUCCESS_CODE) {
					Map<String, Object> dataMap = (Map<String, Object>) reqMap.get("data");
					String syncStatus = SyncStatus.getStatus(task.getSyncStatus(), dataMap.get("taskStatus").toString(), dataMap.get("taskResult").toString(), String.valueOf(dataMap.get("taskCode")));
					task.setSyncStatus(syncStatus);
					task.setTaskCode(String.valueOf(dataMap.get("taskCode")));
					task.setTaskResult((String) dataMap.get("taskResult"));
					task.setErrorDetail((String) dataMap.get("errorDetail"));
					// task.setTaskProgress((int) dataMap.get("taskProgress"));
					List<Map<String, Object>> deviceDetails = (List<Map<String, Object>>) dataMap.get("deviceDetails");
					if (deviceDetails != null && !deviceDetails.isEmpty()) {
						int totalProgress = 0;
						Boolean dnNotExist = false;
						TaskResource taskFailResource = null;
						List<String> dnList = new ArrayList<String>();
						for (Map<String, Object> deviceDetail : deviceDetails) {
							TaskResource taskResource = new TaskResource();
							if (deviceDetail.get("deviceProgress") != null && !String.valueOf(deviceDetail.get("deviceProgress")).isEmpty()) {
								totalProgress += (int) deviceDetail.get("deviceProgress");
								taskResource.setDeviceProgress((int)deviceDetail.get("deviceProgress"));
								taskResource.setDeviceResult((String)deviceDetail.get("deviceResult"));
								taskResource.setDn((String)deviceDetail.get("dn"));
								taskResource.setErrorDetail((String)deviceDetail.get("errorDetail"));
								taskResource.setHwESightTaskID(task.getTaskName());
								taskResource.setTaskType(TaskType.TASK_TYPE_DEPLOY.name());
								taskResource.setIpAddress(esight.getHostIp());
								if (deviceDetail.get("errorCode") != null && !"".equals(String.valueOf(deviceDetail.get("errorCode")))) {
									taskResource.setErrorCode(ErrorPrefix.TEMPLATE_ERROR_PREFIX + String.valueOf(deviceDetail.get("errorCode")));
								} else {
									taskResource.setErrorCode((String) deviceDetail.get("errorCode"));
								}
								
								String status = SyncStatus.getStatus(task.getSyncStatus(), null, (String)deviceDetail.get("deviceResult"), String.valueOf(deviceDetail.get("errorCode")));
								taskResource.setSyncStatus(status);
								if (taskResource.getDn() != null) {
									dnList.add(taskResource.getDn());
									if (taskResourceDao.updateTaskResource(taskResource) == 0) {
										taskResourceDao.saveTaskResource(taskResource);
									}
								}else{
									dnNotExist = true;
									taskFailResource = taskResource;
								}
							}
						}
						
						if (SyncStatus.STATUS_HW_FAILED.equals("syncStatus") && dnNotExist && taskFailResource != null) {
							taskResourceDao.updateTaskResourceNotExistDN(dnList, taskFailResource);
						}
						
						task.setTaskProgress(totalProgress / deviceDetails.size());
					}
					taskDao.saveTaskStatus(task);
				}
			} catch (RuntimeException e) {
			    throw e;
			}  catch (Exception e) {
				LOGGER.error(e.getMessage());
			}
		}
		// 同步时会更新状态，需要重新拿
		taskParam.put("pageNo", pageNo);
		taskParam.put("pageSize", pageSize);
		if (taskName != null && !taskName.isEmpty()) {
			taskParam.put("softwareSourceName", taskName);
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

		Map<String, Object> dataMap = new HashMap<String, Object>();

		for(Task task : data){
			task.setDeviceDetails(taskResourceDao.getTaskResource(task.getTaskName()));
		}
		
		dataMap.put("data", data);
		dataMap.put("count", count);

		return dataMap;
	}

	@Override
	public String postTemplateTask(String data, HttpSession session) throws IOException, SQLException {
		Map<String, Object> reqMap = JsonUtil.readAsMap(data);
		String ip = (String)reqMap.get("esight");
		ESight esight = getESightByIp(ip);
		String responseBody = new PostDeployTaskApi<String>(esight, new SessionOpenIdProvider(esight, session)).doCall(((Map) reqMap.get("param")).get("templates").toString(),
				((Map) reqMap.get("param")).get("dn").toString(), String.class);

		// 保存任务
		setTemplateTask(esight.getId(), ((Map) reqMap.get("param")).get("deployTaskName").toString(), ((Map) reqMap.get("param")).get("templates").toString(),
				((Map) reqMap.get("param")).get("dn").toString(), responseBody);
		return responseBody;
	}

	private void setTemplateTask(int esightId, String taskName, String templates, String dn, String responseBody) throws SQLException, IOException {
		Map<String, Object> reqMap = JsonUtil.readAsMap(responseBody);
		if ((int) reqMap.get("code") == RESULT_SUCCESS_CODE) {
			Task task = new Task();
			task.setHwEsighthostId(esightId);
			task.setSoftwareSourceName(taskName);
			task.setTaskProgress(0);
			task.setSyncStatus(SyncStatus.STATUS_CREATED);
			task.setTaskType(TaskType.TASK_TYPE_DEPLOY.name());
			task.setTaskName(((Map) reqMap.get("data")).get("taskName").toString());
			task.setTemplates(templates);

			taskDao.saveTask(task);
		}
	}

	@Override
	public int deleteTemplateTask(int taskId) throws SQLException {
		return taskDao.deleteTaskById(taskId);
	}

	@Override
	public String getDetail(String ip, String templateName, HttpSession session) throws SQLException {
		ESight esight = getESightByIp(ip);
		String response = new GetTemplateDetailApi<String>(esight, new SessionOpenIdProvider(esight, session)).doCall(templateName, String.class);
		return response;
	}
	
	public void setTaskDao(TaskDao taskDao) {
		this.taskDao = taskDao;
	}

	public void setTaskResourceDao(TaskResourceDao taskResourceDao) {
		this.taskResourceDao = taskResourceDao;
	}
}
