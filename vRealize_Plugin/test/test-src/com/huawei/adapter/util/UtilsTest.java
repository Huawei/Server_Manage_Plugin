package com.huawei.adapter.util;

import static org.junit.Assert.assertEquals;

import org.junit.Test;

/**
 * ConvertUtils单元测试类.
 * @author harbor
 *
 */
public class UtilsTest {
    
    @Test
    public void testConvertHealthState() {
        assertEquals("Normal", ConvertUtils.convertHealthState(0));
        assertEquals("Offline", ConvertUtils.convertHealthState(-1));
        assertEquals("Unknown", ConvertUtils.convertHealthState(-2));
        assertEquals("Faulty", ConvertUtils.convertHealthState(-3));
        assertEquals("Faulty", ConvertUtils.convertHealthState(898));
    }
    
    @Test
    public void testConvertPower() {
        assertEquals("500", ConvertUtils.convertPower("500.0 W"));
    }
    
}
