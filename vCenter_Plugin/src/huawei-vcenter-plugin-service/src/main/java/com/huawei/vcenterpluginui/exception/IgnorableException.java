package com.huawei.vcenterpluginui.exception;

/**
 * Created by hyuan on 2017/6/9.
 */
public class IgnorableException extends VcenterException {
    public IgnorableException() {
        super();
    }

    public IgnorableException(String message) {
        super(message);
    }

    public IgnorableException(String code, String message) {
        super(code, message);
    }
}
