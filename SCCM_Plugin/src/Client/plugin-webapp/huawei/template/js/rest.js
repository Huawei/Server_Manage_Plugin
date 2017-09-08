var templateManage = {
    //获取模板列表
    getList: function(param, callback) {
        if (!templateManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "getList", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);
					console.log("getList resultJson");
					console.log(resultJson);

					var listData = resultJson.data;
					if (typeof callback === "function") {
							var ret = { code: resultJson.code, msg: resultJson.description, data: listData, totalNum: resultJson.totalNum  }
							callback(ret);
					}
				});	
    },
    //删除模板
    delete: function(param, callback) {
        if (!templateManage.checkCefBrowserConnection()) return;

				var delParam = { "hostIP": param.esight, "templateName": param.templateName };
				ExecuteAnynsMethod(delParam, "delete", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);
        
					if (typeof callback === "function") {
							var ret = { code: resultJson.Code, msg: resultJson.ExceptionMsg }
							callback(ret);
					}
				});	
    },
    //添加上下电模板
    addOnePower: function (param, callback) {
        if (!templateManage.checkCefBrowserConnection()) return;

        var postData = {
						esights: param.esights,
						data: {
								templateName: param.templateName,
								templateType: "POWER",
								templateDesc: param.templateDesc,
								templateProp: { "powerPolicy": param.powerPolicy }
						}
				}
				//
				ExecuteAnynsMethod(postData, "saveTemplate", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);

					console.log("save power result:");
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
    //添加BIOS模版
    addBIOS: function(param, callback) {
        if (!templateManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "saveTemplate", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);

					console.log("save bios result:");
					console.log(resultJson);

					var result = {
							"code": resultJson.Code,
							"description": resultJson.ExceptionMsg,
							"data": resultJson.Data
					};
					var ret = { code: result.code, msg: result.description, data: result.data }
					dealResult(ret, callback);
				});	
    },
    //获取软件源列表
    getSoftwareList: function(param, callback) {
        if (!templateManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "getSoftwareList", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);

					console.log("getSoftwareList result:");
					console.log(resultJson);

					var listData = resultJson.data;
					var totalNum = resultJson.totalNum;
					if (typeof callback === "function") {
							var ret = { code: resultJson.code, msg: resultJson.description, data: { data: listData, totalNum: totalNum } }
							callback(ret);
					}
				});	
    },
    addOneOS: function (param, callback) {
        if (!templateManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "saveTemplate", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);

					console.log("save os result:");
					console.log(resultJson);

					var ret = { code: resultJson.Code, msg: resultJson.ExceptionMsg, data: resultJson.Data }
					dealResult(ret, callback);
				});	
    },
    //添加HBA模板
    addHBA: function(param, callback) {
        if (!templateManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "saveTemplate", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);

					console.log("save hba result:");
					console.log(resultJson);

					var result = {
							"code": resultJson.Code,
							"description": resultJson.ExceptionMsg,
							"data": resultJson.Data
					};
					var ret = { code: result.code, msg: result.description, data: result.data }
					dealResult(ret, callback);
				});	
    },
    addiBMC: function(param, callback) {
        if (!templateManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "saveTemplate", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);

					console.log("save bmc result:");
					console.log(resultJson);

					var result = {
							"code": resultJson.Code,
							"description": resultJson.ExceptionMsg,
							"data": resultJson.Data
					};
					var ret = { code: result.code, msg: result.description, data: result.data }
					dealResult(ret, callback);
				});	
    },
    //填充--型号--下拉框 请求数据
    // Lubin之前说，此处数据不确定是否从后台获取，所以写成这种结构，暂时不需要后台人员修改
    getModeList: function(param, callback) {

        var result = {
            "code": "0",
            "description": "",
            "data": [{
                label: 'OCE11102',
                value: 'OCE11102'
            }, {
                label: 'MZ510',
                value: 'MZ510'
            }, {
                label: 'MZ512',
                value: 'MZ512'
            }, {
                label: 'MZ910',
                value: 'MZ910'
            }]
        }
        var ret = { code: result.code, msg: result.description, data: result.data }
        dealResult(ret, callback);
    },
    //填充--槽位号--下拉框 请求数据
    // Lubin之前说，此处数据不确定是否从后台获取，所以写成这种结构，暂时不需要后台人员修改
    getSlotList: function(param, callback) {

        var result = {
            "code": "0",
            "description": "",
            "data": [{
                label: 'Slot1',
                value: '1'
            }, {
                label: 'Slot2',
                value: '2'
            }, {
                label: 'Slot3',
                value: '3'
            }, {
                label: 'Slot4',
                value: '4'
            }, {
                label: 'Slot5',
                value: '5'
            }, {
                label: 'Slot6',
                value: '6'
            }, {
                label: 'Slot7',
                value: '7'
            }, {
                label: 'Slot8',
                value: '8'
            }]
        }
        var ret = { code: result.code, msg: result.description, data: result.data }
        dealResult(ret, callback);
    },
    //点击确定 上传到服务器 CNA的配置
    postCNAdata: function(param, callback) {
        if (!templateManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "saveTemplate", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);

					console.log("save cna result:");
					console.log(resultJson);

					var result = {
							"code": resultJson.Code,
							"description": resultJson.ExceptionMsg,
							"data": resultJson.Data
					};
					var ret = { code: result.code, msg: result.description, data: result.data }
					dealResult(ret, callback);
				});	
    },
    addRAID: function(param, callback) {
        if (!templateManage.checkCefBrowserConnection()) return;

				ExecuteAnynsMethod(param, "saveTemplate", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);

					console.log("save raid result:");
					console.log(resultJson);

					var result = {
							"code": resultJson.Code,
							"description": resultJson.ExceptionMsg,
							"data": resultJson.Data
					};
					var ret = { code: result.code, msg: result.description, data: result.data }
					dealResult(ret, callback);
				});	
    },
		/**
     * 
     * 获取模板详情
     *  @param {any} param 
     *  @param {Function} callback 
     */
    getTemplateDetail: function(param, callback) {
        if (!templateManage.checkCefBrowserConnection()) return;
				
				ExecuteAnynsMethod(param, "getTemplateDetail", false, (resultStr) => {
					var resultJson = JSON.parse(resultStr);

					console.log("get template detail result:");
					console.log(resultJson);

					var ret = { code: resultJson.code, msg: resultJson.description, data: resultJson.data }
					dealResult(ret, callback);
				});	
    }
}