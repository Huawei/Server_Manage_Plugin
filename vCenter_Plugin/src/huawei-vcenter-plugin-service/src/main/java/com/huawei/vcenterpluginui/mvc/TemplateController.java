package com.huawei.vcenterpluginui.mvc;

import java.io.IOException;
import java.sql.SQLException;
import java.util.List;
import java.util.Map;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpSession;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.ResponseBody;

import com.huawei.vcenterpluginui.constant.ErrorPrefix;
import com.huawei.vcenterpluginui.entity.ResponseBodyBean;
import com.huawei.vcenterpluginui.services.TemplateApiService;

/**
 * 模版管理 控制层
 * @author licong
 *
 */
@RequestMapping(value = "/services/template")
public class TemplateController extends BaseController {

	private TemplateApiService templateApiService;

	@Autowired
	public TemplateController(@Qualifier("templateApiService") TemplateApiService templateApiService) {
		this.templateApiService = templateApiService;
	}
	
	// Empty controller to avoid compiler warnings in huawei-vcenter-plugin-ui's
	// bundle-context.xml
	// where the bean is declared
	public TemplateController() {
		templateApiService = null;
	}
	
	/**
	 * get template list
	 * 
	 * @param ip   esightIp
	 * @param session
	 * @return
	 * @throws Exception
	 */
	@RequestMapping(value = "/list", method = RequestMethod.GET)
	@ResponseBody
	public ResponseBodyBean templateList(HttpServletRequest request, @RequestParam String ip,@RequestParam(required = false) String templateType,@RequestParam(required = false) String pageNo,
			  @RequestParam(required = false) String pageSize, HttpSession session) throws IOException, SQLException {
		return getResultByData(templateApiService.list(ip, session, templateType, pageNo, pageSize),ErrorPrefix.TEMPLATE_ERROR_PREFIX);
	}
	
	/**
	 * get template detail
	 * 
	 * @param templateName 
	 * @param ip   esightIp
	 * @param session
	 * @return
	 * @throws Exception
	 */
	@RequestMapping(value = "/{templateName}", method = RequestMethod.GET)
	@ResponseBody
	public ResponseBodyBean getTemplateDetail(HttpServletRequest request, @PathVariable String templateName, @RequestParam String ip, HttpSession session) throws IOException, SQLException {
		return getResultByData(templateApiService.getDetail(ip, templateName, session),ErrorPrefix.TEMPLATE_ERROR_PREFIX);
	}
	
	/**
	 * delete template
	 * 
	 * @param templateName 
	 * @param ip   esightIp
	 * @param session
	 * @return
	 * @throws Exception
	 */
	@RequestMapping(value = "/{templateName}", method = RequestMethod.DELETE)
	@ResponseBody
	public ResponseBodyBean deleteTemplate(HttpServletRequest request, @PathVariable String templateName, @RequestParam String ip, HttpSession session) throws IOException, SQLException {
		return getResultByData(templateApiService.delete(ip, templateName, session),ErrorPrefix.TEMPLATE_ERROR_PREFIX);
	}
	
	/**
	 * post template
	 * 
	 * @param data  提交模版数据{
								"esights": [
								"127.0.0.1",
								"192.168.1.1"
								],
								"data": {
									"templateName": "模板名称",
									"templateType": "模版类型",
									"templateDesc": "this is a template",
									"templateProp": {
									"模版属性": "模版属性值"
									}
								}
							  }
	 * @param session
	 * @return
	 * @throws Exception
	 */
	@RequestMapping(value = "", method = RequestMethod.POST)
	@ResponseBody
	public ResponseBodyBean postTemplate(HttpServletRequest request, @RequestBody String data, HttpSession session) throws IOException {
		List<Map<String, Object>> dataMapList = templateApiService.templatePost(data, session);

		return listData(dataMapList,ErrorPrefix.TEMPLATE_ERROR_PREFIX);
	}
	
	/**
     * 模版添加部署任务
     *
     * @param data    // var postParam = {
			          //     "esight": "127.0.0.1",
			          //     "param": {
			          //         "deployTaskName": "test1",
			          //         "dn": "NE=123;NE=1234",
			          //         "templates": "template1,poweron1"
			          //     }
			          // };
     * @param session
     * @return
     * @throws Exception
     */
    @RequestMapping(value = "/task", method = RequestMethod.POST)
    @ResponseBody
    public ResponseBodyBean templateTaskPost(HttpServletRequest httpServletRequest, @RequestBody String data, HttpSession session) throws IOException, SQLException {

        return getResultByData(templateApiService.postTemplateTask(data, session),ErrorPrefix.TEMPLATE_ERROR_PREFIX);
    }

    /**
     * 模版任务列表
     *
     * @param httpServletRequest
     * @param esightIp
     * @param taskName
     * @param taskStatus
     * @param pageNo
     * @param pageSize
     * @param order
     * @param orderDesc
     * @param session
     * @return
     * @throws Exception
     */
    @RequestMapping(value = "/task/list", method = RequestMethod.GET)
    @ResponseBody
    public ResponseBodyBean templateTasklist(HttpServletRequest httpServletRequest,
											 @RequestParam String esightIp,
											 @RequestParam String taskName,
											 @RequestParam String taskStatus,
											 @RequestParam int pageNo,
                                             @RequestParam int pageSize,
											 @RequestParam String order,
											 @RequestParam String orderDesc, HttpSession session) throws SQLException {
        return success(templateApiService.templateTaskList(esightIp, taskName, taskStatus, pageNo, pageSize, order, orderDesc, session));
    }

    /**
     * 删除模版任务
     *
     * @return
     * @throws Exception
     */
    @RequestMapping(value = "task/{taskId}", method = RequestMethod.DELETE)
    @ResponseBody
    public ResponseBodyBean deleteTask(HttpServletRequest httpServletRequest, @PathVariable int taskId) throws SQLException {

		return templateApiService.deleteTemplateTask(taskId) > 0 ? success() : failure();
    }
	
}
