package com.huawei.vcenter.utils;

import java.util.HashMap;
import java.util.Map;

import com.vmware.automatic.plugin.registration.PluginRegistrationMain;

public class VcenterRegisterRunner {
	public static void run(String version, String packageUrl, String serverThumbprint, String vcenterIP,
						   String vcenterPort, String username, String password, String key) {
		Map<String, String> unRegParamMap = new HashMap();
		unRegParamMap.put("-action", "unregisterPlugin");
		unRegParamMap.put("-username", username);
		unRegParamMap.put("-password", password);
		unRegParamMap.put("-key", key);
		unRegParamMap.put("-url", "https://" + vcenterIP + ":" + vcenterPort + "/sdk");

		PluginRegistrationMain.main(convertMapToArray(unRegParamMap));

		Map<String, String> regParamMap = new HashMap();
		regParamMap.put("-action", "registerPlugin");
		regParamMap.put("-username", username);
		regParamMap.put("-password", password);
		regParamMap.put("-key", key);
		regParamMap.put("-version", version);
		regParamMap.put("-pluginUrl", packageUrl);
		regParamMap.put("--serverThumbprint", serverThumbprint);
		regParamMap.put("-url", "https://" + vcenterIP + ":" + vcenterPort + "/sdk");

		PluginRegistrationMain.main(convertMapToArray(regParamMap));
	}

	public static void unRegister(String vcenterIP,
								  String vcenterPort, String username, String password, String key) {
		Map<String, String> unRegParamMap = new HashMap();
		unRegParamMap.put("-action", "unregisterPlugin");
		unRegParamMap.put("-username", username);
		unRegParamMap.put("-password", password);
		unRegParamMap.put("-key", key);
		unRegParamMap.put("-url", "https://" + vcenterIP + ":" + vcenterPort + "/sdk");

		PluginRegistrationMain.main(convertMapToArray(unRegParamMap));
	}

	private static String[] convertMapToArray(Map<String, String> map) {
		if ((map == null) || (map.size() == 0)) {
			return new String[0];
		}
		String[] args = new String[map.size() * 2];
		int index = 0;
		for (Map.Entry<String, String> entry : map.entrySet()) {
			args[(index++)] = ((String) entry.getKey()).trim();
			args[(index++)] = ((String) entry.getValue()).trim();
		}
		return args;
	}
}
