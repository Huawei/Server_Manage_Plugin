document.write("<script src='../js/lang.js'></script>");

function langSetting(settingLang) {
    var currentLang = localStorage.getItem('lang');
    if (settingLang == currentLang) {
        console.log("Current lang is correct lang.");
    } else {
        changelang(settingLang);
        console.log("Replace lang using settingLang: " + settingLang + ".");
    }
}

/**
 * 改变当前语言
 * @param {string} lang (zhCN,en)
 */
function changelang(lang) {
    if (lang == 'zhCN') {
        ELEMENT.locale(ELEMENT.lang.zhCN);
        localStorage.setItem('lang', 'zhCN');
        this.lang = ELEMENT.lang.zhCN.el.templateManage;
    } else {
        ELEMENT.locale(ELEMENT.lang.en);
        this.lang = ELEMENT.lang.en.el.templateManage;
        localStorage.setItem('lang', 'en');
    }
}
/**
 * 获取禁用启用
 **/
function getForbiddenType() {
    var lang = localStorage.getItem('lang');
    if (lang) {
        if (lang == 'en') {
            return [{
                value: 'Enabled',
                label: 'Enabled'
            }, {
                value: 'Disabled',
                label: 'Disabled'
            }];
        }
    }
    return [{
        value: 'Enabled',
        label: '启用'
    }, {
        value: 'Disabled',
        label: '禁用'
    }];
}
/**
 * 获取模板类别
 **/
function getTemplateType() {
    var lang = localStorage.getItem('lang');
    if (lang) {
        if (lang == 'en') {
            return [{
                value: 'OS',
                label: 'OS template'
            }, {
                value: 'POWER',
                label: 'Power template'
            }, {
                value: 'BIOS',
                label: 'BIOS template'
            }, {
                value: 'HBA',
                label: 'HBA template'
            }, {
                value: 'RAID',
                label: 'RAID template'
            }, {
                value: 'CNA',
                label: 'CNA template'
            }, {
                value: 'IBMC',
                label: 'iBMC template'
            }];
        }
    }
    return [{
        value: 'OS',
        label: 'OS模板'
    }, {
        value: 'POWER',
        label: '上下电模板'
    }, {
        value: 'BIOS',
        label: 'BIOS模板'
    }, {
        value: 'HBA',
        label: 'HBA模板'
    }, {
        value: 'RAID',
        label: 'RAID模板'
    }, {
        value: 'CNA',
        label: 'CNA模板'
    }, {
        value: 'IBMC',
        label: 'iBMC 模板'
    }];
}

/**
 * 根据value值获取下拉列表Label名称
 * @param {string} arry 
 * @param {string} value
 */
function getOptionlabel(arry, value) {
    var optionItem = _.find(arry, function(x) {
        return x.value == value;
    });
    if (optionItem) {
        return optionItem.label;
    }
    return value;
}

/**
 * 获取服务器型号
 **/
function getServerType() {
    var lang = localStorage.getItem('lang');
    if (lang) {
        if (lang == 'en') {
            return [{
                value: 'Rack',
                label: 'Rack'
            }, {
                value: 'Blade',
                label: 'Blade'
            }, {
                value: 'Highdensity',
                label: 'Highdensity'
            }];
        }
    }
    return [{
        value: '机架服务器',
        label: '机架服务器'
    }, {
        value: '刀片服务器',
        label: '刀片服务器'
    }, {
        value: '高密服务器',
        label: '高密服务器'
    }];
}
/**
 * 获取部署设备类别
 **/
function getOSDeployPolicys() {
    var lang = localStorage.getItem('lang');
    if (lang) {
        if (lang == 'en') {
            return [{
                value: '0',
                label: 'USB Device'
            }, {
                value: '1',
                label: 'Hard Disk'
            }, {
                value: '2',
                label: 'San Boot'
            }]
        }
    }
    return [{
        value: '0',
        label: 'USB设备'
    }, {
        value: '1',
        label: '内置硬盘'
    }, {
        value: '2',
        label: '网络设备'
    }];

}



/**
 * 模板类别改变
 * @templateType 模板类别
 **/
