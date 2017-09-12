package com.huawei.vcenter;

import com.huawei.vcenter.utils.KeytookUtil;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.builder.SpringApplicationBuilder;
import org.springframework.boot.context.embedded.EmbeddedServletContainerInitializedEvent;
import org.springframework.boot.web.support.SpringBootServletInitializer;
import org.springframework.context.ApplicationListener;

import java.io.IOException;
import java.net.InetAddress;
import java.net.UnknownHostException;

@SpringBootApplication
public class VcenterDeployerApplication extends SpringBootServletInitializer implements ApplicationListener<EmbeddedServletContainerInitializedEvent> {

//	private static int SERVER_PORT;

	protected static final Log LOGGER = LogFactory.getLog(VcenterDeployerApplication.class);

	@Override
	protected SpringApplicationBuilder configure(SpringApplicationBuilder application) {
		return application.sources(VcenterDeployerApplication.class);
	}

	@Override
	public void onApplicationEvent(EmbeddedServletContainerInitializedEvent event) {
//		this.SERVER_PORT = event.getEmbeddedServletContainer().getPort();
	}

	public static void main(String[] args) {
		try {
			KeytookUtil.genKey();
			LOGGER.info("Starting server...");
		} catch (IOException e) {
			e.printStackTrace();
		}

		SpringApplication.run(VcenterDeployerApplication.class, args);

		String url = "";
		try {
			String host = InetAddress.getLocalHost().getHostAddress();
			url = "https://" + host + ":8443";// + SERVER_PORT;
			LOGGER.info("Server has been started, " + url);
		} catch (UnknownHostException e) {
			e.printStackTrace();
		}
	}

}
