package com.huawei.adapter.util;

import com.fasterxml.jackson.core.JsonParseException;
import com.fasterxml.jackson.databind.JsonMappingException;
import com.fasterxml.jackson.databind.ObjectMapper;

import java.io.IOException;

/**
 * 转义显示的工具类.
 * @author harbor
 *
 */
public class ConvertUtils {
    
    /**
     * 健康状态转换处理.
     * 0 = normal
     * -1 = offline
     * -2 = unknown
     * 其他 = error 
     * @param healthState 健康状态
     * @return 转换后的字符串
     */
    public static String convertHealthState(int healthState) {
        switch (healthState) {
            case 0 : {
                return "Normal";
            }
            case -1: {
                return "Offline";
            }
            case -2: {
                return "Unknown";
            }
            default: {
                return "Faulty";
            }
            
        } 
    }
    
    
    /**
     * RAID健康状态转换处理.
     * 1 = normal
     * 0 = offline
     * -1 = unknown
     * 其他 = error 
     * @param healthState 健康状态
     * @return 转换后的字符串
     */
    public static String convertHealthState4Raid(int healthState) {
        switch (healthState) {
            case 1 : {
                return "Normal";
            }
            case 0: {
                return "Offline";
            }
            case -1: {
                return "Unknown";
            }
            default: {
                return "Faulty";
            }
            
        } 
    }
    
    /**
     * 转换在位状态.
     * @param presentState 在位状态
     * @return 转换后的字符串
     */
    public static String convertPresentState(int presentState) {
        
        if (presentState == 0) {
            return "Not detected";
        } else {
            return "Detected";
        }
        
    }
    
    /**
     * 转换输入电源模式.
     * @param inputMode 输入电源模式
     * @return 转换后的字符串
     */
    public static String convertInputMode(int inputMode) {
        
        if (inputMode == 1) {
            return "AC";
        } else if (inputMode == 2) {
            return "DC";
        } else if (inputMode == 3) {
            return "AC/DC";
        } else {
            return "";
        }
        
    }
    
    /**
     * 转换电源协议.
     * @param powerProtocol 电源协议
     * @return 转换后的字符串
     */
    public static String convertPowerProtocol(int powerProtocol) {
        
        if (powerProtocol == 0) {
            return "PSMI";
        } else if (powerProtocol == 1) {
            return "PMBUS";
        } else {
            return "";
        }
        
    }
    
    /**
     * 转换风扇控制模式.
     * @param controlModel 风扇控制模式
     * @return 转换后的字符串
     */
    public static String convertControlModel(int controlModel) {
        
        if (controlModel == 0) {
            return "Automatic";
        } else if (controlModel == 1) {
            return "Manual";
        } else {
            return "";
        }
        
    }
    
    /**
     * 转换风扇转百分比.
     * @param rotatePercent 风扇转百分比
     * @return 转换后的字符串
     */
    public static String convertRotatePercent(int rotatePercent) {
        
        if (rotatePercent == 255) {
            return "Automatic";
        } else if (rotatePercent >= 0 && rotatePercent <= 100) {
            return "" + rotatePercent;
        } else {
            return "--";
        }
        
    }
    
    /**
     * 转换主板类型.
     * @param boardType 主板类型
     * @return 转换后的字符串
     */
    public static String convertBoardType(int boardType) {
        
        if (boardType == 0) {
            return "Blade";
        } else if (boardType == 1) {
            return "Switchboard";
        } else {
            return "";
        }
        
    }
    
    /**
     * 转换功率字符串(如750.0 W转换为750). 
     * @param source 源字符串
     * @return 转换结果
     */
    public static String convertPower(String source) {
        if (source == null || source.isEmpty()) {
            return "";
        }
        return source.replaceAll("\\.[0]+\\s+[^0-9]+$", "");
    }
    
    /**
     * 
     * @param <T> json字符转换为对象.
     * @param jsonString son字符
     * @param returnType 返回类型
     * @return 返回指定对象
     */
    public static <T> T json2Object(String jsonString, Class<T> returnType) {
        
        if (jsonString == null || jsonString.isEmpty()) {
            return null;
        }
        
        ObjectMapper objectMapper = new ObjectMapper();
        try {
            return objectMapper.readValue(jsonString.getBytes("UTF-8"), returnType);
        } catch (JsonParseException e) {
            return null;
        } catch (JsonMappingException e) {
            return null;
        } catch (IOException e) {
            return null;
        }
    }
    
    /**
     * 转换RAID的interfaceType.
     * @param interfaceType 接口类型
     * @return 转换结果
     */
    public static String covnertInterfaceType(String interfaceType) {
        switch (interfaceType) {
            case "1": {
                return "SPI";
            }
            case "2": {
                return "SAS-3G";
            }
            case "3": {
                return "SATA-1.5G";
            }
            case "4": {
                return "SATA-3G";
            }
            case "5": {
                return "SAS-6G";
            }
            case "6": {
                return "SAS-12G";
            }
            case "255": {
                return "Unknown";
            }
            default : {
                return "Unknown";
            }
        }
    }
    
    /**
     * check if resource offline.
     * @param presentState - online flag
     * @return check result
     */
    public static boolean isOffline(int presentState){
        return presentState == 0;
    }
    
}

