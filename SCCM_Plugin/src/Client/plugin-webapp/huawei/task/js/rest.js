var manager = {

    getList: function(queryParam, cb) {
        if (!manager.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(queryParam, "getTaskList", true, (resultStr) => {
					var result = JSON.parse(resultStr);

					console.log("deployTaskManage getTaskList result:");
					console.log(result);

					var ret = { code: '0', msg: '', data: { totalCount: result.size, data: result.data } }
					dealResult(ret, cb);
				});	
    },
    //添加任务
    create: function(postParam, cb) {
        if (!manager.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(postParam, "save", false, (resultStr) => {
					var result = JSON.parse(resultStr);

					console.log("deployTaskManage save result:");
					console.log(result);

					var ret = { code: result.code, msg: result.description, data: result.data }
					dealResult(ret, cb);
				});	
    },
    //删除
    delete: function(param, cb) {
        if (!manager.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "delete", true, (resultStr) => {
					var result = JSON.parse(resultStr);

					console.log("deployTaskManage delete result:");
					console.log(result);

					var ret = { code: result.code, msg: result.description, data: result.data }
					dealResult(ret, cb);
				});	
    },
    //获取模板列表
    getTemplateList: function(queryParam, cb) {
        if (!manager.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(queryParam, "getTemplateList", true, (resultStr) => {
					var result = JSON.parse(resultStr);

					console.log("deployTaskManage getTemplateList result:");
					console.log(result);

					var ret = { code: result.code, msg: result.description, data: { totalCount: result.size, data: result.data } }
					dealResult(ret, cb);
				});	
    },
    //获取服务器列表
    getServerList: function(queryParam, cb) {
        if (!manager.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(queryParam, "getServerList", false, (resultStr) => {
					var result = JSON.parse(resultStr);

					console.log("deployTaskManage getServerList result:");
					console.log(result);

					var ret = { code: result.code, msg: result.description, data: { totalCount: result.size, data: result.data } }
					dealResult(ret, cb);
				});	
    },
		//获取服务器详细信息
    getServerInfo: function(param, callback) {
        if (!manager.checkCefBrowserConnection()) return;
				
				ExecuteAnynsMethod(param, "getDeviceDetail", false, (resultStr) => {
					var result = JSON.parse(resultStr);

					console.log("deployTaskManage getDeviceDetail result:");
					console.log(result);

					callback(result);
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
};