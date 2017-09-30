var eSightManage = {
    //获取eSight配置列表
    getList: function (param, callback) {
        if (!eSightManage.checkCefBrowserConnection()) return;

        //调用C#方法获取数据
				ExecuteAnynsMethod(param, "getList", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);
					console.log("getESightList result:");
					console.log(resultJson);
					var listData = [];
					listData = resultJson.data;

					if (typeof callback === "function") {
							var ret = { code: resultJson.code, msg: resultJson.description, data: listData, totalNum: resultJson.totalNum }
							callback(ret);
					}
				});	
    },
    //编辑eSight配置
    edit: function (param, callback) {
        eSightManage.add(param, callback);
    },
    //删除eSight配置（单个或者批量）
    delete: function (param, callback) {
        //删除代码逻辑 根据param.ids
        if (!eSightManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "delete", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);

					if (typeof callback === "function") {
							var ret = { code: resultJson.Code, msg: resultJson.ExceptionMsg }
							callback(ret);
					}
				});	
    },
    //测试eSight配置
    test: function (param, callback) {
        if (!eSightManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "test", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);

					if (typeof callback === "function") {
							var ret = { code: resultJson.Code, msg: resultJson.ExceptionMsg }
							callback(ret);
					}
				});
    },
    //保存eSight配置
    add: function (param, callback) {
        if (!eSightManage.checkCefBrowserConnection()) return;

        var ayncResult;
        if (param.loginPwd == null || param.loginPwd == undefined || param.loginPwd == "undefined") {
            ayncResult = NetBound.execute("saveNoCert", param);
        } else {
            ayncResult = NetBound.execute("save", param);
        }
				ExecuteAnynsMethodOnlyHandlerPromise(ayncResult, (resultStr) => {
					var resultJson = JSON.parse(resultStr);

					if (typeof callback === "function") {
							var ret = { code: resultJson.Code, msg: resultJson.ExceptionMsg }
							callback(ret);
					}
				});	
    },
    //测试是否连接上CefBrowser
    checkCefBrowserConnection: function () {
        if (window.NetBound == undefined || window.NetBound == null || !window.NetBound) {
            //判断cefBrowser是否已注册JsObject
            alert('window.NetBound does not exist.');
            console.log('window.NetBound does not exist.');
            return false;
        }
        return true;
    }
}