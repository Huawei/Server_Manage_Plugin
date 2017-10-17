package com.huawei.vcenter;

import com.huawei.vcenter.utils.Validations;

import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.UnsupportedEncodingException;
import java.net.URLEncoder;
import java.nio.charset.StandardCharsets;
import java.util.Map;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

/**
 * Created by hyuan on 2017/7/6.
 */
@RestController
public class ApplicationController {

    @RequestMapping(value = "rest", method = RequestMethod.GET)
    public Map onload(HttpServletRequest request,HttpServletResponse response) throws UnsupportedEncodingException{
        String rqHd = request.getHeader("Accept-Language");
        if(rqHd != null){
            String rqHeader = URLEncoder.encode(rqHd, StandardCharsets.UTF_8.displayName());
            response.addHeader("Accept-Language", rqHeader);
        }
        return Validations.onloadChecker(request);
    }

    @RequestMapping(value = "rest", method = RequestMethod.POST)
    public Map onsubmit(@RequestBody Map<String, String> formData) {
        return Validations.onSubmit(formData.get("packageUrl"),
                formData.get("vcenterUsername"),
                formData.get("vcenterPassword"),
                formData.get("vcenterIP"),
                formData.get("vcenterPort"));
    }

    @RequestMapping(value = "rest/register", method = RequestMethod.POST)
    public Map register(@RequestBody Map<String, String> formData) {
        return Validations.onSubmit(formData.get("packageUrl"),
                formData.get("vcenterUsername"),
                formData.get("vcenterPassword"),
                formData.get("vcenterIP"),
                formData.get("vcenterPort"),
                formData.get("version"));
    }

    @RequestMapping(value = "rest/unregister", method = RequestMethod.POST)
    public Map unRegister(@RequestBody Map<String, String> formData) {
        return Validations.unRegister(formData.get("packageUrl"),
                formData.get("vcenterUsername"),
                formData.get("vcenterPassword"),
                formData.get("vcenterIP"),
                formData.get("vcenterPort"));
    }

    @RequestMapping(value = Constants.UPDATE_FILE, method = RequestMethod.GET)
    public void getPackage(HttpServletResponse response) throws IOException {
        getPackage(response,"huawei-vcenter-plugin");
    }

    @RequestMapping(value = "package/{zipName}", method = RequestMethod.GET)
    public void getPackage(HttpServletResponse response, @PathVariable String zipName) throws IOException {
        File file = new File(zipName + ".zip");
        response.setContentType("application/zip");
        response.setContentLengthLong(file.length());
        try (OutputStream out = response.getOutputStream();
             InputStream in = new FileInputStream(file)) {
            byte[] b = new byte[2048];
            int length;
            while ((length = in.read(b)) > 0) {
                out.write(b, 0, length);
            }
        }
    }
}
