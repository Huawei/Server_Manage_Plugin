package com.huawei.vcenter.utils;

import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.zip.ZipEntry;
import java.util.zip.ZipFile;
import java.util.zip.ZipInputStream;

public class ZipUtils {
	public static String getVersionFromPackage(String file) throws Exception {
        ZipFile zf = new ZipFile(file);
        InputStream in = null;
        ZipInputStream zin = null;
        BufferedReader br = null;
        String version = null;

        try {
            in = new BufferedInputStream(new FileInputStream(file));
            zin = new ZipInputStream(in);

            while(true) {
                ZipEntry ze;
                do {
                    do {
                        if((ze = zin.getNextEntry()) == null) {
                            return version;
                        }
                    } while(ze.isDirectory());
                } while(!"plugin-package.xml".equals(ze.getName()));

                br = new BufferedReader(new InputStreamReader(zf.getInputStream(ze)));

                String line;
                while((line = br.readLine()) != null) {
                    if(line.startsWith("<pluginPackage") && line.contains("version=\"")) {
                        line = line.substring(line.indexOf("version=\"") + "version=\"".length());
                        version = line.substring(0, line.indexOf(34));
                    }
                }
            }
        } catch (IOException var11) {
            var11.printStackTrace();
        } finally {
            if(zin != null) {
                zin.closeEntry();
            }

            if(br != null) {
                br.close();
            }

            if(zin != null) {
                zin.close();
            }

            if(in != null) {
                in.close();
            }

        }

        return version;
    }
}
