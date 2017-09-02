// New menu option on the device list cog wheel menu, context-configure-menu

Ext.ComponentMgr.onAvailable('context-configure-menu', function(config) {
  var origOnGet = config.onGetMenuItems;
  config.onGetMenuItems = function(uid) {
    var result = origOnGet.call(this, uid) || [];
    // Menu item only shows up when certain device class is selected
    if( uid.match('^/zport/dmd/Devices/Server/Huawei/BMC') ) {
        result.push( {
            text: _t('Configure FRU Power'),
            hidden: Zenoss.Security.doesNotHavePermission('Manage Device'),
            handler: function() {
                var win = new Zenoss.dialog.CloseDialog({
                    width: 300,
                    title: _t('Modify Configuration'),
                    items: [{
                        xtype: 'form',
                        buttonAlign: 'left',
                        monitorValid: true,
                        labelAlign: 'top',
                        footerStyle: 'padding-left: 0',
                        border: false,
                        // Ensure that name field of items match the attribute names
                        //   that you want to populate
                        // allowBlank: false means OK button will not be active until this condition satisfied
                        //  if allowBlank set to true and field not supplied then field will be set to null
                        items: [{
                            xtype: 'textfield',
                            name: 'deviceip',
                            fieldLabel: _t('Device IP'),
                            id: "exampleDeviceDeviceIPField",
                            width: 260,
                            allowBlank: false
                        }, {
                            xtype: 'combo',
                            name: 'frupowercontrol',
                            fieldLabel: _t('FRU Power Action'),
                            id: "exampleDeviceFRUPowerCtrl",
                            store: [ [ 1, 'Power Off' ], [ 2, 'Power On' ]
                            ,[3, 'Forced System Reset'], [4, 'Forced Power Cycle'], [5, 'Forced Power Off'] ],
                            value: 1,
                            forceSelection: true,
                            editable: false,
                            width: 260,
                            autoSelect: true
                        }],
                        buttons: [{
                            xtype: 'DialogButton',
                            id: 'modifyBMCDevice-submit',
                            text: _t('Confirm'),
                            formBind: true,
                            handler: function(b) {
                                var form = b.ownerCt.ownerCt.getForm();
                                var opts = form.getFieldValues();
        
                                //  Following line must match the class defined in routers.py
                                //    and the last part must match the method defined on that class
                                //    ie. router class = myAppRouter, method = myRouterFunc
                                //    The 2 input fields for comments and rackSlot are passed as
                                //       opts to the router function.
        
                                Zenoss.remote.bmcRouter.routerfpc(opts,
                                function(response) {
                                    if (response.success) {
                                        new Zenoss.dialog.SimpleMessageDialog({
                                            title: _t(' Device FRU Power Control'),
                                            message: response.msg,
                                            buttons: [{
                                                xtype: 'DialogButton',
                                                text: _t('OK')
                                            }]
                                        }).show();
                                    }
                                    else {
                                        new Zenoss.dialog.SimpleMessageDialog({
                                            message: response.msg,
                                            buttons: [{
                                                xtype: 'DialogButton',
                                                text: _t('OK')
                                            }]
                                        }).show();
                                    }
                                });
                            }
                        }, Zenoss.dialog.CANCEL]
                    }]
                });
                win.show();
            }
        },{
            text: _t('Configure Boot Sequence'),
            hidden: Zenoss.Security.doesNotHavePermission('Manage Device'),
            handler: function() {
                var win = new Zenoss.dialog.CloseDialog({
                    width: 400,
                    title: _t('BMCBootSequence'),
                    items: [{
                        xtype: 'form',
                        buttonAlign: 'left',
                        monitorValid: true,
                        labelAlign: 'top',
                        footerStyle: 'padding-left: 0',
                        border: false,
                        // Ensure that name field of items match the attribute names
                        //   that you want to populate
                        // allowBlank: false means OK button will not be active until this condition satisfied
                        //  if allowBlank set to true and field not supplied then field will be set to null
                        items: [{
                            xtype: 'textfield',
                            name: 'deviceip',
                            fieldLabel: _t('Device IP'),
                            id: "exampleDeviceDeviceIPField",
                            width: 360,
                            allowBlank: false
                        }, {
                            xtype: 'combo',
                            name: 'cfgboottype',
                            fieldLabel: _t('Boot Type'),
                            id: "exampleBootTypeField",
                            store: [ [ 1, 'One-time' ]
                            ,[2, 'Permanent'] ],
                            value: 1,
                            forceSelection: true,
                            editable: false,
                            width: 360,
                            autoSelect: true
                        }, {
                            xtype: 'combo',
                            name: 'bootsequence',
                            fieldLabel: _t('BootSequence'),
                            id: "exampleDeviceBootOptionField",
                            store: [ [ 1, 'No override' ], [ 2, 'PXE' ], [ 3, 'Hard Drive' ]
                            ,[4, 'DVD ROM']
                            ,[5, 'FDD Removable Device']
                            ,[7, 'Bios Setup'] ],
                            value: 1,		            
                            forceSelection: true,
                            editable: false,
                            width: 360,
                            autoSelect: true
                        }],
                        buttons: [{
                            xtype: 'DialogButton',
                            id: 'modifyBMCDevice-submit',
                            text: _t('Confirm'),
                            formBind: true,
                            handler: function(b) {
                                var form = b.ownerCt.ownerCt.getForm();
                                var opts = form.getFieldValues();
        
                                //  Following line must match the class defined in routers.py
                                //    and the last part must match the method defined on that class
                                //    ie. router class = BMCRouter, method = myRouterFunc
                                //    The 2 input fields for comments and rackSlot are passed as
                                //       opts to the router function.
        
                                Zenoss.remote.bmcRouter.routerbs(opts,
                                function(response) {
                                    if (response.success) {
                                        new Zenoss.dialog.SimpleMessageDialog({
                                            title: _t(' Device Boot Sequence'),
                                            message: response.msg,
                                            buttons: [{
                                                xtype: 'DialogButton',
                                                text: _t('OK')
                                            }]
                                        }).show();
                                    }
                                    else {
                                        new Zenoss.dialog.SimpleMessageDialog({
                                            message: response.msg,
                                            buttons: [{
                                                xtype: 'DialogButton',
                                                text: _t('OK')
                                            }]
                                        }).show();
                                    }
                                });
                            }
                        }, Zenoss.dialog.CANCEL]
                    }]
                });
                win.show();            
            }
        });
    }
    else if( uid.match('^/zport/dmd/Devices/Server/Huawei/HMM') ) {
        result.push( {
            text: _t('Configure HMM BiosBootOption'),
            hidden: Zenoss.Security.doesNotHavePermission('Manage Device'),
            handler: function() {
                var win = new Zenoss.dialog.CloseDialog({
                    width: 400,
                    title: _t('Modify Configuration'),
                    items: [{
                        xtype: 'form',
                        buttonAlign: 'left',
                        monitorValid: true,
                        labelAlign: 'top',
                        footerStyle: 'padding-left: 0',
                        border: false,
                        // Ensure that name field of items match the attribute names
                        //   that you want to populate
                        // allowBlank: false means OK button will not be active until this condition satisfied
                        //  if allowBlank set to true and field not supplied then field will be set to null
                        items: [{
                            xtype: 'textfield',
                            name: 'deviceip',
                            fieldLabel: _t('Device IP'),
                            id: "exampleDeviceDeviceIPField",
                            width: 360,
                            allowBlank: false
                        }, {
                            xtype: 'textfield',
                            name: 'hmmbladenum',
                            fieldLabel: _t('Blade Num'),
                            id: "hmmBladeDeviceNum",
                            width: 360,
                            allowBlank: false
                        }, {
                            xtype: 'combo',
                            name: 'hmmbotype',
                            fieldLabel: _t('Boot Type'),
                            id: "exampleBootTypeField",
                            store: [ [ 0, 'Disable' ], [ 1, 'Once' ]
                            ,[2, 'Persistent'] ],
                            value: 1,
                            forceSelection: true,
                            editable: false,
                            width: 360,
                            autoSelect: true
                        },  {
                            xtype: 'combo',
                            name: 'hmmbiosbootoption',
                            fieldLabel: _t('Bios Boot Option'),
                            id: "exampleDeviceBootOptionField",
                            store: [ [ 0, 'No override' ], [ 1, 'PXE' ], [ 2, 'HDD' ]
                            ,[5, 'CD/DVD']
                            ,[6, 'Bios Settings']
                            ,[15, 'Floppy/primary removable media'] ],
                            value: 0,	            
                            forceSelection: true,
                            editable: false,
                            width: 360,
                            autoSelect: true
                        }],
                        buttons: [{
                            xtype: 'DialogButton',
                            id: 'modifyHMMDevice-submit',
                            text: _t('Confirm'),
                            formBind: true,
                            handler: function(b) {
                                var form = b.ownerCt.ownerCt.getForm();
                                var opts = form.getFieldValues();
                
                                Zenoss.remote.hmmRouter.routerbbo(opts,
                                function(response) {
                                    if (response.success) {
                                        new Zenoss.dialog.SimpleMessageDialog({
                                            title: _t(' Device Bios Boot Option'),
                                            message: response.msg,
                                            buttons: [{
                                                xtype: 'DialogButton',
                                                text: _t('OK')
                                            }]
                                        }).show();
                                    }
                                    else {
                                        new Zenoss.dialog.SimpleMessageDialog({
                                            message: response.msg,
                                            buttons: [{
                                                xtype: 'DialogButton',
                                                text: _t('OK')
                                            }]
                                        }).show();
                                    }
                                });
                            }
                        }, Zenoss.dialog.CANCEL]
                    }]
                });
                win.show();
            }
        }, {
        	text: _t('Configure HMM FRU Control'),
            hidden: Zenoss.Security.doesNotHavePermission('Manage Device'),
            handler: function() {
                var win = new Zenoss.dialog.CloseDialog({
                    width: 300,
                    title: _t('Modify Configuration'),
                    items: [{
                        xtype: 'form',
                        buttonAlign: 'left',
                        monitorValid: true,
                        labelAlign: 'top',
                        footerStyle: 'padding-left: 0',
                        border: false,
                        // Ensure that name field of items match the attribute names
                        //   that you want to populate
                        // allowBlank: false means OK button will not be active until this condition satisfied
                        //  if allowBlank set to true and field not supplied then field will be set to null
                        items: [{
                            xtype: 'textfield',
                            name: 'deviceip',
                            fieldLabel: _t('Device IP'),
                            id: "exampleDeviceDeviceIPField",
                            width: 260,
                            allowBlank: false
                        }, {
                            xtype: 'textfield',
                            name: 'hmmbladenum',
                            fieldLabel: _t('Blade Num'),
                            id: "hmmBladeDeviceNum",
                            width: 260,
                            allowBlank: false
                        }, {
                            xtype: 'combo',
                            name: 'hmmfrucontrol',
                            fieldLabel: _t('FRU Control'),
                            id: "exampleDeviceFRUControl",
                            store: [ [ 0, 'Force System Reset' ], [ 2, 'Force Power Cycle' ]
                            ,[3, 'NMI'] ],
                            value: 0,
                            forceSelection: true,
                            editable: false,
                            width: 260,
                            autoSelect: true
                        }],
                        buttons: [{
                            xtype: 'DialogButton',
                            id: 'modifyHMMDevice-submit',
                            text: _t('Confirm'),
                            formBind: true,
                            handler: function(b) {
                                var form = b.ownerCt.ownerCt.getForm();
                                var opts = form.getFieldValues();
        
                                //  Following line must match the class defined in routers.py
                                //    and the last part must match the method defined on that class
                                //    ie. router class = bmcRouter, method = myRouterFunc
                                //    The 2 input fields for comments and rackSlot are passed as
                                //       opts to the router function.
        
                                Zenoss.remote.hmmRouter.routerfrucontrol(opts,
                                function(response) {
                                    if (response.success) {
                                        new Zenoss.dialog.SimpleMessageDialog({
                                            title: _t(' Device FRU Power Control'),
                                            message: response.msg,
                                            buttons: [{
                                                xtype: 'DialogButton',
                                                text: _t('OK')
                                            }]
                                        }).show();
                                    }
                                    else {
                                        new Zenoss.dialog.SimpleMessageDialog({
                                            message: response.msg,
                                            buttons: [{
                                                xtype: 'DialogButton',
                                                text: _t('OK')
                                            }]
                                        }).show();
                                    }
                                });
                            }
                        }, Zenoss.dialog.CANCEL]
                    }]
                });
                win.show();
            }	
        });
    }
    return result;
  };
});


