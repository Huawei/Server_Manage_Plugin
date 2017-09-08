var serverListManager = {
    getList: function (param, callback) {
        if (!serverListManager.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "getList", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);
					console.log("getList result:");
					console.log(resultJson);
					
					var totalCount = resultJson.size;
					var listData = resultJson.data;
					if (typeof callback === "function") {
							var ret = { code: resultJson.code, msg: resultJson.description, data: { totalCount, data: listData } }
							callback(ret);
					}
				});	
    },
    getUnits: function(param, callback) {
        if (!serverListManager.checkCefBrowserConnection()) return;
				
				ExecuteAnynsMethod(param, "getDeviceDetail", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);
					console.log("getDeviceDetail result:");
					console.log(resultJson);
					
					dealResult(resultJson, callback);
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