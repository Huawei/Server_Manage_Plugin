package com.huawei.esight.service.bean;

import com.integrien.alive.common.adapter3.MetricData;
import com.integrien.alive.common.adapter3.ResourceKey;

import java.util.List;
import java.util.Map;

/**
 * 树形节点类要实现的接口，如server device, CPU, Fan, Disk, Memory等.
 * @author harbor
 *
 */
public interface TreeNodeResource {
    
    /**
     * Object 转换为 resource, 并返回 key.
     * @param id 唯一标识
     * @param adapterKind 插件类型
     * @param metricsByResource key to data map
     * @return resource key
     */
    public ResourceKey convert2Resource(String id,String adapterKind, 
            Map<ResourceKey, List<MetricData>> metricsByResource);
    
}
