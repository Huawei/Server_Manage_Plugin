var testDatas = [];

var eSightManage = {
    //获取eSight配置列表
    getList: function (param, callback) {
        var keyword = param.param.hostIP;
        var listData = [];
        var eSightListUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/esight?ip="+keyword+"&pageNo="+param.param.pageNo+"&pageSize="+param.param.pageSize + "&s=" + Math.random();
        //通过API获取数据
        $.get(eSightListUrl, function(response){
        	console.log(response);
            if (typeof callback === "function") {
            	var dataCode = response.code;
                listData = response.data;
                var totalNum = 0;
                if(listData instanceof Array){
                	dataCode = listData.length == 0 ? "0" : response.code;
                	if(dataCode==="0"){
                	   totalNum = response.totalNum;
                	}
                }
                var ret = {code: dataCode, msg: response.description, data: response.data, totalNum: totalNum}
                callback(ret);
            }
          },"json");
    },
    ///编辑eSight配置
    edit: function (param, callback) {
        var postData = {
            hostIp: param.hostIp,
            aliasName: param.aliasName,
            hostPort: param.hostPort,
            loginAccount: param.loginAccount,
            loginPwd: param.loginPwd
        }
        var eSightSaveUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/esight?s=" + Math.random();
        $.ajax({  
            type: 'POST',
            contentType : 'application/json;charset=utf-8',
            url: eSightSaveUrl,  
            data: JSON.stringify(postData),
            dataType: "json",
            success: function(response){  
            	if (typeof callback === "function") {
                    var ret = {code: response.code, msg: response.description}
                    callback(ret);
                }
            }
        });
    },
    //删除eSight配置（单个或者批量）
    "delete": function (param, callback) {
        //删除代码逻辑 根据param.hostIp
        var eSightDeleteUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/esight/rm?s=" + Math.random();
        $.ajax({  
            type: 'POST',
            contentType : 'application/json;charset=utf-8',
            url: eSightDeleteUrl,  
            data: JSON.stringify(param),
            dataType: "json",
            success: function(response){  
            	if (typeof callback === "function") {
                    var ret = {code: response.code, msg: response.description}
                    callback(ret);
                }  
            }
        });
           
    },
    //测试eSight配置
    test: function (param, callback) {
        var postData = {
            condition: {
                hostIp: param.hostIp,
                hostPort: param.hostPort,
                loginAccount: param.loginAccount,
                loginPwd: param.loginPwd
            }
        }

        var eSightTestUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/esight/test?s=" + Math.random();
        $.ajax({  
            type: 'PUT', 
            contentType : 'application/json;charset=utf-8',
            url: eSightTestUrl,  
            data: JSON.stringify(postData.condition),
            dataType: "json",
            success: function(response){  
            	if (typeof callback === "function") {
                    var ret = {code: response.code, msg: response.description}
                    callback(ret);
                }  
            }
        });
    },
    //保存eSight配置
    add: function (param, callback) {
        var postData = {
            hostIp: param.hostIp,
            aliasName: param.aliasName,
            hostPort: param.hostPort,
            loginAccount: param.loginAccount,
            loginPwd: param.loginPwd
        }
        var eSightSaveUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/esight?s=" + Math.random();
        $.ajax({  
            type: 'POST',
            contentType : 'application/json;charset=utf-8',
            url: eSightSaveUrl,  
            data: JSON.stringify(postData),
            dataType: "json",
            success: function(response){  
            	if (typeof callback === "function") {
                    var ret = {code: response.code, msg: response.description}
                    callback(ret);
                }
            }
        });
    }
}