package com.huawei.vcenterpluginui.exception;

/**
 * Created by hyuan on 2017/6/9.
 */
public class NoEsightException extends IgnorableException {
    public NoEsightException() {
        super("-90002", "no esight in DB");
    }

}
