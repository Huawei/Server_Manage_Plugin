package com.huawei.vcenterpluginui.task;

import java.security.NoSuchAlgorithmException;
import java.sql.SQLException;
import java.util.List;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.scheduling.annotation.Scheduled;    
import org.springframework.stereotype.Component;

import com.huawei.esight.bean.Esight;
import com.huawei.vcenterpluginui.dao.ESightDao;
import com.huawei.vcenterpluginui.entity.ESight;
import com.huawei.vcenterpluginui.utils.CipherUtils;
import com.huawei.vcenterpluginui.utils.FileUtils;

@Component("RefreshKeyJob") 
public class RefreshKeyTask {
	
	@Autowired
	private ESightDao eSightDao;
	
	private static final Log LOGGER = LogFactory.getLog(RefreshKeyTask.class);
	
	@Scheduled(cron = "0 0/5 * * * ?")  
    public void job1() {  
		LOGGER.info("更新密钥中。。。");  
		try {
			//获取用户密码等信息
			List<ESight> eSightList = eSightDao.getESightList(null, -1, -1);
			for (ESight eSight : eSightList) {
				Esight eSightInfo = eSightDao.getESightById(eSight.getId());
				eSight.setLoginPwd(ESight.newEsightWithDecryptedPassword(eSightInfo).getLoginPwd());
			}
			
			//更新密钥，重新加密
			String fileStringKey = CipherUtils.getSafeRandomToString(CipherUtils.KEY_SIZE);
			FileUtils.saveWorkKey(fileStringKey);
			for (ESight eSight : eSightList) {
				ESight.updateEsightWithEncryptedPassword(eSight);
				eSightDao.updateESight(eSight);
			}
			
		} catch (NoSuchAlgorithmException e) {
			LOGGER.error(e.getMessage());
		}catch (SQLException e) {
			LOGGER.error(e.getMessage());
		} 
    }

	public ESightDao geteSightDao() {
		return eSightDao;
	}

	public void seteSightDao(ESightDao eSightDao) {
		this.eSightDao = eSightDao;
	} 
	
}
