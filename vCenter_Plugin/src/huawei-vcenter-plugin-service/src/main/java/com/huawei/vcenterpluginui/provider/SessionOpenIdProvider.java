package com.huawei.vcenterpluginui.provider;

import com.huawei.esight.api.provider.DefaultOpenIdProvider;
import com.huawei.esight.bean.Esight;
import com.huawei.esight.exception.ParseJsonException;
import com.huawei.esight.utils.JsonUtil;
import com.huawei.vcenterpluginui.entity.ESight;
import com.huawei.vcenterpluginui.exception.UnsupportedTypeException;
import com.huawei.vcenterpluginui.utils.OpenIdSessionManager;

import javax.servlet.http.HttpSession;
import java.io.IOException;
import java.util.Map;

/**
 * Created by hyuan on 2017/6/29.
 */
public class SessionOpenIdProvider extends DefaultOpenIdProvider {

    private HttpSession session;

    public SessionOpenIdProvider(Esight esight, HttpSession session) {
        super(ESight.newEsightWithDecryptedPassword(esight));
        this.session = session;
    }

    public String provide() {
        String openId = OpenIdSessionManager.getOpenIdFromSession(session, esight.getHostIp());
        if (openId == null || "".equals(openId.trim())) {
            openId = super.provide();
            OpenIdSessionManager.setOpenIdToSession(session, openId, esight.getHostIp());
        }
        return openId;
    }

    @Override
    public boolean isOpenIdExpired(Object result) {
        Map resultMap;
        if (result instanceof String) {
            try {
                resultMap = JsonUtil.readAsMap((String) result);
            } catch (IOException e) {
                throw new ParseJsonException(e.getMessage());
            }
        } else if (result instanceof Map) {
            resultMap = (Map) result;
        } else {
            throw new UnsupportedTypeException("Can not parse type of object: " + result);
        }
        return hasValidCode(resultMap);
    }

    @Override
    public void updateOpenId() {
        OpenIdSessionManager.setOpenIdToSession(session, super.provide(), esight.getHostIp());
    }

    private boolean hasValidCode(Map resultMap) {
        Object code = resultMap.get("code");
        if (code instanceof Integer) {
            return (Integer) code != 1204;
        } else if (code instanceof Double) {
            return (Double) code != 1204.0;
        }
        return !"1204".equals((String) code);
    }

}
