var aboutManage = {
    getInfo: function(param, callback) {
      var versionUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/esight/version?s=" + Math.random();
      //通过API获取版本
	    $.get(versionUrl, function(response){
	    	console.log(response);
	        if (typeof callback === "function") {
	        	var data = {
	                    versionNumber: response.data
	                };
	        	var ret = { code: '0', msg: '', data: data };
	            callback(ret);
	        }
	      },"json");
    }
}
