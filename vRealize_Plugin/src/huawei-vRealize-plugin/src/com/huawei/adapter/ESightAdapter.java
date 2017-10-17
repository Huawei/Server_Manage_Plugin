package com.huawei.adapter;
/*
 * Copyright (c) 2014-2015 VMware, Inc. All rights reserved.
 */

import com.huawei.adapter.util.ConvertUtils;
import com.huawei.adapter.util.ESightAdapterUtil;
import com.huawei.esight.exception.EsightException;
import com.huawei.esight.service.ESightService;
import com.huawei.esight.service.ESightServiceImpl;
import com.huawei.esight.service.bean.BoardBean;
import com.huawei.esight.service.bean.CPUBean;
import com.huawei.esight.service.bean.ChildBladeBean;
import com.huawei.esight.service.bean.Constant;
import com.huawei.esight.service.bean.DiskBean;
import com.huawei.esight.service.bean.FanBean;
import com.huawei.esight.service.bean.MemoryBean;
import com.huawei.esight.service.bean.MezzBean;
import com.huawei.esight.service.bean.NetworkCardBean;
import com.huawei.esight.service.bean.PCIEBean;
import com.huawei.esight.service.bean.PSUBean;
import com.huawei.esight.service.bean.RAIDBean;
import com.huawei.esight.service.bean.ServerDeviceBean;
import com.huawei.esight.service.bean.ServerDeviceDetailBean;

import com.integrien.alive.common.adapter3.AdapterBase;
import com.integrien.alive.common.adapter3.DiscoveryParam;
import com.integrien.alive.common.adapter3.DiscoveryResult;
import com.integrien.alive.common.adapter3.IdentifierCredentialProperties;
import com.integrien.alive.common.adapter3.MetricData;
import com.integrien.alive.common.adapter3.MetricKey;
import com.integrien.alive.common.adapter3.Relationships;
import com.integrien.alive.common.adapter3.ResourceKey;
import com.integrien.alive.common.adapter3.ResourceStatus;
import com.integrien.alive.common.adapter3.TestParam;
import com.integrien.alive.common.adapter3.config.ResourceConfig;
import com.integrien.alive.common.adapter3.config.ResourceIdentifierConfig;
import com.integrien.alive.common.adapter3.describe.AdapterDescribe;

import java.util.ArrayList;
import java.util.Collection;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.TimeUnit;

import org.apache.log4j.Logger;

/**
 * Main adapter class for ESightAdapter.
 * 插件的入口类
 */
public class ESightAdapter extends AdapterBase {
    
    //logger instance
    private final Logger logger;
    
    private ESightService service = null;
    
    //ESightAdapterUtil instance
    private ESightAdapterUtil adapterUtil;
    
    // Stores metrics for each resource
    private Map<ResourceKey, List<MetricData>> metricsByResource = new LinkedHashMap<>();
    
    // Stores the relationships for each resource
    private Map<ResourceKey, List<ResourceKey>> relationshipsByResource = new LinkedHashMap<>();
    
    //key map for device name to resource key
    private Map<String, ResourceKey> deviceResourceKeyMap = new LinkedHashMap<>();
    
    private String host = null;
    
    private void setHost(String host){
    	
    	if (this.host == null) {
    		this.host = host;
    		return;
    	}
    	
    	if (this.host.equals(host)) {
    		return;
    	}
    	
    	logger.error("eSight Host IP changed from '" + this.host + "' to '" + host + "'.");
    	this.host = host;
    	metricsByResource.clear();
    	relationshipsByResource.clear();
    	deviceResourceKeyMap.clear();
    }
    
    /**
     * Default constructor.
     */
    public ESightAdapter() {
        this(null, null);
    }
    
