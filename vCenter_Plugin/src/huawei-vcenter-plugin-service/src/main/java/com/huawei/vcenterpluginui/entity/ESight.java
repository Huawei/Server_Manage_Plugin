package com.huawei.vcenterpluginui.entity;

import java.io.Serializable;

import com.huawei.esight.bean.Esight;
import com.huawei.vcenterpluginui.utils.CipherUtils;

public class ESight extends Esight implements Serializable {

	public ESight(String hostIp, int hostPort, String loginAccount, String loginPwd) {
		super(hostIp, hostPort, loginAccount, loginPwd);
	}

	public ESight() {
		super(null, 0, null, null);
	}

	private static final long serialVersionUID = -9063117479397179007L;

	private int id;
	private String aliasName;
	private String latestStatus;
	private String reservedInt1;
	private String reservedInt2;
	private String reservedStr1;
	private String reservedStr2;
	private String lastModify;
	private String createTime;

	public String getLatestStatus() {
		return latestStatus;
	}

	public void setLatestStatus(String latestStatus) {
		this.latestStatus = latestStatus;
	}

	public String getReservedInt1() {
		return reservedInt1;
	}

	public void setReservedInt1(String reservedInt1) {
		this.reservedInt1 = reservedInt1;
	}

	public String getReservedInt2() {
		return reservedInt2;
	}

	public void setReservedInt2(String reservedInt2) {
		this.reservedInt2 = reservedInt2;
	}

	public String getReservedStr1() {
		return reservedStr1;
	}

	public void setReservedStr1(String reservedStr1) {
		this.reservedStr1 = reservedStr1;
	}

	public String getReservedStr2() {
		return reservedStr2;
	}

	public void setReservedStr2(String reservedStr2) {
		this.reservedStr2 = reservedStr2;
	}

	public int getId() {
		return id;
	}

	public void setId(int id) {
		this.id = id;
	}

	public String getAliasName() {
		return aliasName;
	}

	public void setAliasName(String aliasName) {
		this.aliasName = aliasName;
	}

	public String getLastModify() {
		return lastModify;
	}

	public void setLastModify(String lastModify) {
		this.lastModify = lastModify;
	}

	public String getCreateTime() {
		return createTime;
	}

	public void setCreateTime(String createTime) {
		this.createTime = createTime;
	}

    @Override
    public String toString() {
        return "ESight [id=" + id + ", hostIp=" + getHostIp() + ", hostPort=" + getHostPort() + ", loginAccount=" + getLoginAccount()
                + ", loginPwd=******" + ", latestStatus=" + latestStatus + ", reservedInt1=" + reservedInt1
                + ", reservedInt2=" + reservedInt2 + ", reservedStr1=" + reservedStr1 + ", reservedStr2=" + reservedStr2
                + ", lastModify=" + lastModify + ", createTime=" + createTime + "]";
    }

	/**
	 * 新建已解密密码的eSight对象
	 * @param esight
	 * @return 已解密密码对象
	 */
	public static Esight newEsightWithDecryptedPassword(Esight esight) {
		return esight == null ? null :
				new Esight(esight.getHostIp(), esight.getHostPort(), esight.getLoginAccount(), CipherUtils.aesDncode(esight.getLoginPwd()));
	}

	/**
	 * 加密eSight对象的登录密码
	 * @param esight
	 */
	public static void updateEsightWithEncryptedPassword(ESight esight) {
		if (esight != null ) {
			esight.setLoginPwd(CipherUtils.aesEncode(esight.getLoginPwd()));
		}
	}
}
