package com.huawei.vcenter.utils;

import com.huawei.vcenter.Constants;

import org.springframework.http.HttpRequest;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import java.io.File;

import javax.servlet.http.HttpServletRequest;

/**
 * Created by hyuan on 2017/7/5.
 */
public class Validations {

    public static Map onSubmit(String packageUrl, String vcenterUsername, String vcenterPassword, String vcenterIP, String vcenterPort) {
        String version = getPackageVersion();
        if ((version == null) || (version.trim().equals(""))) {
            return Collections.singletonMap("error", "安装包版本号读取错误");
        }
        return onSubmit(packageUrl, vcenterUsername, vcenterPassword, vcenterIP, vcenterPort, version);
    }

    public static Map onSubmit(String packageUrl, String vcenterUsername, String vcenterPassword, String vcenterIP, String vcenterPort, String version) {
        if (!packageUrl.startsWith("https://")) {
            return Collections.singletonMap("error", "URL必须以https开始");
        }
        if ((vcenterIP == null) || (vcenterIP.isEmpty())) {
            return Collections.singletonMap("error", "vCenter IP不能为空");
        }
        if ((vcenterPort == null) || (vcenterPort.isEmpty())) {
            return Collections.singletonMap("error", "vCenter端口号不能为空");
        }
        String serverThumbprint;
        try {
            serverThumbprint = KeytookUtil.getKeystoreServerThumbprint();
        } catch (IOException e) {
            e.printStackTrace();
            return Collections.singletonMap("error", "获取证书指纹出错");
        }
        String pluginKey = "com.huawei.vcenterpluginui";
        VcenterRegisterRunner.run(version, packageUrl, serverThumbprint, vcenterIP, vcenterPort, vcenterUsername,
                vcenterPassword, pluginKey);
        return Collections.singletonMap("info", "check log");
    }

    public static Map unRegister(String packageUrl, String vcenterUsername, String vcenterPassword, String vcenterIP, String vcenterPort) {
        if ((vcenterIP == null) || (vcenterIP.isEmpty())) {
            return Collections.singletonMap("error", "vCenter IP不能为空");
        }
        if ((vcenterPort == null) || (vcenterPort.isEmpty())) {
            return Collections.singletonMap("error", "vCenter端口号不能为空");
        }
        String pluginKey = "com.huawei.vcenterpluginui";
        VcenterRegisterRunner.unRegister( vcenterIP, vcenterPort, vcenterUsername,
                vcenterPassword, pluginKey);
        return Collections.singletonMap("info", "check log");
    }

    public static Map onloadChecker(HttpServletRequest request) {
        Map<String, Object> returnMap = new HashMap<>();


        File keyFile = new File(Constants.KEYSTORE_FILE);
        if (!keyFile.exists()) {
            return Collections.singletonMap("error", "tomcat.keystore证书文件不存在.");
        }

        List<String> packageNameList = new ArrayList<>();
        List<String> versionList = new ArrayList<>();

        // check file
        File rootFile = new File("./");
        File[] fileList = rootFile.listFiles();
        if(fileList != null) {
            for (File file : fileList) {
                if (file.getName().lastIndexOf(".zip") >= 0) {
                    // check version
                    String version = getPackageVersion(file.getName());
                    if ((version == null) || (version.trim().equals(""))) {
                        continue;
                    }
                    packageNameList.add(file.getName());
                    versionList.add(version);
                }
            }
        }

        if (packageNameList.isEmpty()) {
            return Collections.singletonMap("error", "未找到更新包程序，请放入zip包然后刷新页面.");
        }

        returnMap.put("packageNameList",packageNameList);
        returnMap.put("versionList",versionList);
        returnMap.put("key",keyFile.getAbsolutePath());
        returnMap.put("path","https://" + request.getServerName() + ":" + request.getServerPort() + "/package/");
        return returnMap;
    }

    private static String getPackageVersion() {
        String version = null;
        try {
            version = ZipUtils.getVersionFromPackage(Constants.UPDATE_FILE);
        } catch (Exception e) {
            e.printStackTrace();
        }
        return version;
    }

    private static String getPackageVersion(String file) {
        String version = null;
        try {
            version = ZipUtils.getVersionFromPackage(file);
        } catch (Exception e) {
            e.printStackTrace();
        }
        return version;
    }
}
