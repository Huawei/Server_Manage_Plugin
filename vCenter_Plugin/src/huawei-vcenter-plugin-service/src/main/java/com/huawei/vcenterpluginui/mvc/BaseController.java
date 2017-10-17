package com.huawei.vcenterpluginui.mvc;

import java.io.IOException;
import java.sql.SQLException;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.ResponseStatus;
import org.springframework.web.client.ResourceAccessException;

import com.huawei.esight.exception.EsightException;
import com.huawei.esight.exception.NoOpenIdException;
import com.huawei.esight.utils.JsonUtil;
import com.huawei.vcenterpluginui.entity.ResponseBodyBean;
import com.huawei.vcenterpluginui.exception.NoEsightException;
import com.huawei.vcenterpluginui.exception.VcenterException;

/**
 * Created by hyuan on 2017/5/23.
 */
@Controller
public class BaseController {

    private static final Log LOGGER = LogFactory.getLog(BaseController.class);

    protected static final String CODE_SUCCESS = "0";
    
    protected static final String CODE_NOTALL_FAILURE = "-100000";

    private static final String CODE_FAILURE = "-99999";

    private static final String CODE_DB_EXCEPTION = "-70001";
    private static final String CODE_ESIGHT_RETURN_PWD_EXCEPTION = "1";
    private static final String CODE_ESIGHT_CONNECT_EXCEPTION = "-80010";
    private static final String CODE_NO_ESIGHT_EXCEPTION = "-80011";
    
    private static final String PREFIX = "-50";

    private static final String FIELD_CODE = "code";

    private static final String FIELD_DESCRIPTION = "description";

    private static final String FIELD_DATA = "data";

    private static final ResponseBodyBean FAILURE_BEAN = new ResponseBodyBean(CODE_FAILURE, null, null);

    private static final ResponseBodyBean SUCCESS_BEAN = new ResponseBodyBean(CODE_SUCCESS, null, null);

    @ExceptionHandler(NoEsightException.class)
    @ResponseStatus(HttpStatus.OK)
    protected Map<String, Object> handleException(NoEsightException exception) {
        LOGGER.debug("没有esight配置!", exception);
        return generateError(generateCode(exception.getCode()), exception.getMessage(), Collections.emptyList());
    }

    @ExceptionHandler(VcenterException.class)
    @ResponseStatus(HttpStatus.OK)
    protected Map<String, Object> handleException(VcenterException exception) {
        LOGGER.error("Vcenter插件异常!", exception);
        return generateError(generateCode(exception.getCode()), exception.getMessage(), null);
    }

    @ExceptionHandler(NoOpenIdException.class)
    @ResponseStatus(HttpStatus.OK)
    protected Map<String, Object> handleException(NoOpenIdException exception) {
        LOGGER.error("获取OpenId异常!", exception);
        if(exception.getCode()==null|| exception.getCode().isEmpty()){
        	exception.setCode(CODE_ESIGHT_CONNECT_EXCEPTION);
        }
		else if (CODE_ESIGHT_RETURN_PWD_EXCEPTION.equals(exception.getCode())) {
			exception.setCode("-33" + exception.getCode());
		}
        return generateError(generateCode(exception.getCode()), exception.getMessage(), null);
    }
    
	@ExceptionHandler(EsightException.class)
	@ResponseStatus(HttpStatus.OK)
	protected Map<String, Object> handleException(EsightException exception) {
		LOGGER.error("esight异常!", exception);
		if (CODE_NO_ESIGHT_EXCEPTION.equals(exception.getCode())) {
			return generateError(exception.getCode(), exception.getMessage(), null);
		}
		return generateError(PREFIX + exception.getCode(), exception.getMessage(), null);
	}
    
    @ExceptionHandler(ResourceAccessException.class)
    @ResponseStatus(HttpStatus.OK)
    protected Map<String, Object> handleException(ResourceAccessException exception) {
        LOGGER.error("连接esight异常!", exception);
        return generateError(generateCode(CODE_ESIGHT_CONNECT_EXCEPTION), exception.getMessage(), null);
    }

