package com.huawei.vcenterpluginui.services;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;
import org.springframework.context.ApplicationListener;
import org.springframework.context.event.ContextRefreshedEvent;

/**
 * Created by hyuan on 2017/6/8.
 */
public class InstantiationBeanServiceImpl implements
        ApplicationListener<ContextRefreshedEvent>, InstantiationBeanService {

    private static final Log LOGGER = LogFactory.getLog(InstantiationBeanServiceImpl.class);

    private SystemService systemService;

    @Override
    public void onApplicationEvent(ContextRefreshedEvent event) {
        init();
    }

    public SystemService getSystemService() {
        return systemService;
    }

    public void setSystemService(SystemService systemService) {
        this.systemService = systemService;
    }

    @Override
    public void init() {
        try {
            systemService.initDB();
        } catch (Exception e) {
            LOGGER.warn(e);
        }
    }
}