    /**
     * 调用eSight REST API采集数据.
     * @param host eSight服务器IP
     * @return ResourceKey列表
     */
    private List<ResourceKey> collectResourceDataFromESight(String host) {
        
        
        List<ResourceKey> allResourceList = new ArrayList<>();
        String[] serverTypes = new String[]{Constant.TREE_SERVER_TYPE_RACK,
                Constant.TREE_SERVER_TYPE_BLADE,
                Constant.TREE_SERVER_TYPE_HIGHDENSITY};
        
        //服务器类型keyList
        List<ResourceKey> serverTypeKeyList = new ArrayList<ResourceKey>();
        
        for (String serverType : serverTypes) {
            
            List<ServerDeviceBean> serverList = service.getServerDeviceList(serverType);
            
            if (serverList.isEmpty()) {
                continue;
            }
            
            //收集 devices key
            List<ResourceKey> serverDeviceKeyList = new ArrayList<ResourceKey>();
            
            //服务器分类
            ResourceKey serverTypeKey = new ResourceKey(serverType, 
                    Constant.KIND_SERVER_TYPES,getAdapterKind());
            ResourceIdentifierConfig ipIdentifier = new ResourceIdentifierConfig(Constant.ATTR_ID, 
            		host + serverType, true);
            serverTypeKey.addIdentifier(ipIdentifier);
            
            allResourceList.add(serverTypeKey);
            serverTypeKeyList.add(serverTypeKey);
            for (ServerDeviceBean deviceBean : serverList) {
                
                ServerDeviceDetailBean device = null;
                
                if (deviceBean.getChildBlades().isEmpty() == false) {
                    device = new ServerDeviceDetailBean(deviceBean);         
                } else {
                    device = service.getServerDetail(deviceBean.getDn());
                    
                    if (device == null) {
                        logger.error("Failed to get detail of device with dn = " + deviceBean.getDn());
                        continue;
                    }
                    
                    device.setVersion(deviceBean.getVersion());
                    device.setLocation(deviceBean.getLocation());
                    device.setManufacturer(deviceBean.getManufacturer());
                }
                
                ResourceKey deviceResourceKey = device.convert2Resource(deviceBean.getDn(), 
                        getAdapterKind(), metricsByResource);
                //dn --> device key map 
                deviceResourceKeyMap.put(deviceBean.getDn(), deviceResourceKey);
                allResourceList.add(deviceResourceKey);
                List<ResourceKey> deviceChildKeys = new ArrayList<>();
                List<ResourceKey> childBladeDeviceKeyList = new ArrayList<ResourceKey>();
                
                //child Blades
                List<ChildBladeBean> childBladeBeans = deviceBean.getChildBlades();
                for (ChildBladeBean bean : childBladeBeans) {
                    childBladeDeviceKeyList.add(deviceResourceKeyMap.get(bean.getDn()));
                }
                
                //处理childBlade显示重复的问题
                if (deviceBean.isChildBlade() == false) {
                    serverDeviceKeyList.add(deviceResourceKey);
                }
                
                //board
                List<BoardBean> boardBeans = device.getBoard();
                if (boardBeans.isEmpty() == false) {
                    List<ResourceKey> boardResourceKey = new ArrayList<ResourceKey>();
                    for (BoardBean board : boardBeans) {
                        
                        if (ConvertUtils.isOffline(board.getPresentState())) {
                            continue;
                        }
                        
                        ResourceKey boardKey = board.convert2Resource(host + deviceBean.getDn(), 
                                getAdapterKind(), metricsByResource);
                        boardResourceKey.add(boardKey);
                        allResourceList.add(boardKey);
                    }
                    
                    //board Group
                    ResourceKey boardGroup = device.createGroupKey(Constant.TREE_BOARD_GROUP, 
                            Constant.KIND_BOARD_GROUP,
                            boardResourceKey, relationshipsByResource, getAdapterKind());
                    deviceChildKeys.add(boardGroup);
                }
                
                //CPU
                List<CPUBean> cpuBeans = device.getCPU();
                if (cpuBeans.isEmpty() == false) {
                    List<ResourceKey> cpuResourceKey = new ArrayList<ResourceKey>();
                    for (CPUBean cpu : cpuBeans) {
                        
                        if (ConvertUtils.isOffline(cpu.getPresentState())) {
                            continue;
                        }
                        
                        ResourceKey key = cpu.convert2Resource(host + deviceBean.getDn(), 
                                getAdapterKind(), metricsByResource);
                        cpuResourceKey.add(key);
                        allResourceList.add(key);
                    }
                    //CPU Group
                    ResourceKey cpuGroup = device.createGroupKey(Constant.TREE_CPU_GROUP, 
                            Constant.KIND_CPU_GROUP,
                            cpuResourceKey, relationshipsByResource, getAdapterKind()); 
                    deviceChildKeys.add(cpuGroup);
                    
                }
                
                //disk
                List<DiskBean> diskBeans = device.getDisk();
                if (diskBeans.isEmpty() == false) {
                    List<ResourceKey> diskResourceKey = new ArrayList<ResourceKey>();
                    for (DiskBean bean : diskBeans) {
                        if (ConvertUtils.isOffline(bean.getPresentState())) {
                            continue;
                        }
                        ResourceKey key = bean.convert2Resource(host + deviceBean.getDn(), 
                                getAdapterKind(), metricsByResource);
                        diskResourceKey.add(key);
                        allResourceList.add(key);
                    }
                    //disk Group
                    ResourceKey diskGroup = device.createGroupKey(Constant.TREE_DISK_GROUP, 
                            Constant.KIND_DISK_GROUP,
                            diskResourceKey, relationshipsByResource, getAdapterKind());
                    deviceChildKeys.add(diskGroup);
                }
                
                //fan
                List<FanBean> fanBeans = device.getFan();
                if (fanBeans.isEmpty() == false) {
                    List<ResourceKey> fanResourceKey = new ArrayList<ResourceKey>();
                    for (FanBean bean : fanBeans) {
                        if (ConvertUtils.isOffline(bean.getPresentState())) {
                            continue;
                        }
                        ResourceKey key = bean.convert2Resource(host + deviceBean.getDn(),
                        		getAdapterKind(), metricsByResource);
                        fanResourceKey.add(key);
                        allResourceList.add(key);
                    }
                    //fan Group
                    ResourceKey fanGroup = device.createGroupKey(Constant.TREE_FAN_GROUP,
                            Constant.KIND_FAN_GROUP,
                            fanResourceKey, relationshipsByResource, getAdapterKind()); 
                    deviceChildKeys.add(fanGroup);
                }
                
                //memory
                List<MemoryBean> memoryBeans = device.getMemory();
                if (memoryBeans.isEmpty() == false) {
                    List<ResourceKey> memoryResourceKey = new ArrayList<ResourceKey>();
                    for (MemoryBean bean : memoryBeans) {
                        if (ConvertUtils.isOffline(bean.getPresentState())) {
                            continue;
                        }
                        ResourceKey key = bean.convert2Resource(host + deviceBean.getDn(), 
                                getAdapterKind(), metricsByResource);
                        memoryResourceKey.add(key);
                        allResourceList.add(key);
                    }
                    
                    //memory Group
                    ResourceKey memoryGroup = device.createGroupKey(Constant.TREE_MEMORY_GROUP, 
                            Constant.KIND_MEMORY_GROUP,
                            memoryResourceKey, relationshipsByResource, getAdapterKind()); 
                    deviceChildKeys.add(memoryGroup);
                }
                
                //PSU
                List<PSUBean> psuBeans = device.getPSU();
                if (psuBeans.isEmpty() == false) {
                    List<ResourceKey> psuResourceKey = new ArrayList<ResourceKey>();
                    for (PSUBean bean : psuBeans) {
                        if (ConvertUtils.isOffline(bean.getPresentState())) {
                            continue;
                        }
                        ResourceKey key = bean.convert2Resource(host + deviceBean.getDn(), 
                                getAdapterKind(), metricsByResource);
                        psuResourceKey.add(key);
                        allResourceList.add(key);
                    }
                    //PSU Group
                    ResourceKey psuGroup = device.createGroupKey(Constant.TREE_PSU_GROUP, 
                            Constant.KIND_PSU_GROUP,
                            psuResourceKey,relationshipsByResource,getAdapterKind());
                    deviceChildKeys.add(psuGroup);
                }
                
                //device Group
                if (childBladeDeviceKeyList.isEmpty() == false) {
                    ResourceKey devicesGroup = device.createGroupKey(Constant.TREE_DEVICES_GROUP, 
                            Constant.KIND_DEVICES_GROUP,
                            childBladeDeviceKeyList, relationshipsByResource, getAdapterKind());
                    deviceChildKeys.add(devicesGroup);
                }
                
                //PCIE
                List<PCIEBean> pcieBeans = device.getPCIE();
                if (pcieBeans.isEmpty() == false) {
                    List<ResourceKey> pcieResourceKey = new ArrayList<ResourceKey>();
                    for (PCIEBean bean : pcieBeans) {
                        ResourceKey key = bean.convert2Resource(host + deviceBean.getDn(), 
                                getAdapterKind(), metricsByResource);
                        pcieResourceKey.add(key);
                        allResourceList.add(key);
                    }
                    //PCIE Group
                    ResourceKey pcieGroup = device.createGroupKey(Constant.TREE_PCIE_GROUP, 
                            Constant.KIND_PCIE_GROUP,
                            pcieResourceKey, relationshipsByResource, getAdapterKind()); 
                    deviceChildKeys.add(pcieGroup);
                }
                
                //RAID
                List<RAIDBean> raidBeans = device.getRAID();
                if (raidBeans.isEmpty() == false) {
                    List<ResourceKey> raidResourceKey = new ArrayList<ResourceKey>();
                    for (RAIDBean bean : raidBeans) {
                        ResourceKey key = bean.convert2Resource(host + deviceBean.getDn(), 
                                getAdapterKind(), metricsByResource);
                        raidResourceKey.add(key);
                        allResourceList.add(key);
                    }
                    //RAID Group
                    ResourceKey raidGroup = device.createGroupKey(Constant.TREE_RAID_GROUP, 
                            Constant.KIND_RAID_GROUP,
                            raidResourceKey, relationshipsByResource, getAdapterKind()); 
                    deviceChildKeys.add(raidGroup);
                }
                
                //network card
                List<NetworkCardBean> networkcardBeans = device.getNetworkCard();
                if (networkcardBeans.isEmpty() == false) {
                    List<ResourceKey> netWorkCardResourceKey = new ArrayList<ResourceKey>();
                    for (NetworkCardBean bean : networkcardBeans) {
                        ResourceKey key = bean.convert2Resource(host + deviceBean.getDn(), 
                                getAdapterKind(), metricsByResource);
                        netWorkCardResourceKey.add(key);
                        allResourceList.add(key);
                    }
                    //network card Group
                    ResourceKey networkCardGroup = device.createGroupKey(Constant.TREE_NETWORK_CARD_GROUP, 
                            Constant.KIND_NETWORK_CARD_GROUP,
                            netWorkCardResourceKey, relationshipsByResource, getAdapterKind()); 
                    deviceChildKeys.add(networkCardGroup);
                }
                
              //Mezz
                List<MezzBean> mezzBeans = device.getMezz();
                if (mezzBeans.isEmpty() == false) {
                    List<ResourceKey> mezzResourceKey = new ArrayList<ResourceKey>();
                    for (MezzBean bean : mezzBeans) {
                        if (ConvertUtils.isOffline(bean.getPresentState())) {
                            continue;
                        }
                        ResourceKey key = bean.convert2Resource(host + deviceBean.getDn(), 
                                getAdapterKind(), metricsByResource);
                        mezzResourceKey.add(key);
                        allResourceList.add(key);
                    }
                    //PSU Group
                    ResourceKey mezzGroup = device.createGroupKey(Constant.TREE_MEZZ_GROUP, 
                            Constant.KIND_MEZZ_GROUP,
                            mezzResourceKey,relationshipsByResource,getAdapterKind());
                    deviceChildKeys.add(mezzGroup);
                }
                
                //设置server device和group object(如cpuGroup, diskGroup, fanGroup等)的关联关系
                relationshipsByResource.put(deviceResourceKey, deviceChildKeys);
                
                allResourceList.addAll(deviceChildKeys);
                
            }
            //设置服务器类型(如rack, blade)和server device关联关系
            relationshipsByResource.put(serverTypeKey, serverDeviceKeyList);
        }
        
        ResourceKey serverGroupKey = new ResourceKey(host, Constant.KIND_HOST_INSTANCE,
                getAdapterKind());
        long timestamp = System.currentTimeMillis();
        List<MetricData> metricDataList = new ArrayList<>();
        //设置healthState为正常状态
        metricDataList.add(new MetricData(new MetricKey(false, Constant.ATTR_HEALTHSTATE), 
                timestamp, ConvertUtils.convertHealthState(0)));
        metricsByResource.put(serverGroupKey, metricDataList);
        
        //eSight server IP和serverTypes的对应关系
        relationshipsByResource.put(serverGroupKey, serverTypeKeyList);
        allResourceList.add(serverGroupKey);
        
        return allResourceList;
    }
    
