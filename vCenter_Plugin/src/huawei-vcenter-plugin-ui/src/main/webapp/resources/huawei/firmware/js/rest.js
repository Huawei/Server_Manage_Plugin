var firmwareManage = {
    getTaskList: function (param, callback) {
    	//获取数据
        var firmwareTaskUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/server/firmware/basepackages/progress/list?s=" + Math.random();;
        //通过API获取数据
        $.get(firmwareTaskUrl, function(response){
        	console.log(response);
            if (typeof callback === "function") {
                var ret = {code: response.code, msg: response.description, data: response.data}
                callback(ret);
            }
          },"json");

    },
    getFirmwareList: function (param, callback) {
        //获取数据
        var firmwareListUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/server/firmware/basepackages/list?esightIp="+param.esight+"&pageNo="+param.param.pageNo+"&pageSize="+param.param.pageSize+"&s=" + Math.random();
        //通过API获取数据
        $.get(firmwareListUrl, function(response){
        	console.log(response);
            var data = [];
            var totalNum = 0;
            if(response.code === '0'){
            	data = response.data.data;
            	totalNum = response.data.totalNum;
            }
            if (typeof callback === "function") {
                var ret = {code: response.code, msg: response.description, data: data,totalNum: totalNum}
                callback(ret);
            }
          },"json");
    },
    getFirmwareDetails: function (param, callback) {
    	
    	 //获取数据
        var firmwareDetailUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/server/firmware/basepackages/detail?esightIp="+param.esight+"&basepackageName="+param.param.basepackageName+"&s=" + Math.random();
        //通过API获取数据
        $.get(firmwareDetailUrl, function(response){
        	console.log(response);
            var data = [];
            if(response.code === '0'){
            	data = response.data.data;
            }
            if (typeof callback === "function") {
                var ret = {code: response.code, msg: response.description, data: data}
                callback(ret);
            }
          },"json");
    },
    addFirmware: function (param, callback, failCallback) {
        var postData = {
            esights: param.esights,//['192.168.1.1','192.168.1.2']
            data: {
                basepackageName: param.basepackageName,
                basepackageDescription: param.basepackageDescription,
                basepackageType: param.basepackageType,
                fileList: param.fileList,
                sftpserverIP: param.sftpserverIP,
                port: param.port,
                username: param.username,
                password: param.password
            }
        }

        var firmwareSaveUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/server/firmware/basepackages/upload?s=" + Math.random();
        $.ajax({  
            type: 'POST',
            contentType : 'application/json;charset=utf-8',
            url: firmwareSaveUrl,  
            data: JSON.stringify(postData),
            dataType: "json",
            success: function(response){  
            	if (typeof callback === "function") {
                    var ret = {code: response.code, msg: response.description,data: response.data}
                    dealResult(ret, callback, failCallback);
                }
            }
        });
    },
    deleteFailTask: function (param, callback) {
        //删除失败任务逻辑
    	var deleteFailTaskUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/server/firmware/basepackages?s=" + Math.random();
    	$.ajax({  
            type: 'DELETE',
            contentType : 'application/json;charset=utf-8',
            url: deleteFailTaskUrl,  
            dataType: "json",
            success: function(response){  
            	if (typeof callback === "function") {
                    var ret = {code: response.code, msg: response.description,data: response.data};
                    dealResult(ret, callback);
                }
            }
        });
    },
    deleteFirmware: function(param, callback) {
        //删除失败逻辑
    	var deleteFailTaskUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/server/firmware/basepackages/"+param.param.basepackageName+"?esightIp="+param.esight+"&s=" + Math.random();
    	$.ajax({  
            type: 'DELETE',
            contentType : 'application/json;charset=utf-8',
            url: deleteFailTaskUrl,  
            dataType: "json",
            success: function(response){  
            	if (typeof callback === "function") {
                    var ret = {code: response.code, msg: response.description,data: response.data};
                    dealResult(ret, callback);
                }
            }
        });
    }
};

