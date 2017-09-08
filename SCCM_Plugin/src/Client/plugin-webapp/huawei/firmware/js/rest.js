//升级包管理
var firmwareManage = {
    getTaskList: function(param, callback) {
        if (!firmwareManage.checkCefBrowserConnection()) return;

        //调用C#方法获取数据
				ExecuteAnynsMethod(param, "getList", true, (resultStr) => {
					var resultJson = JSON.parse(resultStr);
					console.log("getTaskList result:");
					console.log(resultJson);
					var listData = [];
					listData = resultJson.Data;

					if (typeof callback === "function") {
							var ret = { code: resultJson.Code, msg: resultJson.ExceptionMsg, data: listData }
							callback(ret);
					}
				});	
    },
    getFirmwareList: function(param, callback) {
        if (!firmwareManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "getFirmwareList", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);
					console.log("getFirmwareList result:");
					console.log(resultJson);
					var listData = resultJson.data;

					if (typeof callback === "function") {
							var ret = { code: resultJson.code, msg: resultJson.description, data: listData, totalNum: resultJson.totalNum }
							callback(ret);
					}
				});
    },
    getFirmwareDetails: function(param, callback) {
        if (!firmwareManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "getFirmwareDetails", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);
					console.log("getFirmwareDetails result:");
					console.log(resultJson);
					var listData = resultJson.Data;

					if (typeof callback === "function") {
							var ret = { code: resultJson.Code, msg: resultJson.ExceptionMsg, data: listData }
							callback(ret);
					}
				});	
    },
    addFirmware: function(param, callback, failCallback) {
        if (!firmwareManage.checkCefBrowserConnection()) return;

        var postData = {
            esights: param.esights, 
            data: {
                basepackageName: param.basepackageName,
                basepackageDescription: param.basepackageDescription,
                basepackageType: param.basepackageType,
                fileList: param.fileList,
                sftpserverIP: param.sftpserverIP,
                username: param.username,
                password: param.password,
                port: param.port
            }
        }
        //
				ExecuteAnynsMethod(postData, "save", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);
					console.log("addFirmware result:");
					console.log(resultJson);

					var ret = { code: resultJson.Code, msg: resultJson.ExceptionMsg, data: resultJson.Data }
					dealResult(ret, callback, failCallback);
				});	
    },
    deleteFailTask: function(param, callback) {
        if (!firmwareManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "delete", true, (resultStr) => {
					var resultJson = JSON.parse(resultStr);
					console.log("delete result: ");
					console.log(resultJson);

					var ret = { code: resultJson.Code, msg: resultJson.ExceptionMsg, data: resultJson.Data }
					dealResult(ret, callback);
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
    },
    deleteFirmware: function(param, callback) {
        if (!firmwareManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "deleteBasepackage", true, (resultStr) => {
					var resultJson = JSON.parse(resultStr);
					console.log("deleteBasepackage result: ");
					console.log(resultJson);

					var ret = { code: resultJson.code, msg: resultJson.description, data: resultJson.data }
					dealResult(ret, callback);
				});
    }
};

//升级任务管理
var firmwareTaskManage = {
    //获取升级任务列表
    getTaskList: function(param, callback) {
        if (!firmwareTaskManage.checkCefBrowserConnection()) return;
        
				ExecuteAnynsMethod(param, "getTaskList", false, (resultStr) => {
					var result = JSON.parse(resultStr);

					console.log("firmwareTaskManage getTaskList result:");
					console.log(result);
					
					var ret = { code: result.code, msg: result.description, data: { data: result.data, totalNum: result.size } }
					dealResult(ret, callback);
				});	
    },
    //删除升级任务
    deleteFirmwareTask: function(param, callback) {
        if (!firmwareTaskManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "delete", true, (resultStr) => {
					var result = JSON.parse(resultStr);

					console.log("firmwareTaskManage deleteFirmwareTask result:");
					console.log(result);

					var ret = { code: result.code, msg: result.description, data: result.data }
					dealResult(ret, callback);
				});	
    },
    //获取服务器列表
    getServerList: function(param, cb) {
        if (!firmwareTaskManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "getServerList", false, (resultStr) => {
					var result = JSON.parse(resultStr);

					console.log("firmwareTaskManage getServerList result:");
					console.log(result);

					var ret = { code: result.code, msg: result.description, data: { data: result.data, totalNum: result.size } }
					dealResult(ret, cb);
				});	
    },
    //添加升级包任务
    addFirmwareTask: function(param, callback) {
        if (!firmwareTaskManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "save", false, (resultStr) => {
					var result = JSON.parse(resultStr);

					console.log("firmwareTaskManage addFirmwareTask result:");
					console.log(result);

					var ret = { code: result.code, msg: result.description, data: result.data }
					dealResult(ret, callback);
				});	
    },
		/**
     * 查询指定设备的升级详情
     * 
     * @param {any} param 
     * @param {function} callback 
     */
    getDeviceTaskInfo: function(param, callback) {
        if (!firmwareTaskManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "getTaskDetail", false, (resultStr) => {
					var result = JSON.parse(resultStr);

					console.log("firmwareTaskManage getTaskDetail result:");
					console.log(result);

					var ret = { code: result.code, msg: result.description, data: { data: result.data } }
					//dealResult(ret, callback);
					callback(ret);
				});	
    },
		//获取服务器详细信息
		getServerInfo: function(param, callback) {
        if (!firmwareTaskManage.checkCefBrowserConnection()) return;
				
				ExecuteAnynsMethod(param, "getDeviceDetail", false, (resultStr) => {
					var result = JSON.parse(resultStr);

					console.log("firmwareTaskManage getDeviceDetail result:");
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