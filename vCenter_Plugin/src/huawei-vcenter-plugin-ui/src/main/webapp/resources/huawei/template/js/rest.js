var templateManage = {
    //获取模板列表
    getList: function (param, callback) {
    	 var listData = [];
         var totalCount = 0;
         var esight = param.esight;
         //获取数据
         var serverListUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/template/list?ip="+param.esight +"&templateType="+param.param.templateType+"&pageNo="+param.param.pageNo+"&pageSize="+param.param.pageSize+"&s=" + Math.random();
         //通过API获取数据
         $.get(serverListUrl, function(response){
        	 console.log(response);
             var data = [];
             var totalNum = 0;
             if (response.code === '0') {
             	data = response.data.data;
             	totalNum = response.data.totalNum;
             }
             if (typeof callback === "function") {
                 var ret = {code: response.code, msg: response.description,data:data, totalNum:totalNum };
                 dealResult(ret, callback);
             }
           },"json");

    },
    //删除模板
    delete: function (param, callback) {
        //根据templateName和esight删除 param.templateName param.esight
    	var deleteTemplateUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/template/"+param.templateName+"?ip="+param.esight+"&s=" + Math.random();
    	$.ajax({  
            type: 'DELETE',
            contentType : 'application/json;charset=utf-8',
            url: deleteTemplateUrl,  
            dataType: "json",
            success: function(response){  
            	console.log(response);
                if (typeof callback === "function") {
                    var ret = {code: response.code, msg: response.description, data: response.data};
                    dealResult(ret, callback);
                }
            }
        });

    },
    //添加上下电模板
    addOnePower: function (param, callback) {
        var postData = {
            esights: param.esights,//['192.168.1.1','192.168.1.2']
            data: {
                templateName: param.templateName,
                templateType: "POWER",
                templateDesc: param.templateDesc,
                templateProp: { "powerPolicy": param.powerPolicy }
            }
        }

        var templateUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/template?s=" + Math.random();
        $.ajax({  
            type: 'POST',
            contentType : 'application/json;charset=utf-8',
            url: templateUrl,  
            data: JSON.stringify(postData),
            dataType: "json",
            success: function(response){  
            	if (typeof callback === "function") {
                    var ret = { code: response.code, msg: response.description, data: response.data }
                    dealResult(ret, callback);
                }
            }
        });
    },

    //添加BIOS模版
    addBIOS: function (param, callback) {
        var templateUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/template?s=" + Math.random();
        $.ajax({  
            type: 'POST',
            contentType : 'application/json;charset=utf-8',
            url: templateUrl,  
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

    //获取软件源列表
    getSoftwareList: function (param, callback) {
        var softwareListUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/software/list?ip="+param.esight+"&pageNo="+param.param.pageNo+"&pageSize="+param.param.pageSize+"&s=" + Math.random();
        //通过API获取数据
        $.get(softwareListUrl, function(response){
        	console.log(response);
            var totalCount = 0;
            var data = [];
            if (response.code === '0') {
            	totalCount = response.data.totalNum;
            	data = response.data.data;
            }
            if (typeof callback === "function") {
                var ret = {code: response.code, msg: response.description,data: { data: data, totalNum:totalCount } };
                dealResult(ret, callback);
            }
          },"json");

    },
    addOneOS: function (param, callback) {

        var templateUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/template?s=" + Math.random();
        $.ajax({  
            type: 'POST',
            contentType : 'application/json;charset=utf-8',
            url: templateUrl,  
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
    //添加HBA模板
    addHBA: function(param, callback) {

    	var templateUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/template?s=" + Math.random();
    	$.ajax({  
            type: 'POST',
            contentType : 'application/json;charset=utf-8',
            url: templateUrl,  
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
    	var templateUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/template?s=" + Math.random();
    	$.ajax({  
            type: 'POST',
            contentType : 'application/json;charset=utf-8',
            url: templateUrl,  
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
    addiBMC: function(param, callback) {
    	var templateUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/template?s=" + Math.random();
    	$.ajax({  
            type: 'POST',
            contentType : 'application/json;charset=utf-8',
            url: templateUrl,  
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
    addRAID: function(param, callback) {
    	var templateUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/template?s=" + Math.random();
    	$.ajax({  
            type: 'POST',
            contentType : 'application/json;charset=utf-8',
            url: templateUrl,  
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
     * 
     * 获取模板详情
     *  @param {any} param 
     *  @param {Function} callback 
     */
    getTemplateDetail: function(param, callback) {
    	var firmwareTaskDeviceDetailUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/template/"+param.param.templateName+"?ip="+param.esight+"&s=" + Math.random();
    	$.get(firmwareTaskDeviceDetailUrl, function(response){
        	console.log(response);
            var data = '';
            if (response.code === '0') {
            	data = response.data.data;
            }
            if (typeof callback === "function") {
                var ret = {code: response.code, msg: response.description, data:  data };
                dealResult(ret, callback);
            }
          },"json");
    }
}