function select_templateChange(templateType) {
    switch (templateType) {
        case 'OS':
            window.location.href = 'addOS.html?s=' + Math.random();
            break;
        case 'POWER':
            window.location.href = 'addPower.html?s=' + Math.random();
            break;
        case 'BIOS':
            window.location.href = 'addBIOS.html?s=' + Math.random();
            break;
        case 'HBA':
            window.location.href = 'HBA.html?s=' + Math.random();
            break;
        case 'RAID':
            window.location.href = 'RAID.html?s=' + Math.random();
            break;
        case 'CNA':
            window.location.href = 'CNA.html?s=' + Math.random();
            break;
        case 'IBMC':
            window.location.href = 'iBMC.html?s=' + Math.random();
            break;
        default:
            break;
    }
}

/**
 * 获取esight列表
 **/
function getEsightList(callback) {
    if (window.NetBound == undefined || window.NetBound == null || !window.NetBound) {
        //判断cefBrowser是否已注册JsObject
        alert('window.NetBound does not exist.');
        console.log('window.NetBound does not exist.');
        return;
    }
		
		var param = { "defaultParam": "" };
		ExecuteAnynsMethod(param, "loadESightList", false, (resultStr) => {
			var resultJson = JSON.parse(resultStr);
			console.log("loadESightList result: ");
			console.log(resultJson);
			var esightData = resultJson.Data;
			
			if (typeof callback === "function") {
					var ret = { code: resultJson.Code, msg: resultJson.ExceptionMsg, data: esightData }
					localStorage.setItem('esightList', JSON.stringify(esightData)); //code==0时
					callback(ret);
			}
		});	
}
/**
 * 获取esight列表
 **/
function getEsightListAsync() {
    return new Promise(function(resolve, reject) {
        var esightData = [{
            id: 1,
            hostIp: '127.0.0.1',
            aliasName: "host",
            hostPort: '32102',
            loginAccount: 'sxw',
            loginPwd: '',
            latestStatus: '',
            reservedInt1: '',
            reservedInt2: '',
            reservedStr1: '',
            reservedStr2: '',
            lastModify: '2017-05-22',
            createTime: '2017-05-22 17:28:46'
        }, {
            id: 2,
            hostIp: '127.0.0.2',
            aliasName: "",
            hostPort: '32102',
            loginAccount: 'sxw',
            loginPwd: '',
            latestStatus: '',
            reservedInt1: '',
            reservedInt2: '',
            reservedStr1: '',
            reservedStr2: '',
            lastModify: '2017-05-22',
            createTime: '2017-05-22 17:28:46'
        }, {
            id: 3,
            hostIp: '127.0.0.3',
            aliasName: "server",
            hostPort: '32102',
            loginAccount: 'sxw',
            loginPwd: '',
            latestStatus: '',
            reservedInt1: '',
            reservedInt2: '',
            reservedStr1: '',
            reservedStr2: '',
            lastModify: '2017-05-22',
            createTime: '2017-05-22 17:28:46'
        }];
        var ret = { code: '0', msg: '', data: esightData }
        dealResult(ret, resolve, reject);
    });
}

/**
 * 国际化
 **/
function getIn18() {
    var lang = localStorage.getItem('lang');
    if (lang) {
        if (lang == 'en') {
            ELEMENT.locale(ELEMENT.lang.en);
            return i18n_en;
        } else {
            ELEMENT.locale(ELEMENT.lang.zhCN);
            return i18n_zh_CN;

        }
    } else {
        ELEMENT.locale(ELEMENT.lang.zhCN);
        return i18n_zh_CN;
    }
}

/**
 * 设置当前Esight
 **/
function setCurrentEsight(esight) {
    localStorage.setItem('esight', esight);
}

/**
 * 获取当前Esight
 **/
function getCurrentEsight() {
    return localStorage.getItem('esight');
}

/**
 * 弹出提示框
 * @param {String}  msg 消息内容
 * @param {Function} cb 回调函数
 */
function alertMsg(msg, cb) {
    if (app) {
        app.$alert(msg, app.i18ns.common.prompt, {
            confirmButtonText: app.i18ns.common.confirm,
            callback: function() {
                cb && cb();
            }
        });
    } else {
        alert(msg);
    }
}

