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
        ZipFile zf = null;
        FileInputStream fin = null;
        InputStream in = null;
        ZipInputStream zin = null;
        BufferedReader br = null;
        String version = null;
        InputStreamReader inr = null;

        try {
            zf = new ZipFile(file);
            fin = new FileInputStream(file);
            in = new BufferedInputStream(fin);
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

                inr = new InputStreamReader(zf.getInputStream(ze),"utf-8");
                br = new BufferedReader(inr);

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
                try {
                zin.closeEntry();
                } catch (Exception var) {
                    var.printStackTrace();
                }
            }

            if(br != null) {
                try {
                br.close();
                } catch (Exception var) {
                    var.printStackTrace();
                }
            }

            if(zin != null) {
                try {
                zin.close();
                } catch (Exception var) {
                    var.printStackTrace();
                }
            }

            if(in != null) {
                try {
                in.close();
                } catch (Exception var) {
                    var.printStackTrace();
                }
            }

            if(inr != null){
                try {
                inr.close();
                } catch (Exception var) {
                    var.printStackTrace();
                }
            }

            if(fin != null){
                try {
                fin.close();
                } catch (Exception var) {
                    var.printStackTrace();
                }
            }

            if(zf != null){
                try {
                zf.close();
                } catch (Exception var) {
                    var.printStackTrace();
                }
            }
        }

        return version;
    }
}
