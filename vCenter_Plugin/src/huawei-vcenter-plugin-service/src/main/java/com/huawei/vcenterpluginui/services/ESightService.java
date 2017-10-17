package com.huawei.vcenterpluginui.services;

import java.io.IOException;
import java.sql.SQLException;
import java.util.List;
import java.util.Map;

import com.huawei.vcenterpluginui.entity.ESight;

import javax.servlet.http.HttpSession;

import org.springframework.web.bind.annotation.RequestParam;

/**
 * It must be declared as osgi:service with the same name in
 * main/resources/META-INF/spring/bundle-context-osgi.xml
 */
public interface ESightService {
	/**
	 * @return save eSight message
	 */
	int saveESight(ESight eSight, HttpSession session) throws SQLException;

	List<ESight> getESightList(String ip, int pageNo, int pageSize) throws SQLException;
	
	int getESightListCount(String ip) throws SQLException;
	
	int deleteESights(String ids) throws SQLException,IOException;

	Map connect(ESight eSight);
}
