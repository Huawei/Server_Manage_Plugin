package com.huawei.vcenterpluginui.mvc;

import com.huawei.vcenterpluginui.constant.ErrorPrefix;
import com.huawei.vcenterpluginui.entity.ResponseBodyBean;
import com.huawei.vcenterpluginui.entity.Task;
import com.huawei.vcenterpluginui.services.FirmwareApiService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.web.bind.annotation.*;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpSession;
import java.io.IOException;
import java.sql.SQLException;
import java.util.List;
import java.util.Map;

/**
 * 固件升级 控制层
 *
 * @author licong
 */
@RequestMapping(value = "/services/server/firmware/basepackages")
public class FirmwareController extends BaseController {

    private FirmwareApiService firmwareApiService;

    @Autowired
    public FirmwareController(@Qualifier("firmwareApiService") FirmwareApiService firmwareApiService) {
        this.firmwareApiService = firmwareApiService;
    }

    // Empty controller to avoid compiler warnings in huawei-vcenter-plugin-ui's
    // bundle-context.xml
    // where the bean is declared
    public FirmwareController() {
        firmwareApiService = null;
    }

    /**
     * upload firmware 上传的同时本地会保存该任务以便后面查询进度。
     *
     * @param data    {
     *                "esights": [
     *                "127.0.0.1"
     *                ],
     *                "data": {
     *                "basepackageName": "basepackage1",
     *                "basepackageDescription": "This is a basepackage.",
     *                "basepackageType": "Firmware",
     *                "fileList": "iBMC.zip,iBMC.zip.asc",
     *                "sftpserverIP": "188.10.18.188",
     *                "username": "itSftpUser"
     *                }
     *                }
     * @param session
     * @return
     * @throws Exception
     */
    @RequestMapping(value = "/upload", method = RequestMethod.POST)
    @ResponseBody
    public ResponseBodyBean upload(HttpServletRequest httpServletRequest, @RequestBody String data, HttpSession session) throws IOException, SQLException {
        List<Map<String, Object>> dataMapList = firmwareApiService.upload(data, session);

        return listData(dataMapList,ErrorPrefix.FIRMWARE_ERROR_PREFIX);
    }

    /**
     * 获取上传任务进度列表（上传未完成任务）
     * 该查询会将循环更新本地数据库保存的任务进度
     *
     * @param session
     * @return
     * @throws Exception
     */
    @RequestMapping(value = "/progress/list", method = RequestMethod.GET)
    @ResponseBody
    public ResponseBodyBean progressList(HttpServletRequest httpServletRequest, HttpSession session) throws SQLException {

        List<Task> dataList = firmwareApiService.progressList(session);

        return success(dataList);
    }

    /**
     * firmware list 上传成功的固件列表
     *
     * @param esightIp
     * @param session
     * @return
     * @throws Exception
     */
    @RequestMapping(value = "/list", method = RequestMethod.GET)
    @ResponseBody
    public ResponseBodyBean list(HttpServletRequest httpServletRequest, @RequestParam String esightIp,@RequestParam(required = false) int pageNo,
			  @RequestParam(required = false) int pageSize, HttpSession session) throws IOException, SQLException {

		String dataList = firmwareApiService.list(esightIp, pageNo, pageSize, session);

        return getResultByData(dataList,ErrorPrefix.FIRMWARE_ERROR_PREFIX);
    }

    /**
     * firmware detail
     *
     * @param esightIp
     * @param basepackageName
     * @param session
     * @return
     * @throws Exception
     */
    @RequestMapping(value = "/detail", method = RequestMethod.GET)
    @ResponseBody
    public ResponseBodyBean detail(HttpServletRequest httpServletRequest, @RequestParam String esightIp, @RequestParam String basepackageName, HttpSession session) throws IOException, SQLException {

        String dataMapList = firmwareApiService.detail(esightIp, basepackageName, session);

        return getResultByData(dataMapList,ErrorPrefix.FIRMWARE_ERROR_PREFIX);
    }
    
    /**
     * firmware device detail
     *
     * @param esightIp
     * @param taskName
     * @param dn
     * @param session
     * @return
     * @throws Exception
     */
    @RequestMapping(value = "/device/detail", method = RequestMethod.GET)
    @ResponseBody
    public ResponseBodyBean deviceDetail(HttpServletRequest httpServletRequest, @RequestParam String esightIp, @RequestParam String taskName, @RequestParam String dn, HttpSession session) throws IOException, SQLException {

        String dataMapList = firmwareApiService.deviceDetail(esightIp, taskName, dn, session);

        return getResultByData(dataMapList,ErrorPrefix.FIRMWARE_ERROR_PREFIX);
    }

    /**
     * 删除固件上传失败任务
     *
     * @return
     * @throws Exception
     */
    @RequestMapping(value = "", method = RequestMethod.DELETE)
    @ResponseBody
    public ResponseBodyBean delete(HttpServletRequest httpServletRequest) throws SQLException {

        return success(firmwareApiService.deleteUploadFailed());
    }
    
    /**
     * 删除固件
     *
     * @return
     * @throws Exception
     */
    @RequestMapping(value = "/{basepackageName}", method = RequestMethod.DELETE)
    @ResponseBody
    public ResponseBodyBean deleteBasepackage(HttpServletRequest httpServletRequest, @PathVariable String basepackageName,@RequestParam String esightIp, HttpSession session) throws IOException,SQLException {

        return getResultByData(firmwareApiService.deleteBasepackage(esightIp, basepackageName, session),ErrorPrefix.FIRMWARE_ERROR_PREFIX);
    }

    /**
     * 固件升级添加部署任务
     *
     * @param data    {
     *                "esight": "127.0.0.1",
     *                "param": {
     *                "basepackageName": "basepackage1",
     *                "firmwareList": "CAN,SSD",
     *                "dn": "NE=123;NE=1234"
     *                }
     *                }
     * @param session
     * @return
     * @throws Exception
     */
    @RequestMapping(value = "/upgrade/task", method = RequestMethod.POST)
    @ResponseBody
    public ResponseBodyBean upgradeTaskPost(HttpServletRequest httpServletRequest, @RequestBody String data, HttpSession session) throws IOException, SQLException {

        return getResultByData(firmwareApiService.postUpgradeTask(data, session),ErrorPrefix.FIRMWARE_ERROR_PREFIX);
    }

    /**
     * 固件升级任务列表
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
    @RequestMapping(value = "/upgrade/task/list", method = RequestMethod.GET)
    @ResponseBody
    public ResponseBodyBean upgradeTasklist(HttpServletRequest httpServletRequest,
                                            @RequestParam String esightIp,
                                            @RequestParam String taskName,
                                            @RequestParam String taskStatus,
                                            @RequestParam int pageNo,
                                            @RequestParam int pageSize,
                                            @RequestParam String order,
                                            @RequestParam String orderDesc,
                                            HttpSession session) throws IOException, SQLException {

        return success(firmwareApiService.upgradeTaskList(esightIp, taskName, taskStatus, pageNo, pageSize, order, orderDesc, session));
    }

    /**
     * 删除固件升级任务
     *
     * @return
     * @throws Exception
     */
    @RequestMapping(value = "/task/{taskId}", method = RequestMethod.DELETE)
    @ResponseBody
    public ResponseBodyBean deleteTask(HttpServletRequest httpServletRequest, @PathVariable int taskId) throws SQLException {

        return firmwareApiService.deleteUpgradeTask(taskId) > 0 ? success() : failure();
    }
}
