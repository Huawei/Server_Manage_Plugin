var http = require('http');  
var https = require('https'); 
var url = require('url'); 
var qs = require("querystring");

const fs = require('fs');

const options = {
        key: fs.readFileSync('./privatekey.pem'),
        cert: fs.readFileSync('./certification.pem')
};

// 访问的json地址与返回的json数据映射关系
var loginErrJson='{"description":"Auth failed","data":null,"code":1024}';
var array=[
  {
  url:'/rest/openapi/sm/session',
  openid:false,
   json:'{"code":0,"data":"bfec0163-dd56-473e-b11f-6d5845e1b684", "description":"操作成功Operation success."}'   
  },
  {
  url:'/rest/openapi/server/device',
  openid:true,
  json:'{"code":0,"data":[{"dn":"NE=34603409", "ipAddress":"10.137.62.207","serverName":"E6000-10.137.62.207", "serverModel":"E6000","manufacturer":"Huawei Technologies Co., Ltd.","status":-3, "childBlades":[{"ipAddress":"10.137.62.208", "dn":"NE=34603411"}, {"ipAddress":"10.137.62.208", "dn":"NE=34603412"}]}],"size":1,"totalPage":1, "description":"Succeeded in querying the server list."}'
  //json:'{"description":"Auth failed","data":null,"code":1024}'
  },
  {
  url:'/rest/openapi/server/device/detail',
  openid:true,
  json:'{"code":0,"data":[{"dn":"NE=34603409","ipAddress":"xx.xx.xx.xx", "name":"E6000-xx.xx.xx.xx","type":"E6000","status":-3,"desc":"", "PSU":[{"name":"PSU1","healthState":-3,"inputPower":"0 W"},{"name":"PSU2","healthState":0,"inputPower":"0 W"},{"name":"PSU3","healthState":-1,"inputPower":"0 W"},{"name":"PSU4","healthState":-3,"inputPower":"0 W"},{"name":"PSU5","healthState":0,"inputPower":"0 W"},{"name":"PSU6","healthState":-1,"inputPower":"0 W"}],"Fan":[{"name":"Fan1 Front","healthState":0,"rotate":"3600.000 RPM","rotatePercent":"30"},{"name":"Fan1 Rear","healthState":0,"rotate":"3360.000 RPM","rotatePercent":"30"}, {"name":"Fan2 Front","healthState":0,"rotate":"3600.000 RPM","rotatePercent":"30"},{"name":"Fan2 Rear","healthState":0,"rotate":"3360.000 RPM","rotatePercent":"30"}, {"name":"Fan3 Front","healthState":0,"rotate":"4320.000 RPM","rotatePercent":"35"},{"name":"Fan3 Rear","healthState":0,"rotate":"3960.000 RPM","rotatePercent":"35"}, {"name":"Fan4 Front","healthState":0,"rotate":"3600.000 RPM","rotatePercent":"30"},{"name":"Fan4 Rear","healthState":0,"rotate":"3360.000 RPM","rotatePercent":"30"}, {"name":"Fan5 Front","healthState":0,"rotate":"3600.000 RPM","rotatePercent":"30"},{"name":"Fan5 Rear","healthState":0,"rotate":"3360.000 RPM","rotatePercent":"30"}, {"name":"Fan6 Front","healthState":0,"rotate":"4320.000 RPM","rotatePercent":"35"},{"name":"Fan6 Rear","healthState":0,"rotate":"3720.000 RPM","rotatePercent":"35"}, {"name":"Fan7 Front","healthState":0,"rotate":"3840.000 RPM","rotatePercent":"30"},{"name":"Fan7 Rear","healthState":0,"rotate":"3480.000 RPM","rotatePercent":"30"}, {"name":"Fan8 Front","healthState":0,"rotate":"3720.000 RPM","rotatePercent":"30"},{"name":"Fan8 Rear","healthState":0,"rotate":"3360.000 RPM","rotatePercent":"30"}, {"name":"Fan9 Front","healthState":0,"rotate":"4200.000 RPM","rotatePercent":"35"},{"name":"Fan9 Rear","healthState":0,"rotate":"3960.000 RPM","rotatePercent":"35"}]}],"description":"Succeeded in querying server details."}'
  },
  {
  url:'/rest/openapi/server/deploy/softwaresource/upload',
  openid:true,
  json:'{"code" : 0, "data":{"taskName":"API@Task_1456209500919"},"description" : "Start to upload softwaresource success."}'  
  }
  ,{
  url:'/rest/openapi/server/deploy/softwaresource/progress',
  openid:true,
  json:'{"code":0,"data":{"taskName":"API@Task_1456209500919","softwaresourceName":"OS_Software1","taskStatus":"Complete","taskProgress":"100","taskResult":"Success","taskCode":0,"errorDetail":"软件源上传失败，传输中断"},"description":"Gettaskdetailsuccess."}'
  }
  ,{
  url:'/rest/openapi/server/deploy/softwaresource/delete',
  openid:true,
  json:'{"code":0, "description":"Delete softwaresource success."}'
   }
  ,{
  url:'/rest/openapi/server/deploy/software/list',
  openid:true,
  json:'{"code":0,"totalNum":3,"data":[{"softwareName":"OS_Softwaresource1","softwareDescription":"ThisisSoftwaresourcetemplate1","softwareType":"Windows"},{"softwareName":"OS_Softwaresource2","softwareDescription":"ThisisSoftwaresourcetemplate1","softwareType":"Windows"},{"softwareName":"OS_Softwaresource3","softwareDescription":"ThisisSoftwaresourcetemplate1","softwareType":"Windows"}],"description":"Getsoftwaresourcelistsuccess."}'
  }
  ,{
	url:'/rest/openapi/server/deploy/template',
  openid:true,
json:'{"code":0,"description":"Createtemplatesuccess."}'	  
  }
  ,{
	url:'/rest/openapi/server/deploy/template/list',
  openid:true,
json:'{"code":0,"totalNum":3,"data":[{"templateName":"OS_Template1","templateType":"OS","templateDesc":"ThisisOStemplate1."},{"templateName":"OS_Template2","templateType":"OS","templateDesc":"ThisisOStemplate2."},{"templateName":"OS_Template3","templateType":"OS","templateDesc":"ThisisOStemplate3."}],"description":"Gettemplatelistsuccess."}'	  
  }
  ,{
	url:'/rest/openapi/server/deploy/task',
  openid:true,
json:'{    "code": 0,    "data": {        "taskName": "API@Task_1456209500919"    },    "resverd": null,    "description": "create task success."}'	  
  }
   ,{
	url:'/rest/openapi/server/deploy/template/del',
  openid:true,
json:'{"code":0,"description":"Delete template success."}'	  
  }
  ,{
	url:'/rest/openapi/server/deploy/task/detail',
  openid:true,
json:'{\"code\":0,\"data\":{\"taskName\":\"API@DeployTask_1497943969620\",\"templates\":\"powertest2\",\"deviceDetails\":[{\"dn\":\"NE=34603009\",\"deviceProgress\":\"\",\"deviceResult\":\"\",\"errorDetail\":\"\"}],\"taskStatus\":\"Idel\",\"taskResult\":\"\",\"taskCode\":\"\"},\"resverd\":null,\"description\":\"get task detail success.\"}'	  
  }
    ,{
	url:'/rest/openapi/server/firmware/basepackages/upload',
  openid:true,
	json:'{"code" : 0, "data":{        "taskName":"API@Task_1456209500919"},"description" : "Upload basepacakge success."}'	  
  }
  ,{
	url:'/rest/openapi/server/firmware/basepackages/progress',
  openid:true,
	json:'{"code":0,"data":{"taskName":"API@Task_ 1456209500919","basepackageName":"basePackage1","taskStatus":"Complete","taskProgress":"100","taskResult":"Success","taskCode":0,"errorDetail":"升级文件上传失败，传输中断"},"description":"Get task detail success."}'	  
  }
  ,{
	url:'/rest/openapi/server/firmware/basepackages/list',
  openid:true,
	json:'{"code":0,"totalNum":3,"data":[{"basepackageName":"basePackage1","basepackageDesc":"This is basepackage1.","basepackageType":"Firmware"},{"basepackageName":"basePackage2","basepackageDesc":"This is basepackage2.","basepackageType":"Driver"},{"basepackageName":"basePackage3","basepackageDesc":"This is basepackage3.","basepackageType":"Bundle"}],"description":"Get basePackage list success."}'	  
  }  
  ,{
	url:'/rest/openapi/server/firmware/basepackage/detail',
  openid:true,
	json:'{"code":0,"data":{"basepackageName":"basepackage1","basepackageType":"Base","basepackageDesc":"This is basepackage1.","basepackageProp":[{"driverPackageName":"onboard_driver_win2k8r2sp1.iso","supportOS":"ubuntu14.04","releaseDate":" 2017-2-16 ","driverPackageProp":[{"firmwareType":"RAID","version":"2.00.76.00","supportModel":"MZ111;SM210;MZ110"},{"firmwareType":"CNA","version":"4.0.3","supportModel":"MZ510;MZ512"}]},{"firmwarePackageName":"RH2288 V2-BIOS-V516.zip","supportDevice":"RH2288 V2","releaseDate":" 2017-2-16 ","firmwarePackageProp":[{"firmwareType":"iBMC","version":"2.30","activeMode":" ResetBMC ","supportModel":"XH628 V3"}]}]},"description":"Get template detail success."}'	  
  }
  
  ,{
	url:'/rest/openapi/server/firmware/task',
  openid:true,
	json:'{"code" : 0, "data":{        "taskName":"API@Task_1456209500919"},"description" : "Create task success."}'	  
  }
  ,{
	url:'/rest/openapi/server/firmware/taskdetail',
  openid:true,
	json:'{"code":0,"data":{"taskName":"API@Task_1456209500919","dn":"NE=12345678;NE=87654321","taskStatus":"Complete","taskProgress":50,"taskResult":"Success"},"description":""}'	  
  }
	,{
	url:'/rest/openapi/server/firmware/taskdevicedetail',
  openid:true,
	json:'{"code":0,"data":{"taskName":"API@FirmWareTask_1503898934595","dn":"NE=34603009","deviceTaskStatus":"Complete","deviceTaskProgress":100,"deviceTaskResult":"Failed","firmwarelist":[{"firmwareType":"BIOS","firmwareProgress":100,"result":"Failed","upgradeVersion":"3.66","currentVersion":"3.66","details":"1211","errorDetail":"Upgrade failed, the current device can not support the BIOS upgrade when the it is powered on."}]},"description":"Get task device detail success."}'	  
  }
];