/**
 * 获取任务状态
 */
function getTaskStatus() {
    var lang = localStorage.getItem('lang');
    if (lang) {
        if (lang == 'en') {
            return [
                { value: 'CREATED', label: 'Running' },
                { value: 'SYNC_FAILED', label: 'Sync Failed' },
                { value: 'HW_FAILED', label: 'Failed' },
                { value: 'PARTION_FAILED', label: 'Partion Success' },
                { value: 'FINISHED', label: 'Complete' }
            ];
        }
    }
    return [
        { value: 'CREATED', label: '运行中' },
        { value: 'SYNC_FAILED', label: '同步失败' },
        { value: 'HW_FAILED', label: '失败' },
        { value: 'PARTION_FAILED', label: '部分成功' },
        { value: 'FINISHED', label: '完成' }
    ];
}

/**
 * 获取任务状态（搜索时）
 */
function getSearchTaskStatus() {
    var lang = localStorage.getItem('lang');
    if (lang) {
        if (lang == 'en') {
            return [
                { value: '', label: 'All' },
                { value: 'CREATED', label: 'Running' },
                { value: 'SYNC_FAILED', label: 'Sync Failed' },
                { value: 'HW_FAILED', label: 'Failed' },
                { value: 'PARTION_FAILED', label: 'Partion Success' },
                { value: 'FINISHED', label: 'Complete' }
            ];
        }
    }
    return [
        { value: '', label: '全部' },
        { value: 'CREATED', label: '运行中' },
        { value: 'SYNC_FAILED', label: '同步失败' },
        { value: 'HW_FAILED', label: '失败' },
        { value: 'PARTION_FAILED', label: '部分成功' },
        { value: 'FINISHED', label: '完成' }
    ];
}

/**
 * 获取升级固件类型
 */
function getFirmwareTypeList() {
    var lang = localStorage.getItem('lang');
    if (lang) {
        if (lang == 'en') {
            return [
                { value: 'All', label: 'All' },
                { value: 'CNA_DRIVE', label: 'CNA&HBA&NIC DRIVE' },
                { value: 'RAID_DRIVE', label: 'RAID DRIVE' },
                { value: 'HMM', label: 'HMM' },
                { value: 'iBMC', label: 'iBMC' },
                { value: 'BIOS', label: 'BIOS' },
                { value: 'CPLD', label: 'CPLD' },
                { value: 'LCD', label: 'LCD' },
                { value: 'Fabric', label: 'Fabric' },
                { value: 'CHN', label: 'CNA&HBA&NIC' },
                { value: 'RAID', label: 'RAID' },
                { value: 'HDD', label: 'HDD' },
                { value: 'NVME', label: 'NVME' },
                { value: 'NVDIMM', label: 'NVDIMM' },
                { value: 'SSD', label: 'SSD' },
                { value: 'IB', label: 'IB' },
                { value: 'OTHERS', label: 'OTHERS' }
            ];
        }
    }
    return [
        { value: 'All', label: '全选' },
        { value: 'CNA_DRIVE', label: 'CNA&HBA&NIC 驱动' },
        { value: 'RAID_DRIVE', label: 'RAID 驱动' },
        { value: 'HMM', label: 'HMM' },
        { value: 'iBMC', label: 'iBMC' },
        { value: 'BIOS', label: 'BIOS' },
        { value: 'CPLD', label: 'CPLD' },
        { value: 'LCD', label: 'LCD' },
        { value: 'Fabric', label: 'Fabric' },
        { value: 'CHN', label: 'CNA&HBA&NIC' },
        { value: 'RAID', label: 'RAID' },
        { value: 'HDD', label: 'HDD' },
        { value: 'NVME', label: 'NVME' },
        { value: 'NVDIMM', label: 'NVDIMM' },
        { value: 'SSD', label: 'SSD' },
        { value: 'IB', label: 'IB' },
        { value: 'OTHERS', label: '其他' }
    ];
}

/**
 * alertMsg('alert msg')
 * alertMsg('alert msg',function(){console.log('alert finish')})
 * @param {*} msg 
 * @param {*} cb 
 */
