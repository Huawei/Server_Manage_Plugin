
package com.huawei.esight.service.bean;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.huawei.esight.service.ESightResponseDataObject;

/**
 * 获取服务器详细信息结果的映射类.
 * @author harbor
 *
 */
@JsonIgnoreProperties(ignoreUnknown = true)
public class ResponseServerDeviceDetailBean 
    extends ESightResponseDataObject<ServerDeviceDetailBean> {
    
}