package com.huawei.vcenter.utils;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import java.io.BufferedReader;
import java.io.File;
import java.io.IOException;
import java.io.InputStreamReader;

/**
 * Created by hyuan on 2017/7/5.
 */
public class KeytookUtil {

    private static final String KEY ="tomcat.keystore";

    protected static final Log LOGGER = LogFactory.getLog(KeytookUtil.class);

    public static void genKey() throws IOException {
        LOGGER.info("Checking keystore file...");
        File file = new File(KEY);
        if (!file.exists()) {
            LOGGER.info("Generating keystore file...");
            String cmd = "keytool -genkeypair -alias tomcat -keyalg RSA  -keystore ./tomcat.keystore  -keypass changeit -storepass changeit -dname CN=localhost,OU=cn,O=cn,L=cn,ST=cn,C=cn";
            Runtime.getRuntime().exec(cmd);
        } else {
            LOGGER.info("Keystore file exists");
        }
    }

    public static String getKeystoreServerThumbprint() throws IOException {
        Process process = null;
        BufferedReader input = null;
        InputStreamReader inr = null;
        try {
            String cmd = "keytool -list -keypass changeit -storepass changeit -keystore ./tomcat.keystore";
            process = Runtime.getRuntime().exec(cmd);
            inr = new InputStreamReader(process.getInputStream(),"utf-8");
            input = new BufferedReader(inr);
            String line = "";
            while ((line = input.readLine()) != null) {
                if (line.indexOf("(SHA1):") > -1) {
                    return line.substring(line.indexOf("(SHA1):") + "(SHA1):".length() + 1);
                }
            }
        } finally {
            if (input != null) {
                input.close();
            }

            if (inr != null) {
                inr.close();
            }
        }
        return null;
    }
}