function alertMsg(msg, cb) {
    if (app) {
        app.$alert(msg, app.i18ns.common.prompt, {
            confirmButtonText: app.i18ns.common.confirm,
            callback: function() {
                cb && cb();
            }
        });
    } else {
        alert(msg);
    }
}
/**
 * alertCode(-99999)
 * alertCode(9999,function(){console.log('alert finish')})
 * @param {*} errCode 
 * @param {*} cb 
 */
function alertCode(errCode, cb) {
    var msg = getErrorMsg(errCode);
    if (app) {
        /*  app.$alert(msg, app.i18ns.common.prompt, {
             confirmButtonText: app.i18ns.common.confirm,
             callback: function() {
                 cb && cb();
             }
         }); */
        var h = app.$createElement;
        var nodes = [];
        var msgTxts = msg.split("</br>")
        for (var i = 0; i < msgTxts.length; i++) {
            nodes.push(h('p', null, msgTxts[i]));
        }
        var context = h('div', null, nodes);
        app.$msgbox({
            title: app.i18ns.common.prompt,
            message: context,
            confirmButtonText: app.i18ns.common.confirm,
        }).then(function() {
            cb && cb();
        });
    } else {
        alert(msg);
    }
}

/**
 * confirm('please confirm').then(()=>{console.log('confirm ok')})
 * confirm('please confirm').then(()=>{console.log('confirm ok')},()=>{console.log('cancel')})
 * @param {*} msg 
 */
function confirm(msg) {
    if (app) {
        return app.$confirm(msg, app.i18ns.common.prompt, {
            confirmButtonText: app.i18ns.common.confirm,
            cancelButtonText: app.i18ns.common.cancel,
            closeOnClickModal: false,
            type: 'warning'
        });
    } else {
        console.error('app is undefind');
    }
}

/**
 * notifySuccess("Success")
 * notifySuccess("Success").then(()=>{console.log('callback')})
 */
function notifySuccess(msg, duration) {
    return new Promise(function(reslove) {
        app.$notify({
            message: msg || 'default msg',
            duration: duration || 2000,
            type: 'success'
        });
        setTimeout(function() {
            reslove && reslove();
        }, duration || 2000);
    });
}
/**
 * notifyInfo("Info")
 * notifyInfo("Info").then(()=>{console.log('callback')})
 */
function notifyInfo(msg, duration) {
    return new Promise(function(reslove) {
        app.$notify({
            message: msg || 'default msg',
            duration: duration || 2000
        });
        setTimeout(function() {
            reslove && reslove();
        }, duration || 2000);
    });
}

/**
 * notifyWarn("Warn")
 * notifyWarn("Warn").then(()=>{console.log('callback')})
 */
function notifyWarn(msg, duration) {
    return new Promise(function(reslove) {
        app.$notify({
            message: msg || 'default msg',
            duration: duration || 2000,
            type: 'Warn'
        });
        setTimeout(function() {
            reslove && reslove();
        }, duration || 2000);
    });
}

/**
 * notifyError("error")
 * notifyError("error").then(()=>{console.log('callback')})
 */
function notifyError(msg, duration) {
    return new Promise(function(reslove) {
        app.$notify.error({
            message: msg || 'default msg',
            duration: duration || 2000
        });
        setTimeout(function() {
            reslove && reslove();
        }, duration || 2000);
    })
}

/**
 * dealResult(ret,function(){})
 * dealResult(ret,function(){},function(){})
 */
function dealResult(ret, success, faild) {
    if (app && app.fullscreenLoading) {
        app.fullscreenLoading = false;
    }
    if (app && app.loading) {
        app.loading = false;
    }
    if (app && app.secondloading) {
        app.secondloading = false;
    }
    if (ret.code == '0') {
        success && success(ret);
    } else {
        if (ret.code == '-100000') {
            operationFailed(ret, faild);
        } else {
            faild && faild(ret);
            alertCode(ret.code);
            console.warn(ret);
        }
    }
}
/**
 * 返回-100000时错误处理
 * @param {*} ret 
 */
