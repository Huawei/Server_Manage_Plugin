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
import com.huawei.vcenterpluginui.entity.Task;
import com.huawei.vcenterpluginui.services.SoftwareApiService;

/**
 * 软件源管理 控制层
 * 
 * @author licong
 *
 */
@RequestMapping(value = "/services/software")
public class SoftwareResourceController extends BaseController {

	private SoftwareApiService softwareApiService;

	@Autowired
	public SoftwareResourceController(@Qualifier("softwareApiService") SoftwareApiService softwareApiService) {
		this.softwareApiService = softwareApiService;
	}

	// Empty controller to avoid compiler warnings in huawei-vcenter-plugin-ui's
	// bundle-context.xml
	// where the bean is declared
	public SoftwareResourceController() {
		softwareApiService = null;
	}

	/**
	 * 软件源列表（上传成功）
	 * 
	 * @param ip
	 * @param session
	 * @return
	 */
	@RequestMapping(value = "/list", method = RequestMethod.GET)
	@ResponseBody
	public ResponseBodyBean softwareResourceList(HttpServletRequest request, @RequestParam String ip,@RequestParam(required = false) int pageNo,
			  @RequestParam(required = false) int pageSize, HttpSession session) throws IOException, SQLException {

		return getResultByData(softwareApiService.list(ip, pageNo, pageSize, session),ErrorPrefix.SOURCEWARE_ERROR_PREFIX);
	}

	/**
	 * 上传软件源
	 * 
	 * @param data
	 *            { // "esights": [ // "192.168.1.1", "192.168.1.4",
	 *            "192.168.1.4" // ], //这里是一个数组 // "data": { // "softwareName":
	 *            "OS_Software1", // "softwareDescription":
	 *            "..This is a OS template.", // "softwareType": "Windows", //
	 *            "softwareVersion": "Windows Server 2008 R2 x64", //
	 *            "softwareEdition": null, // "softwareLanguage": "Chinese", //
	 *            "sourceName":
	 *            "7601.17514.101119 - 1850_x64fre_server_eval_en - us - GRMSXEVAL_EN_DVD.iso"
	 *            , // "sftpserverIP": "188.10.18.188", // "username":
	 *            "itSftpUser",  } // }
	 * @param session
	 * @return
	 */
	@RequestMapping(value = "/upload", method = RequestMethod.POST)
	@ResponseBody
	public ResponseBodyBean upload(HttpServletRequest httpServletRequest, @RequestBody String data, HttpSession session) throws IOException, SQLException {

		List<Map<String, Object>> dataMapList = softwareApiService.upload(data, session);

		return listData(dataMapList,ErrorPrefix.SOURCEWARE_ERROR_PREFIX);
	}

	/**
	 * 上传软件源任务进度列表
	 * 
	 * @param session
	 * @return
	 */
	@RequestMapping(value = "/progress/list", method = RequestMethod.GET)
	@ResponseBody
	public ResponseBodyBean progressList(HttpServletRequest httpServletRequest, HttpSession session) throws SQLException {

		List<Task> dataList = softwareApiService.progressList(session);

		return success(dataList);
	}

	/**
	 * 删除失败任务（上传软件源）
	 * 
	 * @return
	 */
	@RequestMapping(value = "", method = RequestMethod.DELETE)
	@ResponseBody
	public ResponseBodyBean deleteAllFailedTask(HttpServletRequest httpServletRequest) throws SQLException {

		return success(softwareApiService.deleteUploadFailed());
	}

	/**
	 * 删除软件源
	 *
	 * @return
	 * @throws Exception
	 */
	@RequestMapping(value = "/{softwareName}", method = RequestMethod.DELETE)
	@ResponseBody
	public ResponseBodyBean deleteTask(HttpServletRequest httpServletRequest, @PathVariable String softwareName, @RequestParam String ip, HttpSession session) throws IOException, SQLException {

		return getResultByData(softwareApiService.deleteSoftResource(softwareName, ip, session),ErrorPrefix.SOURCEWARE_ERROR_PREFIX);
	}
}