    @ExceptionHandler(SQLException.class)
    @ResponseStatus(HttpStatus.OK)
    protected Map<String, Object> handleException(SQLException exception) {
        LOGGER.error("DB异常!", exception);
        Map<String, Object> errorMap = new HashMap<>();
        errorMap.put(FIELD_CODE, CODE_DB_EXCEPTION);
        errorMap.put(FIELD_DESCRIPTION, exception.getMessage());
        errorMap.put(FIELD_DATA, null);
        return errorMap;
    }

	@ExceptionHandler(Exception.class)
	@ResponseStatus(HttpStatus.OK)
	protected Map<String, Object> handleException(Exception exception) {
		LOGGER.error("系统异常!", exception);
		return generateError(CODE_FAILURE, exception.getMessage(), null);
	}

    private Map<String, Object> generateError(String code, String message, Object data) {
        Map<String, Object> errorMap = new HashMap<>();
        errorMap.put(FIELD_CODE, code);
        errorMap.put(FIELD_DESCRIPTION, message);
        errorMap.put(FIELD_DATA, data);
        return errorMap;
    }

    public String generateCode() {
        return generateCode(null);
    }

    public String generateCode(String code) {
        return (code == null || code.isEmpty()) ? CODE_FAILURE : code;
    }

    protected ResponseBodyBean success() {
        return SUCCESS_BEAN;
    }

    protected ResponseBodyBean success(Object data) {
        return success(data, null);
    }

    protected ResponseBodyBean success(Object data, String description) {
        ResponseBodyBean bodyBean = null;
        bodyBean = new ResponseBodyBean(CODE_SUCCESS, null, null);
        bodyBean.setData(data);
        bodyBean.setDescription(description);
        return bodyBean;
    }
	protected ResponseBodyBean getResultByData(String data) throws IOException {
		return getResultByData(data, "");
	}
	protected ResponseBodyBean getResultByData(String data, String prefix) throws IOException {
		Map<String, Object> dataMap = JsonUtil.readAsMap(data);
		if (!CODE_SUCCESS.equals(String.valueOf(dataMap.get(FIELD_CODE)))) {
			if (dataMap.containsKey(FIELD_CODE)) {
				return failure(prefix + dataMap.get(FIELD_CODE).toString(), dataMap.get(FIELD_DESCRIPTION).toString(), dataMap);
			}
			return failure(CODE_FAILURE, "esight 返回异常", dataMap);
		} else {
			return success(dataMap, (String) dataMap.get(FIELD_DESCRIPTION));
		}
	}

    protected ResponseBodyBean failure() {
        return FAILURE_BEAN;
    }

    protected ResponseBodyBean failure(String description) {
        return failure(CODE_FAILURE, description);
    }

    protected ResponseBodyBean failure(String code, String description) {
        ResponseBodyBean bodyBean = null;
        bodyBean = new ResponseBodyBean(CODE_FAILURE, null, null);
        bodyBean.setDescription(description);
        bodyBean.setCode(code);
        return bodyBean;
    }

    protected ResponseBodyBean failure(String code, String description, Object data) {
        ResponseBodyBean bodyBean = null;
        bodyBean = new ResponseBodyBean(CODE_FAILURE, null, null);
        bodyBean.setDescription(description);
        bodyBean.setCode(code);
        bodyBean.setData(data);
        
        return bodyBean;
    }

    protected ResponseBodyBean listData(List<Map<String, Object>> dataMapList) {
    	return listData(dataMapList,"");
	}
    
    protected ResponseBodyBean listData(List<Map<String, Object>> dataMapList, String prefix) {
    	
		Boolean success = false;
		Boolean failure = false;
		int errorCode = 0;
		String errorDescription = null;

		for (Map<String, Object> dataMap : dataMapList) {
			if (!CODE_SUCCESS.equals(String.valueOf(dataMap.get(FIELD_CODE)))) {
				if (!failure) {
					errorCode = (int) dataMap.get(FIELD_CODE);
					errorDescription = (String) dataMap.get(FIELD_DESCRIPTION);
				}
				dataMap.put(FIELD_CODE, prefix + String.valueOf((int) dataMap.get(FIELD_CODE)));
				failure = true;
			} else {
				success = true;
			}
		}

		if (success && failure) {
			return failure(CODE_NOTALL_FAILURE, errorDescription, dataMapList);
		} else if (failure) {
			return failure(prefix + String.valueOf(errorCode), errorDescription, dataMapList);
		}

		return success(dataMapList);
	}
    
}
