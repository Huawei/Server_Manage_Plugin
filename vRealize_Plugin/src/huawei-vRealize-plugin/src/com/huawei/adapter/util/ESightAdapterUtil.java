/*
 * Copyright (c) 2014-2015 VMware, Inc. All rights reserved.
 */

package com.huawei.adapter.util;

import com.integrien.alive.common.adapter3.describe.AdapterDescribe;
import com.integrien.alive.common.util.InstanceLoggerFactory;

import java.io.File;
import org.apache.log4j.Logger;

/**
 * Utility class.
 */
public class ESightAdapterUtil {
    
    private final Logger logger;
    
    public ESightAdapterUtil(InstanceLoggerFactory loggerFactory) {
        this.logger = loggerFactory.getLogger(ESightAdapterUtil.class);
    }
    
    /**
     * Utility Method to create Adapter describe. Instance of AdapterDescribe is
     * created using describe.xml kept under config folder.
     *
     * @return instance of AdapterDescribe class {@link AdapterDescribe}
     */
    public AdapterDescribe createAdapterDescribe() {
        
        assert (logger != null);
        
        // AdapterDescribe has all static information about the adapter like
        // what
        // all resource kinds (object types)
        // are supported by the adapter, what all metrics are expected for those
        // resource kinds
        
        AdapterDescribe adapterDescribe = AdapterDescribe
                .make(getDescribeXmlLocation() + "describe.xml");
        if (adapterDescribe == null) {
            logger.error("Unable to load adapter describe");
        } else {
            logger.info("Successfully loaded adapter");
        }
        return adapterDescribe;
    }
    
    /**
     * Method to return Adapter's home directory/folder.
     *
     * @return Adapter home folder path
     */
    public static String getAdapterHome() {
        // intentionally left constant CommononalConstants.ADAPTER_HOME for
        // common.jar version compatibility
        String adapterHome = System.getProperty("ADAPTER_HOME");
        String collectorHome = System.getProperty("COLLECTOR_HOME");
        if (collectorHome != null) {
            adapterHome = collectorHome + File.separator + "adapters";
        }
        return adapterHome;
    }
    
    /**
     * Returns the adapters root folder.
     *
     * @return Adapter home folder path
     */
    public static String getAdapterFolder() {
        return getAdapterHome() + File.separator + "ESightAdapter"
                + File.separator;
    }
    
    /**
     * Returns the adapters conf folder.
     *
     * @return adapters conf folder
     */
    public static String getConfFolder() {
        return getAdapterFolder() + "conf" + File.separator;
    }
    
    /**
     * Method to return describe XML location including the file name. It first
     * checks if
     *
     * @return describe XML location
     */
    public static String getDescribeXmlLocation() {
        
        String describeXml = null;
        String adapterHome = System.getProperty("ADAPTER_HOME");
        if (adapterHome == null) {
            describeXml = System.getProperty("user.dir") + File.separator;
        } else {
            describeXml = getConfFolder();
        }
        return describeXml;
    }
    
    /**
     * 判断字符串是相等.
     * @param a 比较字符a
     * @param b 比较字符b
     * @return 对比结果, true or false
     */
    public static boolean equals(CharSequence a, CharSequence b) {
        if (a == b) { 
            return true;
        }
        int length;
        if (a != null && b != null && (length = a.length()) == b.length()) {
            if (a instanceof String && b instanceof String) {
                return a.equals(b);
            } else {
                for (int i = 0; i < length; i++) {
                    if (a.charAt(i) != b.charAt(i)) {
                        return false;
                    }
                }
                return true;
            }
        }
        return false;
    }
}
