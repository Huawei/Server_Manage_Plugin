var deviceInfo = {
    getUnits: function(param, callback) {
        var serverDeviceDetailUrl = com_huawei_vcenterpluginui.webContextPath + "/rest/services/server/device/detail?ip="+param.ip+"&dn="+param.dn+"&s=" + Math.random();
        //通过API获取数据
        $.get(serverDeviceDetailUrl, function(response){
        	console.log(response);
            if (typeof callback === "function") {
            	callback(response);
            }
          },"json");
    }
}