function operationFailed(ret, faild) {
    if (ret.data && ret.data.length > 0) {
        var data = ret.data;
        var h = app.$createElement;
        var nodes = [];
        for (var i = 0; i < data.length; i++) {
            var msgTxt = getEsightaliasName(data[i].esight) + '  :  ' + getErrorMsg(data[i].code);
            nodes.push(h('p', null, msgTxt));
        }
        var mgs = h('div', null, nodes);
        app.$msgbox({
            title: app.i18ns.common.prompt,
            message: mgs,
            confirmButtonText: app.i18ns.common.confirm,
        }).then(function() {
            if (faild) {
                faild(ret);
            } else {
                var list_url = localStorage.getItem('list_url');
                if (list_url) {
                    location.href = list_url;
                }
            }
        });
    }
}

/**
 * 根据eSightIp获取别名
 * @param {*} ip 
 */
function getEsightaliasName(ip) {
    var esights = localStorage.getItem('esightList');
    if (esights) {
        var esightList = JSON.parse(esights);
        for (var i = 0; i < esightList.length; i++) {
            if (esightList[i].hostIp == ip) {
                if (esightList[i].aliasName) {
                    return esightList[i].aliasName;
                }
                return ip;
            }
        }
    }
    return ip;
}
var pageEvents = {
        handleSizeChange: function(pageSize) {
            localStorage.setItem("taskListPageSize", pageSize);
            app.pageSize = pageSize;
            app.getListData();
        },
        handleCurrentChange: function(pageNo) {
            app.pageNo = pageNo;
            app.getListData();
        },
    }
    /**
     * 封装返回列表页弹出提示
     * @param {*} url 
     */
function goBack(url) {
    app.$confirm(app.i18ns.common.beBackTips, app.i18ns.common.prompt, {
        confirmButtonText: app.i18ns.common.confirm,
        cancelButtonText: app.i18ns.common.cancel,
        closeOnClickModal: false,
        type: 'warning'
    }).then(function() {
        location.href = url + '?s=' + Math.random();
    }).catch(function() {});
}

function preventNonNum(event) {
    if ((event.keyCode < 48 || event.keyCode > 57) && event.keyCode != 46 || event.keyCode == 13 || event.keyCode == 16 || event.keyCode == 103 || event.keyCode == 102)
        event.returnValue = false
}

function getSystemBootOption() {
    var lang = localStorage.getItem('lang');
    if (lang) {
        if (lang == 'en') {
            return [{
                    value: '1',
                    label: 'Default'
                }, {
                    value: '0',
                    label: 'PXE'
                },
                {
                    value: '2',
                    label: 'CD/DVD/ ROM'
                }, {
                    value: '3',
                    label: 'Hard Disk'
                }, {
                    value: '4',
                    label: 'FDD'
                }
            ];
        }
    }
    return [{
            value: '1',
            label: '默认'
        }, {
            value: '0',
            label: 'PXE'
        },
        {
            value: '2',
            label: 'CD/DVD/ ROM'
        }, {
            value: '3',
            label: '硬盘'
        }, {
            value: '4',
            label: '软盘'
        }
    ];
}
//获取服务器源选项
function getNTPServerSources() {
    var lang = localStorage.getItem('lang');
    if (lang) {
        if (lang == 'en') {
            return [{
                value: '1',
                label: 'Automatically obtain IPv4'
            }, {
                value: '2',
                label: 'Automatically obtain IPv6'
            }, {
                value: '0',
                label: 'Manually obtain'
            }];
        }
    }
    return [{
        value: "1",
        label: "自动获取IPV4"
    }, {
        value: "2",
        label: "自动获取IPV6"
    }, {
        value: "0",
        label: "手动获取"
    }];
}

//获取组权限
function getGroupPrivileges() {
    var lang = localStorage.getItem('lang');
    if (lang) {
        if (lang == 'en') {
            return [{
                value: '1',
                label: 'CommonUser'
            }, {
                value: '2',
                label: 'Operator'
            }, {
                value: '3',
                label: 'Administrator'
            }];
        }
    }
    return [{
        value: "1",
        label: "普通用户"
    }, {
        value: "2",
        label: "操作员"
    }, {
        value: "3",
        label: "管理员"
    }];
}

/**
 * 根据类型获取软件源版本
 * @param {*} type 
 */