    /**
     * Parameterized constructor.
     *
     * @param instanceName
     *            instance name
     * @param instanceId
     *            instance Id
     */
    public ESightAdapter(String instanceName, Integer instanceId) {
        super(instanceName, instanceId);
        logger = loggerFactory.getLogger(ESightAdapter.class);
        adapterUtil = new ESightAdapterUtil(loggerFactory);
        logger.info("Esight adapter instantce created: instanceName = " + instanceName 
                + ", instanceId = " + instanceId);
        service = new ESightServiceImpl();
        service.setLogger(loggerFactory.getLogger(ESightServiceImpl.class));
    }
    
    /**
     * The responsibility of the method is to provide adapter describe
     * information to the collection framework.
     */
    @Override
    public AdapterDescribe onDescribe() {
        logger.info("Inside onDescribe method of ESightAdapter class");
        return adapterUtil.createAdapterDescribe();
    }
    
    /**
     * This method is called when user wants to discover resources in the target
     * system manually. onConfigure is not called before onDiscover.
     */
    @Override
    public DiscoveryResult onDiscover(DiscoveryParam discParam) {
        
        //  get the adapter instance properties(IdentifierCredentialProperties)
        // and use it for data collection.
        // final IdentifierCredentialProperties prop =
        // new IdentifierCredentialProperties(loggerFactory, adapterInstResource);
        
        logger.info("Inside onDiscover method of ESightAdapter class");
        
        //  call the external datasource and fetch the data.
        
        // Create discovery result object for adapter instance resource
        // DiscoveryResult object holds any new resources
        // to be added in the system.
        DiscoveryResult discoveryResult = new DiscoveryResult(discParam
                .getAdapterInstResource());
        
        //  populate the discoveryResult object and return it.
        
        return discoveryResult;
    }
    
