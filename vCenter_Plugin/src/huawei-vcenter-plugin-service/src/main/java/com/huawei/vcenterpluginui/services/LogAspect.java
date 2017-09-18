package com.huawei.vcenterpluginui.services;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;
import org.aspectj.lang.JoinPoint;

import javax.servlet.http.HttpServletRequest;
import java.util.Arrays;

/**
 * Created by hyuan on 2017/6/12.
 */
public class LogAspect {

    private static final Log LOGGER = LogFactory.getLog(LogAspect.class);
    private static final String UNKNOWN = "UNKNOWN";

    public void logRequest(JoinPoint joinPoint) {
        try {
            String requestFrom = UNKNOWN;
            if (joinPoint.getArgs() != null) {
                for (Object param : joinPoint.getArgs()) {
                    if (param instanceof HttpServletRequest) {
                        requestFrom = ((HttpServletRequest) param).getRemoteAddr();
                        break;
                    }
                }
            }
			LOGGER.info("Request from " + requestFrom + ": " + joinPoint.getSignature().toShortString() + " "
					+ Arrays.toString(joinPoint.getArgs()).replaceAll("\"password\":\"[^&]*\"", "\"password\":\"******\"").replaceAll("Password\":\"[^&]*\"", "Password\":\"******\""));
        } catch (Exception e) {
            LOGGER.warn(e);
        }
    }

}
