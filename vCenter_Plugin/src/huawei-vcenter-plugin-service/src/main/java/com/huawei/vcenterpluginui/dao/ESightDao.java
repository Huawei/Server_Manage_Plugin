package com.huawei.vcenterpluginui.dao;

import com.huawei.vcenterpluginui.entity.ESight;
import com.huawei.vcenterpluginui.utils.VersionUtils;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class ESightDao extends H2DataBaseDao {
	
    public ESight getESightById(int id) throws SQLException {
        Connection con = null;
        PreparedStatement ps = null;
        ResultSet rs = null;
        try {
            con = getConnection();
            ps = con.prepareStatement("select * from HW_ESIGHT_HOST where id = ?");
            ps.setInt(1, id);
            rs = ps.executeQuery();
            if (rs.next()) {
                ESight eSight = new ESight();
                eSight.setHostIp(rs.getString("HOST_IP"));
                eSight.setAliasName(rs.getString("ALIAS_NAME"));
                eSight.setLoginAccount(rs.getString("LOGIN_ACCOUNT"));
                eSight.setLoginPwd(rs.getString("LOGIN_PWD"));
                eSight.setId(rs.getInt("ID"));
                eSight.setHostPort(rs.getInt("HOST_PORT"));
                SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
                eSight.setCreateTime(sdf.format(rs.getTimestamp("CREATE_TIME")));
                eSight.setLastModify(sdf.format(rs.getTimestamp("LAST_MODIFY_TIME")));
                return eSight;
            }
        } catch (SQLException e) {
            LOGGER.error(e);
            throw e;
        } finally {
            closeConnection(con, ps, rs);
        }
        return null;
    }

    public ESight getESightByIp(String ip) throws SQLException {
    	checkIp(ip);
    	
        Connection con = null;
        PreparedStatement ps = null;
        ResultSet rs = null;
        try {
            con = getConnection();
            ps = con.prepareStatement("select * from HW_ESIGHT_HOST where HOST_IP = ?");
            ps.setString(1, ip);
            rs = ps.executeQuery();
            if (rs.next()) {
                ESight eSight = new ESight();
                eSight.setHostIp(rs.getString("HOST_IP"));
                eSight.setAliasName(rs.getString("ALIAS_NAME"));
                eSight.setLoginAccount(rs.getString("LOGIN_ACCOUNT"));
                eSight.setLoginPwd(rs.getString("LOGIN_PWD"));
                eSight.setId(rs.getInt("ID"));
                eSight.setHostPort(rs.getInt("HOST_PORT"));
                SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
                eSight.setCreateTime(sdf.format(rs.getTimestamp("CREATE_TIME")));
                eSight.setLastModify(sdf.format(rs.getTimestamp("LAST_MODIFY_TIME")));
                return eSight;
            }
        } catch (SQLException e) {
            LOGGER.error(e);
            throw e;
        } finally {
            closeConnection(con, ps, rs);
        }
        return null;

    }

    public List<ESight> getESightList(String ip, int pageNo, int pageSize) throws SQLException {
    	checkNullIp(ip);
    	
        Connection con = null;
        PreparedStatement ps = null;
        ResultSet rs = null;
        try {
            con = getConnection();
            StringBuffer sql = new StringBuffer();
            sql.append("select * from HW_ESIGHT_HOST");
            if (ip != null && !ip.isEmpty()) {
            	sql.append(" where HOST_IP like ?");
            }
            
			if (pageSize > 0) {
				sql.append(" limit ? offset ?");
			}
            ps = con.prepareStatement(sql.toString());
            
            int i = 1;
            
            if (ip != null && !ip.isEmpty()) {
                ps.setString(i++, "%" + ip + "%");
            }
            
            if (pageSize > 0) {
				ps.setInt(i++, pageSize);
				ps.setInt(i++, (pageNo - 1) * pageSize);
			}
            rs = ps.executeQuery();
            List<ESight> eSightList = new ArrayList<>();
            while (rs.next()) {
                ESight eSight = new ESight();
                eSight.setHostIp(rs.getString("HOST_IP"));
                eSight.setLoginAccount(rs.getString("LOGIN_ACCOUNT"));
//                eSight.setLoginPwd(rs.getString("LOGIN_PWD"));
                eSight.setAliasName(rs.getString("ALIAS_NAME"));
                eSight.setId(rs.getInt("ID"));
                eSight.setHostPort(rs.getInt("HOST_PORT"));
                SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
                eSight.setCreateTime(sdf.format(rs.getTimestamp("CREATE_TIME")));
                eSight.setLastModify(sdf.format(rs.getTimestamp("LAST_MODIFY_TIME")));
                eSightList.add(eSight);
            }
            return eSightList;
        } catch (SQLException e) {
            LOGGER.error(e);
            throw e;
        } finally {
            closeConnection(con, ps, rs);
        }

    }
    
	public int getESightListCount(String ip) throws SQLException {
		checkNullIp(ip);
		
		Connection con = null;
		PreparedStatement ps = null;
		ResultSet rs = null;
		int count = 0;
		try {
			con = getConnection();
			StringBuffer sql = new StringBuffer();
			sql.append("select count(*) as count from HW_ESIGHT_HOST");
			if (ip != null && !ip.isEmpty()) {
				sql.append(" where HOST_IP like ?");
			}
			ps = con.prepareStatement(sql.toString());

			int i = 1;
			if (ip != null && !ip.isEmpty()) {
				ps.setString(i, "%" + ip + "%");
			}

			rs = ps.executeQuery();
			if (rs.next()) {
				count = rs.getInt("count");
			}

		} catch (SQLException e) {
			LOGGER.error(e);
			throw e;
		} finally {
			closeConnection(con, ps, rs);
		}
		return count;
	}

    public int saveESight(ESight eSight) throws SQLException {
    	checkEsight(eSight);
    	
        Connection con = null;
        PreparedStatement ps = null;
        ResultSet rs = null;
        try {
            con = getConnection();
            ps = con.prepareStatement(
                    "insert into HW_ESIGHT_HOST (HOST_IP,ALIAS_NAME,HOST_PORT,LOGIN_ACCOUNT,LOGIN_PWD,RESERVED_STR1,CREATE_TIME, LAST_MODIFY_TIME) values (?,?,?,?,?,?,CURRENT_TIMESTAMP,CURRENT_TIMESTAMP)");
            ps.setString(1, eSight.getHostIp());
            ps.setString(2, eSight.getAliasName());
            ps.setInt(3, eSight.getHostPort());
            ps.setString(4, eSight.getLoginAccount());
            ps.setString(5, eSight.getLoginPwd());
            ps.setString(6, VersionUtils.getVersion());
            
			int re = ps.executeUpdate();
			if (re > 0) {
				rs = ps.getGeneratedKeys();
				if (rs.next()) {
					int deptno = rs.getInt(1);
					LOGGER.info("save esight info successful,esight id:" + deptno);
				}
			}
			return re;
        } catch (SQLException e) {
            LOGGER.error(e);
            throw e;
        } finally {
            closeConnection(con, ps, rs);
        }
    }

    public int updateESight(ESight eSight) throws SQLException {
    	checkEsight(eSight);
    	
        Connection con = null;
        PreparedStatement ps = null;
        try {
            con = getConnection();
            ps = con.prepareStatement(
                    "update HW_ESIGHT_HOST set HOST_IP = ? , ALIAS_NAME = ?, HOST_PORT = ? , LOGIN_ACCOUNT = ? , LOGIN_PWD = ? , LAST_MODIFY_TIME = CURRENT_TIMESTAMP where ID = ?");
            ps.setString(1, eSight.getHostIp());
            ps.setString(2, eSight.getAliasName());
            ps.setInt(3, eSight.getHostPort());
            ps.setString(4, eSight.getLoginAccount());
            ps.setString(5, eSight.getLoginPwd());
            ps.setInt(6, eSight.getId());
            return ps.executeUpdate();
        } catch (SQLException e) {
            LOGGER.error(e);
            throw e;
        } finally {
            closeConnection(con, ps, null);
        }
    }
    
	public int deleteESight(List<Integer> id) throws SQLException {
		checkIds(id);

		if (id.isEmpty()) {
			return 0;
		}

		Connection con = null;
		PreparedStatement ps = null;
		try {
			con = getConnection();
			StringBuffer sql = new StringBuffer();
			sql.append("delete from HW_ESIGHT_HOST where ID in (");
			for (int i = 0; i < id.size(); i++) {
				sql.append("?,");
			}
			sql.deleteCharAt(sql.length() - 1);
			sql.append(")");
			ps = con.prepareStatement(sql.toString());
			for (int i = 0; i < id.size(); i++) {
				ps.setInt(i + 1, id.get(i));
			}

			int result = ps.executeUpdate();

			closeConnection(null, ps, null);

			// remove relations
			// delete tasks
			StringBuffer tsql = new StringBuffer();
			tsql.append("delete from HW_ESIGHT_TASK where HW_ESIGHT_HOST_ID in (");
			for (int i = 0; i < id.size(); i++) {
				tsql.append("?,");
			}
			tsql.deleteCharAt(tsql.length() - 1);
			tsql.append(")");
			ps = con.prepareStatement(tsql.toString());
			for (int i = 0; i < id.size(); i++) {
				ps.setInt(i + 1, id.get(i));
			}
			result += ps.executeUpdate();

			return result;
		} catch (SQLException e) {
			LOGGER.error(e);
			throw e;
		} finally {
			closeConnection(con, ps, null);
		}
	}

	private void checkNullIp(String ip) throws SQLException {
		if (ip != null && ip.length() > 255) {
			throw new SQLException("parameter ip is not correct");
		}
	}
	
	private void checkIp(String ip) throws SQLException {
		if (ip == null || ip.length() > 255) {
			throw new SQLException("parameter ip is not correct");
		}
	}

	private void checkIds(List<Integer> ids) throws SQLException {
		if (ids == null || ids.size() > 1000) {
			throw new SQLException("parameter ids is not correct");
		}
	}

	private void checkAliasName(String aliasName) throws SQLException {
		if (aliasName != null && aliasName.length() > 255) {
			throw new SQLException("parameter aliasName is not correct");
		}

		String regEx = "[a-zA-Z0-9_\\-\\.]{1,100}";
		Matcher matcher = Pattern.compile(regEx).matcher(aliasName);
		if (aliasName != null && aliasName.length() >0 && !matcher.matches()) {
			throw new SQLException("parameter aliasName is not correct");
		}
	}

	private void checkLoginAccount(String loginAccount) throws SQLException {
		if (loginAccount == null || loginAccount.length() > 255) {
			throw new SQLException("parameter loginAccount is not correct");
		}
	}

	private void checkLoginPwd(String loginPwd) throws SQLException {
		if (loginPwd == null || loginPwd.length() > 255) {
			throw new SQLException("parameter loginPwd is not correct");
		}
	}

	private void checkEsight(ESight eSight) throws SQLException {
		checkIp(eSight.getHostIp());
		checkAliasName(eSight.getAliasName());
		checkLoginAccount(eSight.getLoginAccount());
		checkLoginPwd(eSight.getLoginPwd());
	}
}
