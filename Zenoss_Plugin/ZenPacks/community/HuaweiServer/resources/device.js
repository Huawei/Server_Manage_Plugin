Ext.onReady(function() {
    var DEVICE_OVERVIEW = 'deviceoverviewpanel_summary';
    Ext.ComponentMgr.onAvailable(DEVICE_OVERVIEW, function(){
        var overview = Ext.getCmp(DEVICE_OVERVIEW);
        overview.removeField('memory');

        var uid = Zenoss.env.device_uid;
        if (uid.match('^/zport/dmd/Devices/Server/Huawei/BMC')) {
	        overview.addField({
	            name: 'bmcDeviceName',
	            fieldLabel: _t("DeviceName")
	        });
	        overview.addField({
	            name: 'bmcHostName',
	            fieldLabel: _t("HostName")
	        });
	        overview.addField({
	            name: 'bmcBoardId',
	            fieldLabel: _t("BoardId")
	        });
        }
        else{
        
        }
        
    });

    var DEVICE_OVERVIEW_ID = 'deviceoverviewpanel_idsummary';
    Ext.ComponentMgr.onAvailable(DEVICE_OVERVIEW_ID, function(){
        var overview = Ext.getCmp(DEVICE_OVERVIEW_ID);

        var uid = Zenoss.env.device_uid;
        if (uid.match('^/zport/dmd/Devices/Server/Huawei/BMC')) {
	        overview.addField({
	            name: 'bmcBootSequence',
	            fieldLabel: _t("Boot Sequence")
	        });
	        overview.addField({
	            name: 'bmcFRUControl',
	            fieldLabel: _t("FRU Power Control")
	        });
        }
        else{
        
        }
        
    });
});
