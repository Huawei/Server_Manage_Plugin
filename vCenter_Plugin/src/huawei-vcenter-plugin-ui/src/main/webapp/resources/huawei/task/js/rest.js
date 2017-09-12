var manager = {

    getList: function(param, cb) {
    	//获取数据
        var TaskListUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/template/task/list?esightIp="+param.esight+
        "&taskName="+param.param.taskSourceName+"&taskStatus="+param.param.taskStatus+"&order="+param.param.order+"&orderDesc="+param.param.orderDesc+
        "&pageNo="+param.param.pageNo+"&pageSize="+param.param.pageSize+"&s=" + Math.random();
        //通过API获取数据
        $.get(TaskListUrl, function(response){
        	console.log(response);
            var totalCount = 0;
            var data = [];
            if (response.code === '0') {
            	totalCount = response.data.count;
            	data = response.data.data;
            }
            if (typeof cb === "function") {
                var ret = {code: response.code, msg: response.description,data: { totalCount: totalCount, data: data } };
                dealResult(ret, cb);
            }
          },"json");
    },
    //添加任务
    create: function(param, cb) {
        // console.log(postParam);
    	var addTaskUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/template/task?s=" + Math.random();
        $.ajax({  
            type: 'POST',
            contentType : 'application/json;charset=utf-8',
            url: addTaskUrl,  
            data: JSON.stringify(param),
            dataType: "json",
            success: function(response){  
            	if (typeof cb === "function") {
                    var ret = { code: response.code, msg: response.description, data: response.data }
                    dealResult(ret, cb);
                }
            }
        });
    },
    //删除
    delete: function(param, cb) {
        //请在删除操作的回调里得到ret并处理。
    	var deleteTaskUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/template/task/"+param.param.taskId+"?s=" + Math.random();
    	$.ajax({  
            type: 'DELETE',
            contentType : 'application/json;charset=utf-8',
            url: deleteTaskUrl,  
            dataType: "json",
            success: function(response){  
            	if (typeof cb === "function") {
                    var ret = {code: response.code, msg: response.description,data:response.data};
                    dealResult(ret, cb);
                }
            }
        });
    },
    //获取模板列表
    getTemplateList: function(param, cb) {
        var totalCount = 0;
        var esight = param.esight;
        var MaxSize = 99999;
        //获取数据
        var templateListUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/template/list?ip="+param.esight+"&pageNo=1&pageSize=" + MaxSize+"&s=" + Math.random();
        //通过API获取数据
        $.get(templateListUrl, function(response){
        	console.log(response);
            var totalCount = 0;
            var data = [];
            if (response.code === '0') {
            	totalCount = response.data.totalNum;
            	data = response.data.data;
            }
            if (typeof cb === "function") {
                var ret = {code: response.code, msg: response.description, data: { totalCount: totalCount, data: data } };
                dealResult(ret, cb);
            }
          },"json");
    },
    //获取服务器列表
    getServerList: function(param, cb) {
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
            if (typeof cb === "function") {
                var ret = {code: response.code, msg: response.description,data: { data: data, totalCount:totalCount } };
                dealResult(ret, cb);
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