    /**
     * This method is called for each collection cycle. By default, this value
     * is 5 minutes unless user changes it
     */
    @Override
    public void onCollect(ResourceConfig adapterInstResource,
            Collection<ResourceConfig> monitoringResources) {
        
        if (logger.isInfoEnabled()) {
            logger.info("Inside onCollect method of ESightAdapter class");
        }
        
        final IdentifierCredentialProperties prop = 
                new IdentifierCredentialProperties(loggerFactory, adapterInstResource);
        
        String host = prop.getIdentifier(Constant.KEY_SERVER_IP_ADDRESS, "").trim();
        
        setHost(host);
        
        int hostPort = prop.getIntIdentifier(Constant.KEY_ESIGHT_SERVER_PORT, 
                Constant.DEFAULT_ESIGHT_SERVER_PORT);
        String username = prop.getCredential(Constant.KEY_ESIGHT_ACCOUNT);
        String eSightCode = prop.getCredential(Constant.KEY_ESIGHT_CODE);
        
        String openid = null;
        
        try {
            
            openid = service.login(host, hostPort, username, eSightCode);
        } catch (EsightException e) {
        	logger.error(e.getMessage()+ ": eSight server (" + host + ") authentication failed.", e);
        }
        if (openid == null || openid.isEmpty()) {
            
            ResourceKey resourceKey = new ResourceKey(host, Constant.KIND_HOST_INSTANCE, 
                    getAdapterKind());
            long timestamp = System.currentTimeMillis();
            
            List<MetricData> metricDataList = new ArrayList<>();
            //设置eSight服务器的状态为离线状态
            metricDataList.add(new MetricData(new MetricKey(false, Constant.ATTR_HEALTHSTATE), 
                    timestamp, ConvertUtils.convertHealthState(-1)));
            metricsByResource.put(resourceKey, metricDataList);
            DiscoveryResult discoveryResult = collectResult.getDiscoveryResult(true);
            if (isNewResource(resourceKey)) {
                discoveryResult.addResource(new ResourceConfig(resourceKey));
            }
            
            // Check if resource is part of monitored set, otherwise continue
            ResourceConfig resourceConfig = getMonitoringResource(resourceKey);
            if (resourceConfig == null) {
                return;
            }
            
            // Add metrics
            addMetricData(resourceConfig, metricsByResource.get(resourceKey));
            
            // Add relationships
            Relationships rel = new Relationships();
            rel.setRelationships(resourceKey, relationshipsByResource.get(resourceKey),
                    Collections.singleton(getAdapterKind()));
            discoveryResult.addRelationships(rel);
            
            return;
        }
        
        // call the external datasource and fetch the data.
        
        // Creates automatically the discovery result object when necessary.
        // For auto-discovery purposes get the discovery result and add new resources or
        // resource kinds to it.
        // DiscoveryResult discoveryResult = ...
        //collectResult.getDiscoveryResult(true);
        
        // TODO Add events and metric data using:
        // ResourceCollectResult#addMetricData(MetricData) 
        // and ResourceCollectResult#addEvent(ExternalEvent) or
        // AdapterBase#addMetricData() and AdapterBase#addEvent() or
        // MetricDataCache#cacheMetricData(ResourceConfig, MetricData) 
        // and MetricDataCache#flushCachedData()
        
        final Long startTime = System.nanoTime();
        
        DiscoveryResult discoveryResult = collectResult.getDiscoveryResult(true);
        
        List<ResourceKey> resources = collectResourceDataFromESight(host);
        
        //注销会话 openid
        service.logout(openid);
        
        if (resources.size() == 0) {
            logger.error("No resources collected from server with IP " + host);
        } else {
            logger.info(resources.size() + " resources collected from server with IP " + host);
        }
        
        for (ResourceKey resourceKey : resources) {
            
            if (isNewResource(resourceKey)) {
                discoveryResult.addResource(new ResourceConfig(resourceKey));
            }
            
            // Check if resource is part of monitored set, otherwise continue
            ResourceConfig resourceConfig = getMonitoringResource(resourceKey);
            if (resourceConfig == null) {
                continue;
            }
            
            // Add metrics
            addMetricData(resourceConfig, metricsByResource.get(resourceKey));
            
            // Add relationships
            Relationships rel = new Relationships();
            rel.setRelationships(resourceKey, relationshipsByResource.get(resourceKey),
                    Collections.singleton(getAdapterKind()));
            discoveryResult.addRelationships(rel);
            
        }
        
        Long seconds = TimeUnit.SECONDS.convert(System.nanoTime() - startTime, TimeUnit.NANOSECONDS);
        logger.info("Collected resource from esight elapsed time is " + seconds + " seconds.");
        
        if (seconds > Constant.DEFAULT_COLLECT_INTERVAL * 60) {
            logger.error("PLEASE UPDATE THE COLLECT INTERVAL GREATER THAN " + seconds + " SECONDS.");
        } 
        
    }
    
