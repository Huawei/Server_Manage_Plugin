package com.huawei.esight.service;

import com.huawei.esight.service.bean.ServerDeviceBean;
import com.huawei.esight.service.bean.ServerDeviceDetailBean;

import java.util.List;

import org.apache.log4j.Logger;

public interface ESightService {
    
    /**
     * eSight登录.
     * @param username 用户名
     * @param password 密码
     * @return openid
     */
    String login(String host, int port, String username, String password);
    
    /**
     * 服务器类型，范围(rack：机架服务器,blade：刀片服务器,highdensity：高密服务器).
     * @param servertype type of server
     * @return list of ServerDeviceBean
     */
    List<ServerDeviceBean> getServerDeviceList(String servertype);
    
    /**
     * 根据dn (device name)获取服务器详细信息.
     * @param dn device name
     * @return detail of server device 
     */
    ServerDeviceDetailBean getServerDetail(String dn);
    
    /**
     * 注销openid.
     * @param openid 会话ID
     */
    void logout(String openid);
    
    void setLogger(Logger logger);
    
}
