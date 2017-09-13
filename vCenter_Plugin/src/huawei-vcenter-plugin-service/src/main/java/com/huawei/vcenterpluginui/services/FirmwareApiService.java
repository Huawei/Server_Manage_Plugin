package com.huawei.vcenterpluginui.services;

import com.huawei.vcenterpluginui.entity.Task;

import javax.servlet.http.HttpSession;
import java.io.IOException;
import java.sql.SQLException;
import java.util.List;
import java.util.Map;

/**
 * It must be declared as osgi:service with the same name in
 * main/resources/META-INF/spring/bundle-context-osgi.xml
 */
public interface FirmwareApiService {
    //========================固件上传===============================

    /**
     * 上传固件
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
     */
    List<Map<String, Object>> upload(String data, HttpSession session) throws IOException, SQLException;

    /**
     * 上传固件任务进度列表
     *
     * @param session
     * @return
     */
    List<Task> progressList(HttpSession session) throws SQLException;

    /**
     * 固件列表（上传成功）
     *
     * @param esightIp
     * @param session
     * @return
     */
    String list(String esightIp,int pageNo, int pageSize, HttpSession session) throws SQLException;

    /**
     * 固件详情
     *
     * @param esightIp
     * @param basepackageName
     * @param session
     * @return
     */
    String detail(String esightIp, String basepackageName, HttpSession session) throws SQLException;
    
    /**
     * 发布任务设备详情
     *
     * @param esightIp
     * @param taskName
     * @param dn
     * @param session
     * @return
     */
    String deviceDetail(String esightIp, String taskName, String dn, HttpSession session) throws SQLException;

    /**
     * 删除失败任务（上传升级包）
     *
     * @return
     */
    int deleteUploadFailed() throws SQLException;
    
    /**
     * 删除固件
     *
     * @return
     */
    String deleteBasepackage(String esightIp, String basepackageName, HttpSession session) throws SQLException;

    //========================固件升级===============================

    /**
     * 固件升级任务列表
     *
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
    Map<String, Object> upgradeTaskList(String esightIp, String taskName, String taskStatus, int pageNo, int pageSize, String order, String orderDesc, HttpSession session) throws SQLException, IOException;

    /**
     * 固件升级任务上传
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
     */
    String postUpgradeTask(String data, HttpSession session) throws IOException, SQLException;

    /**
     * 删除固件升级任务
     *
     * @param taskId
     * @return
     */
    int deleteUpgradeTask(int taskId) throws SQLException;
}