    /**
     * This method is called when a new adapter instance is created.
     */
    @Override
    public void onConfigure(ResourceStatus resStatus,
            ResourceConfig adapterInstResource) {
        
        // get the adapter instance properties(IdentifierCredentialProperties)
        // and use it as part of the onCollect
        // final IdentifierCredentialProperties prop =
        // new IdentifierCredentialProperties(loggerFactory, adapterInstResource);
        
        adapterInstResource.setInterval(Constant.DEFAULT_COLLECT_INTERVAL);
        
        final IdentifierCredentialProperties prop = 
                new IdentifierCredentialProperties(loggerFactory, adapterInstResource);
        String host = prop.getIdentifier(Constant.KEY_SERVER_IP_ADDRESS, "").trim();
        logger.info("A new adapter instance with server ip '" + host + "' is created");
    }
    
    /**
     * This method is called when presses "Test" button while
     * creating/editing an adapter instance.
     */
    @Override
    public boolean onTest(TestParam testParam) {
        
        // TODO get the adapter instance properties(IdentifierCredentialProperties)
        // and perform a test.
        ResourceConfig adapterInstanceResource = testParam.getAdapterConfig().getAdapterInstResource();
        final IdentifierCredentialProperties prop = 
                new IdentifierCredentialProperties(loggerFactory, adapterInstanceResource);
        
        if (logger.isInfoEnabled()) {
            logger.info("Inside onTest method of ESightAdapter class");
        }
        
        String empty = "";
        String host = prop.getIdentifier(Constant.KEY_SERVER_IP_ADDRESS, empty).trim();
        int serverPort = prop.getIntIdentifier(Constant.KEY_ESIGHT_SERVER_PORT, 
                Constant.DEFAULT_ESIGHT_SERVER_PORT);
        String username = prop.getCredential(Constant.KEY_ESIGHT_ACCOUNT);
        String password = prop.getCredential(Constant.KEY_ESIGHT_CODE);
        
        try {
            ESightService service = new ESightServiceImpl();
            String openid = service.login(host, serverPort, username, password);
            
            if (openid == null || openid.isEmpty()) {
                return false;
            } 
            service.logout(openid);
        } catch (EsightException e) {
            logger.error("Test eSight login failed.", e);
            return false;
        }
        
        return true;
    }
}
