package com.huawei.vcenterpluginui.dao;

import org.junit.Test;
import java.sql.*; 

import com.huawei.vcenterpluginui.ContextSupported;

public class H2DataBaseTest extends ContextSupported {
	@Test
	public void testGetESight() throws Exception {
		 try {  
	            String sourceURL = "jdbc:h2:/Users/licong/git/vcenter-plugin/huawei-vcenter-plugin-service/src/main/resources/db/testH2DB.db";  
	            String user = "123";  
	            String key = "123";  
	  
	            try {  
	                Class.forName("org.h2.Driver");  
	            } catch (Exception e) {  
	                e.printStackTrace();  
	            }  
	            Connection conn = DriverManager.getConnection(sourceURL, user, key);  
	            Statement stmt = conn.createStatement();  
//	            stmt.execute("CREATE TABLE mytable(name VARCHAR(100),sex VARCHAR(10))");  
	            stmt.executeUpdate("INSERT INTO mytable VALUES('Steven Stander','male')");  
	            stmt.executeUpdate("INSERT INTO mytable VALUES('Elizabeth Eames','female')");  
	            ResultSet rset = stmt.executeQuery("select * from mytable");  
	            while (rset.next()) {  
	            	LOGGER.info(rset.getString("name"));  
	            }  
	            rset.close();  
	            stmt.close();  
	            conn.close();  
	        } catch (SQLException sqle) {  
	        	LOGGER.info(sqle);  
	        } 
	}
}
