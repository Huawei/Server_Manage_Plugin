package com.huawei.vcenterpluginui.services;

import com.huawei.esight.api.provider.DefaultOpenIdProvider;
import com.huawei.esight.utils.JsonUtil;
import com.huawei.vcenterpluginui.entity.ESight;
import com.huawei.vcenterpluginui.exception.NoEsightException;
import com.huawei.vcenterpluginui.provider.SessionOpenIdProvider;

import javax.servlet.http.HttpSession;

import java.io.IOException;
import java.sql.SQLException;
import java.util.List;
import java.util.Map;

/**
 * Implementation of the EchoService interface
 */
public class ESightServiceImpl extends ESightOpenApiService implements ESightService {

    @Override
    public int saveESight(ESight eSight, HttpSession session) throws SQLException {
        ESight existEsight = eSightDao.getESightByIp(eSight.getHostIp());
        if (existEsight == null) {
        	// 加密esight密码
    		ESight.updateEsightWithEncryptedPassword(eSight);
        	
        	// 更新session中的openId
    		new SessionOpenIdProvider(eSight, session).updateOpenId();
    		
            return eSightDao.saveESight(eSight);
        } else {
        	if(eSight.getLoginAccount()==null||eSight.getLoginAccount().isEmpty()){
        	    // 未更新用户名密码
        		eSight.setLoginAccount(existEsight.getLoginAccount());
        		eSight.setLoginPwd(existEsight.getLoginPwd());
        	} else {
        	    // 更新了用户名密码，加密esight密码
                ESight.updateEsightWithEncryptedPassword(eSight);
                // 更新session中的openId
                new SessionOpenIdProvider(eSight, session).updateOpenId();
            }
            eSight.setId(existEsight.getId());
            return eSightDao.updateESight(eSight);
        }
    }

    @Override
    public List<ESight> getESightList(String ip, int pageNo, int pageSize) throws SQLException {
        List<ESight> eSightList = eSightDao.getESightList(ip, pageNo, pageSize);
        if (eSightList.isEmpty()) {
            throw new NoEsightException();
        }
        return eSightList;
    }

    @Override
    public Map connect(ESight eSight) {
        return new DefaultOpenIdProvider(eSight).call(null, null, Map.class);
    }

	@Override
	public int deleteESights(String ids) throws SQLException, IOException {
		Map<String, Object> idMap = JsonUtil.readAsMap(ids);
		List<Integer> id = (List<Integer>)idMap.get("ids");
		return eSightDao.deleteESight(id);
	}

	@Override
	public int getESightListCount(String ip) throws SQLException {
		return eSightDao.getESightListCount(ip);
	}

}
