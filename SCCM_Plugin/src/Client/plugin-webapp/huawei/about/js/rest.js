var aboutManage = {
    //获取模板列表
    getInfo: function (param, callback) {
        if (!aboutManage.checkCefBrowserConnection()) return;

        //调用C#方法获取数据
				ExecuteAnynsMethod(param, "getVersion", true, (resultStr) => {
					var resultJson = JSON.parse(resultStr);
					console.log("getVersion result:");
					console.log(resultJson);
					var data = {
							versionNumber: resultJson.Data
					};
					if (typeof callback === "function") {
							var ret = { code: resultJson.Code, msg: resultJson.ExceptionMsg, data: data }
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
