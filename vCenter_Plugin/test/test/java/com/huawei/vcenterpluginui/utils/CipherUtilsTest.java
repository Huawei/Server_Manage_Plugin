package com.huawei.vcenterpluginui.utils;

import org.junit.Assert;
import org.junit.Test;

import com.huawei.vcenterpluginui.ContextSupported;

public class CipherUtilsTest extends ContextSupported{
	
	@Test
	public void test(){
		String key = CipherUtils.aesEncode("测试passwordtest");
		String password = CipherUtils.aesDncode(key);
		Assert.assertEquals("测试passwordtest", password);
	}
	
	public static void main(String[] args){
//		String key = CipherUtils.AESEncode("passwordtest");
//		System.out.println(key);
//		String password = CipherUtils.AESDncode(key);
//		System.out.println(password);
	}
}
