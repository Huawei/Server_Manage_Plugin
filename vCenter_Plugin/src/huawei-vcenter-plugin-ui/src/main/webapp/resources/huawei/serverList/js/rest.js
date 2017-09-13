var serverListManager = {
    getList: function(param, callback) {
        //var esight = localStorage.getItem("esight")
        //通过API获取数据
        var listData = [];
        var totalCount = 0;
        var esight = param.esight;

        var keyword = param.param.servertype;
        var pageIndex = param.param.start;
        var size = param.param.size;
        //获取数据
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
                var ret = {code: response.code, msg: response.description,data: { data: data, totalCount:totalCount } };
                dealResult(ret, callback);
            }
          },"json");
        
    },
    getUnits: function(param, callback) {
        var serverDeviceDetailUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/server/device/detail?ip="+param.ip+"&dn="+param.dn+"&s=" + Math.random();
        //通过API获取数据
        $.get(serverDeviceDetailUrl, function(response){
        	console.log(response);
            if (typeof callback === "function") {
                dealResult(response, callback);
            }
          },"json");
    }
}