// var temResult='{"TotalHits":88,"errorCode":0,"errorDesc":"no_error","MatchResults":[{"CatId":"4098","CatTitlePath":"%E5%86%85%E9%83%A8%E7%9F%A5%E8%AF%86%E5%BA%93%3E%E8%B4%A6%E6%88%B7%E5%9F%BA%E7%A1%80%E5%8F%8A%E8%B5%84%E4%BA%A7%E7%AE%A1%E7%90%86%EF%BC%88%E6%96%B0%EF%BC%89%3E%E8%B4%A6%E6%88%B7%E5%9F%BA%E7%A1%80%3E%E5%AF%86%E7%A0%81%3E%E6%89%8B%E5%8A%BF%E5%AF%86%E7%A0%81","ChannelNames":"","Content":"%e7%87%95%e5%ad%90","CreatorName":"%E6%A1%83%E7%98%B4","GmtCreate":"2015-04-07 17:58:42","GmtModified":"2015-05-19 11:48:36","Id":"6056","Keywords":"","ModifierId":"12484","ModifierName":"%E7%89%A7%E6%9A%AE","Status":"PUBLISHED","Title":"%E9%80%9A%E8%BF%87%E6%94%AF%E4%BB%98%E5%AE%9D%E9%92%B1%E5%8C%85%EF%BC%8C%3Cfont+color%3Dred%3E%E5%BF%98%E8%AE%B0%3C%2Ffont%3E%E6%89%8B%E5%8A%BF%3Cfont+color%3Dred%3E%E5%AF%86%E7%A0%81%3C%2Ffont%3E%E7%9A%84%E5%A4%84%E7%90%86%E6%B5%81%E7%A8%8B","Type":"NORMAL","deleted":"N"},{"CatId":"4098","CatTitlePath":"%E5%86%85%E9%83%A8%E7%9F%A5%E8%AF%86%E5%BA%93%3E%E8%B4%A6%E6%88%B7%E5%9F%BA%E7%A1%80%E5%8F%8A%E8%B5%84%E4%BA%A7%E7%AE%A1%E7%90%86%EF%BC%88%E6%96%B0%EF%BC%89%3E%E8%B4%A6%E6%88%B7%E5%9F%BA%E7%A1%80%3E%E5%AF%86%E7%A0%81%3E%E6%89%8B%E5%8A%BF%E5%AF%86%E7%A0%81","ChannelNames":"","Content":"%e7%87%95%e7%aa%9d","CreatorName":"%E6%A1%83%E7%98%B4","GmtCreate":"2015-04-07 17:58:42","GmtModified":"2015-05-19 11:48:36","Id":"6056","Keywords":"","ModifierId":"12484","ModifierName":"%E7%89%A7%E6%9A%AE","Status":"PUBLISHED","Title":"%E9%80%9A%E8%BF%87%E6%94%AF%E4%BB%98%E5%AE%9D%E9%92%B1%E5%8C%85%EF%BC%8C%3Cfont+color%3Dred%3E%E5%BF%98%E8%AE%B0%3C%2Ffont%3E%E6%89%8B%E5%8A%BF%3Cfont+color%3Dred%3E%E5%AF%86%E7%A0%81%3C%2Ffont%3E%E7%9A%84%E5%A4%84%E7%90%86%E6%B5%81%E7%A8%8B","Type":"NORMAL","deleted":"N"}]}';
https.createServer(options,function(request, response){  

     

    var reqURL=request.url;
    var newResult = [];
    var returnRes = {};
    var par = url.parse(reqURL, true).query;
    var pathname = url.parse(request.url).pathname;;
    console.log("========"+reqURL+"=============");
	var currentData = "";
	  request.on("data",function(data){
		//打印
		currentData += data;
		console.log(qs.parse(currentData));
	});
	console.log("=============End "+reqURL+"====================");
    var result="{}";

    var i=array.length;
	var isFind=false;
	//第一次查找，带参数匹配。
	while(i--){	
	if(reqURL==array[i].url){
	
		var openid = request.headers['openid']; 
	console.log(openid);
	if((openid=="" || openid==undefined) && array[i].openid)
		result=loginErrJson;
	else
		result= array[i].json;
	isFind=true;
    }    
    }
	i=array.length;
	//第二次查找，不带参数匹配
    while(!isFind && i--){
	reqURL = reqURL.split('?')[0];	
	if(array[i].param!=undefined && array[i].param!=""){
		
	}
    //if(reqURL.indexOf(array[i].url)==0){
	if(reqURL==array[i].url){
		var openid = request.headers['openid']; 
	console.log(openid);
	if((openid=="" || openid==undefined) && array[i].openid)
		result=loginErrJson;
	else
		result= array[i].json;
	
  // console.log(result);
   /*if(pathname == "/rest/openapi/server/device/detail" && typeof par.dn != "undefined")
   {
    if(JSON.parse(result).data != null && (typeof JSON.parse(result).data != "string"))
      {
          JSON.parse(result).data.forEach(function(element,index) {
                  if(element.dn == par.dn)
                  {
                    newResult.push(element);
                  }
              });
      }
       returnRes.data = newResult;
   }
   else
   {
     returnRes.data = JSON.parse(result).data;
   }
   returnRes.code = JSON.parse(result).code;
    returnRes.description = JSON.parse(result).description;
   */
    
	 // console.log(result);
	 
    }  
   
    }
 returnRes=JSON.parse(result);
    console.log(returnRes);
   // var params = url.parse(request.url, true).query;  

   // console.log(params); 
	if(JSON.stringify(returnRes)!="{}"){
		response.writeHead(200,{
    //"Access-Control-Allow-Origin":"http://10.37.187.79:8000",
    "Access-Control-Allow-Credentials": "true",
    "Access-Control-Allow-Headers":"X-Requested-With",
    "Access-Control-Allow-Methods":"PUT,POST,GET,DELETE,OPTIONS",
    "X-Powered-By":"3.2.1",
    "Content-Type":"application/json;charset=utf-8",
    "Connection":"keep-alive"
    }); 
		response.write(JSON.stringify(returnRes));  
        response.end();  
	}
	else{
		console.log("No request handler found.");
		response.writeHead(404, {"Content-Type": "text/plain"});
		response.write("404 Not found");
		response.end();
	}
}).listen(32102);  


console.log('started...'); 