function getSoftSourceVersion(type) {
    var data = [];
    switch (type) {
        case '2':
            data = [
                { name: "SUSE Linux Enterprise 11 SP1 x64" },
                { name: "SUSE Linux Enterprise 11 SP2 x64" },
                { name: "SUSE Linux Enterprise 11 SP3 x64" },
                { name: "SUSE Linux Enterprise 11 SP4 x64" },
                { name: "SUSE Linux Enterprise 12 SP1 x64" }
            ];
            break;
        case '3':
            data = [
                { name: "Red Hat Linux Enterprise 6.1 x64" },
                { name: "Red Hat Linux Enterprise 6.2 x64" },
                { name: "Red Hat Linux Enterprise 6.3 x64" },
                { name: "Red Hat Linux Enterprise 6.5 x64" },
                { name: "Red Hat Linux Enterprise 6.6 x64" },
                { name: "Red Hat Linux Enterprise 7.0 x64" },
                { name: "Red Hat Linux Enterprise 7.1 x64" },
                { name: "Red Hat Linux Enterprise 7.2 x64" }
            ];
            break;
        case '4':
            data = [
                { name: "CentOS Linux Enterprise 6.3 x64" },
                { name: "CentOS Linux Enterprise 6.5 x64" },
                { name: "CentOS Linux Enterprise 6.6 x64" },
                { name: "CentOS Linux Enterprise 6.7 x64" },
                { name: "CentOS Linux Enterprise 7.0 x64" },
                { name: "CentOS Linux Enterprise 7.1 x64" }
            ];
            break;
        case '7':
            data = [
                { name: "Ubuntu Linux Enterprise 14.04 x64" }
            ];
            break;
        default:
            break;
    }
    return data;
}
/**
 * 字符串扩展函数
 */
String.prototype.format = function(args) {
    var result = this;
    if (arguments.length > 0) {
        if (arguments.length == 1 && typeof(args) == "object") {
            for (var key in args) {
                if (args[key] != undefined) {
                    var reg = new RegExp("({" + key + "})", "g");
                    result = result.replace(reg, args[key]);
                }
            }
        } else {
            for (var i = 0; i < arguments.length; i++) {
                if (arguments[i] != undefined) {
                    //var reg = new RegExp("({[" + i + "]})", "g");//这个在索引大于9时会有问题，谢谢何以笙箫的指出
                    　　　　　　　　　　　　
                    var reg = new RegExp("({)" + i + "(})", "g");
                    result = result.replace(reg, arguments[i]);
                }
            }
        }
    }
    return result;
}

//判断是否空对象 by Jacky on 2017-8-24
function isEmptyObject(obj) {
	if (JSON.stringify(obj) === "{}") {
		return true;
	} else {
		return false;
	}
}

//判断是否空对象并附加默认参数 by Jacky on 2017-8-24
function isEmptyObjectWithDefaultParameter(obj) {
	if (isEmptyObject(obj)) {
		return { "defaultParam": "" };
	} else {
		return obj;
	} 
}

/*
 *功能：异步执行rest.js中跟CefSharp的交互方法（只处理Promise逻辑）
 *参数：ayncResult -> 一个Promise
 *			callback -> 回调函数
 *作者：Jacky
 *日期：2017-8-24
 */
function ExecuteAnynsMethodOnlyHandlerPromise(ayncResult, callback) {
	ayncResult.then(function (resultStr) {
		callback(resultStr);
	}).catch((exception) => {
		console.log(exception);
		alert(exception);
	});
}

/*
 *功能：异步执行rest.js中跟CefSharp的交互方法
 *参数：param -> 交互方法的参数
 *			methodName -> 交互方法的名称
 *			isCheckParam -> 是否需要检查是不是空参数
 *			callback -> 回调函数
 *作者：Jacky
 *日期：2017-8-24
 */
function ExecuteAnynsMethod(param, methodName, isCheckParam, callback) {
	if (isCheckParam) {
		param = isEmptyObjectWithDefaultParameter(param);
	}
	var ayncResult = NetBound.execute(methodName, param);
	ExecuteAnynsMethodOnlyHandlerPromise(ayncResult, callback);
}