//升级任务管理
var firmwareTaskManage = {
    //获取升级任务列表
    getTaskList: function(param, callback) {
      //获取数据
        var firmwareTaskUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/server/firmware/basepackages/upgrade/task/list?esightIp="+param.esight+
        "&taskName="+param.param.taskName+"&taskStatus="+param.param.taskStatus+"&order="+param.param.order+"&orderDesc="+param.param.orderDesc+
        "&pageNo="+param.param.pageNo+"&pageSize="+param.param.pageSize+"&s=" + Math.random();
        //通过API获取数据
        $.get(firmwareTaskUrl, function(response){
        	console.log(response);
            var totalCount = 0;
            var data = [];
            if (response.code === '0') {
            	totalCount = response.data.count;
            	data = response.data.data;
            }
            if (typeof callback === "function") {
                var ret = {code: response.code, msg: response.description,data: { totalNum: totalCount, data: data } };
                dealResult(ret, callback);
            }
          },"json");
    },
    //删除升级任务
    deleteFirmwareTask: function(param, callback) {
    	var deleteTaskUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/server/firmware/basepackages/task/"+param.param.taskId+"?s=" + Math.random();
    	$.ajax({  
            type: 'DELETE',
            contentType : 'application/json;charset=utf-8',
            url: deleteTaskUrl,  
            dataType: "json",
            success: function(response){  
            	console.log(response);
                if (typeof callback === "function") {
                    var ret = {code: response.code, msg: response.description,data:response.data};
                    dealResult(ret, callback);
                }
            }
        });
    },
    //获取服务器列表
    getServerList: function(param, callback) {
      //获取数据
        var keyword = param.param.servertype;
        var pageIndex = param.param.start;
        var size = param.param.size;
        
        var serverListUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/server/list?ip="+param.esight+"&servertype="+keyword+"&pageNo="+pageIndex+"&pageSize="+size+"&s=" + Math.random();
        //通过API获取数据
        $.get(serverListUrl, function(response){
        	console.log(response);
            var totalCount = 0;
            var data = [];
            if (response.code === '0') {
            	totalCount = response.data.size;
            	data = response.data.data;
            }
            if (typeof callback === "function") {
                var ret = {code: response.code, msg: response.description,data: { data: data, totalNum:totalCount } };
                dealResult(ret, callback);
            }
          },"json");

    },
    //添加升级包任务
    addFirmwareTask: function(param, callback) {
    	var addFirmwareTaskUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/server/firmware/basepackages/upgrade/task?s=" + Math.random();
        $.ajax({  
            type: 'POST',
            contentType : 'application/json;charset=utf-8',
            url: addFirmwareTaskUrl,  
            data: JSON.stringify(param),
            dataType: "json",
            success: function(response){  
            	if (typeof callback === "function") {
                    var ret = { code: response.code, msg: response.description, data: response.data }
                    dealResult(ret, callback);
                }
            }
        });
    },
    /**
     * 查询指定设备的升级详情
     * 
     * @param {any} param 
     * @param {function} callback 
     */
    getDeviceTaskInfo: function(param, callback) {
    	var firmwareTaskDeviceDetailUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/server/firmware/basepackages/device/detail?esightIp="+param.esight+"&taskName="+param.param.taskName+"&dn="+param.param.dn+"&s=" + Math.random();
    	$.get(firmwareTaskDeviceDetailUrl, function(response){
        	console.log(response);
            var data = [];
            if (response.code === '0') {
            	data = response.data.data;
            	for(var i=0;i<data.firmwarelist.length;i++){
            		if(data.firmwarelist[i].details!=''){
            		    data.firmwarelist[i].details="firmware.error."+data.firmwarelist[i].details;
            		}
            	}
            }
            if (typeof callback === "function") {
                var ret = {code: response.code, msg: response.description, data: { data: data }};
                callback(ret);
            }
          },"json");
    },
    getServerInfo: function(param, callback) {
        var serverDeviceDetailUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/server/device/detail?ip="+param.ip+"&dn="+param.dn+"&s=" + Math.random();
        //通过API获取数据
        $.get(serverDeviceDetailUrl, function(response){
        	console.log(response);
            if (typeof callback === "function") {
            	callback(response);
            }
          },"json");
    }
};