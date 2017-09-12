package com.huawei.vcenterpluginui.dao;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;

import com.huawei.vcenterpluginui.entity.TaskResource;

public class TaskResourceDao extends H2DataBaseDao {

	public List<TaskResource> getTaskResource(String taskName) throws SQLException {
		Connection con = null;
		PreparedStatement ps = null;
		ResultSet rs = null;

		try {

			con = getConnection();

			String sql = "select * from HW_TASK_RESOURCE where HW_ESIGHT_TASK_ID = ?";

			ps = con.prepareStatement(sql);
			ps.setString(1, taskName);
			rs = ps.executeQuery();

			List<TaskResource> taskResourceList = new ArrayList<>();

			while (rs.next()) {

				TaskResource taskResource = new TaskResource();
				taskResource.setHwESightTaskID(rs.getString("HW_ESIGHT_TASK_ID"));
				taskResource.setDn(rs.getString("DN"));
				taskResource.setIpAddress(rs.getString("IP_ADDRESS"));
				taskResource.setSyncStatus(rs.getString("SYNC_STATUS"));
				taskResource.setDeviceResult(rs.getString("DEVICE_RESULT"));
				taskResource.setErrorDetail(rs.getString("ERROR_DETAIL"));
				taskResource.setDeviceProgress(rs.getInt("DEVICE_PROGRESS"));
				taskResource.setLastModifyTime(rs.getString("LAST_MODIFY_TIME"));
				taskResource.setCreateTime(rs.getString("CREATE_TIME"));
				taskResource.setTaskType(rs.getString("TASK_TYPE"));
				taskResource.setErrorCode(rs.getString("ERROR_CODE"));

				taskResourceList.add(taskResource);
			}

			return taskResourceList;
		} catch (SQLException e) {
            LOGGER.error(e);
            throw e;
        }  finally {
			closeConnection(con, ps, rs);
		}

	}

	public int saveTaskResource(TaskResource taskResource) throws SQLException {

		Connection con = null;
		PreparedStatement ps = null;

		try {

			con = getConnection();
			ps = con.prepareStatement(
					"insert into HW_TASK_RESOURCE (HW_ESIGHT_TASK_ID,DN,IP_ADDRESS,SYNC_STATUS,TASK_TYPE,DEVICE_RESULT,ERROR_DETAIL,DEVICE_PROGRESS,ERROR_CODE,CREATE_TIME, LAST_MODIFY_TIME) values (?,?,?,?,?,?,?,?,?,CURRENT_TIMESTAMP,CURRENT_TIMESTAMP)");

			ps.setString(1, taskResource.getHwESightTaskID());
			ps.setString(2, taskResource.getDn());
			ps.setString(3, taskResource.getIpAddress());
			ps.setString(4, taskResource.getSyncStatus());
			ps.setString(5, taskResource.getTaskType());
			ps.setString(6, taskResource.getDeviceResult());
			ps.setString(7, taskResource.getErrorDetail());
			ps.setInt(8, taskResource.getDeviceProgress());
			ps.setString(9, taskResource.getErrorCode());

			return ps.executeUpdate();

		} catch (SQLException e) {
            LOGGER.error(e);
            throw e;
        }  finally {
			closeConnection(con, ps, null);
		}

	}

	public int updateTaskResource(TaskResource taskResource) throws SQLException {

		Connection con = null;
		PreparedStatement ps = null;

		try {

			con = getConnection();
			ps = con.prepareStatement(
					"update HW_TASK_RESOURCE set  SYNC_STATUS = ? ,DEVICE_RESULT = ?,ERROR_DETAIL = ?,DEVICE_PROGRESS=?,ERROR_CODE=?, LAST_MODIFY_TIME = CURRENT_TIMESTAMP where HW_ESIGHT_TASK_ID = ? and DN = ?");

			ps.setString(1, taskResource.getSyncStatus());
			ps.setString(2, taskResource.getDeviceResult());
			ps.setString(3, taskResource.getErrorDetail());
			ps.setInt(4, taskResource.getDeviceProgress());
			ps.setString(5, taskResource.getErrorCode());
			
			ps.setString(6, taskResource.getHwESightTaskID());
			ps.setString(7, taskResource.getDn());

			return ps.executeUpdate();

		} catch (SQLException e) {
            LOGGER.error(e);
            throw e;
        } finally {
			closeConnection(con, ps, null);
		}
	}
	
	public int updateTaskResourceNotExistDN(List<String> dnList, TaskResource taskResource) throws SQLException {

		Connection con = null;
		PreparedStatement ps = null;
		if (dnList != null && dnList.size() > 0) {
			try {

				con = getConnection();
				StringBuffer sql = new StringBuffer();
				sql.append("update HW_TASK_RESOURCE set  SYNC_STATUS = ? ,DEVICE_RESULT = ?,ERROR_DETAIL = ?,DEVICE_PROGRESS=?,ERROR_CODE=?, LAST_MODIFY_TIME = CURRENT_TIMESTAMP where HW_ESIGHT_TASK_ID = ? and DN not in (");
				for(String dn :dnList){
					sql.append("?,");
				}
				sql.deleteCharAt(sql.length()-1);
				sql.append(")");
				
				ps = con.prepareStatement(sql.toString());

				int i = 1;
				
				ps.setString(i++, taskResource.getSyncStatus());
				ps.setString(i++, taskResource.getDeviceResult());
				ps.setString(i++, taskResource.getErrorDetail());
				ps.setInt(i++, taskResource.getDeviceProgress());
				ps.setString(i++, taskResource.getErrorCode());

				ps.setString(i++, taskResource.getHwESightTaskID());
				
				for(int postion = 0 ; postion< dnList.size() ; postion++){
					ps.setString(i++,dnList.get(postion));
				}

				return ps.executeUpdate();

			} catch (SQLException e) {
				LOGGER.error(e);
				throw e;
			} finally {
				closeConnection(con, ps, null);
			}

		}
		return 0;
	}

}

