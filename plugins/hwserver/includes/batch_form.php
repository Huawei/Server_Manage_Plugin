<?php
/***************************************************************************
 * Author  ......... Jack Zhang
 * Version ......... 1.0
 * History ......... 2017/4/18 Created
 * Purpose ......... The batch operate form is used by both scan_server and
 *                       batch_import pages.
 ***************************************************************************/

function server_batch_form($title, $empty_message, $show_ping=true) {
    global $config;
    $tokens = csrf_get_tokens();
    $name = $GLOBALS['csrf']['input-name'];
?>
    <form name="chk">
    <table id="tblScanResult" class="cactiTable" cellpadding="3" cellspacing="0" border="0" width="100%">
        <tr>
            <td colspan="100" class="textHeaderDark">
                <?php echo $title; ?>
            </td>
        </tr>
        <tr class='tableHeader'>
            <th><a class='textSubHeaderDark' href='#'>IP Address</a></th>
            <?php if ($show_ping) { ?>
            <th><a class='textSubHeaderDark' href='#'>PING Latency</a></th>
            <?php } ?>
            <th>SNMP Ver</th>
            <th>SNMP Port</th>
            <th>Community</th>
            <th>User Name</th>
            <th>Auth Protocol</th>
            <th>Auth Pass</th>
            <th>Privacy Protocol</th>
            <th>Privacy Pass</th>
            <th width="1%" align="right" class='tdSelectAll' align='right'>
                <input type='checkbox' name='all' title='Select All' onClick='SelectAll("chkSelect_",this.checked)'>
            </th>
        </tr>
        <tr class='odd selectable' id="rowEmpty">
            <td colspan="12" onClick='select_line("1")'>
                <?php echo $empty_message; ?>
            </td>
        </tr>
    </table>
    <table align="right" style="margin-top:20px;">
        <tr>
            <td>
                <input type="hidden" id="batchImportId">
                <input id="btnImport" style="display:none;" type="button" value="Import Selected Servers">
                <input id="btnReturn" type="button" value="Return" onClick="window.location.href='server_list.php'">
            </td>
        </tr>
    </table>
    </form>

    <div id="importDialog" title="Import server is in progress, please wait...">
        <div class="progress-label"></div>
        <div class="progressbar" style="margin-top: 20px;"></div>
    </div>

    <link href="<?php echo $config['url_path']; ?>plugins/hwserver/static/hwserver.css" type="text/css" rel="stylesheet">
    <script>
        $("#btnReturn").button();
        var importServersList = [];

        var importButton = $("#btnImport");
        importButton.button().on("click", function () {
            var batchImporter = new BatchImporter(
                $("#batchImportId").val(),
                "importDialog",
                function(){
                    importButton.button("option", {
                        disabled: true,
                        label: "Importing..."
                    });
                },
                function() {
                    importButton.button("option", {
                        disabled: false,
                        label: "Import Selected Servers"
                    });
                    importButton.trigger("focus");
                });

            batchImporter.startImporting(importServersList);
        });

        function BatchImporter(batchId, dialogId, onStart, onStop) {
            var _this = this;

            this.onStart = onStart;
            this.onStop = onStop;
            this.batchId = batchId;
            this.progressTimer = undefined;
            this.progressbar = $("#"+dialogId+" .progressbar");
            this.progressLabel = $("#"+dialogId+" .progress-label");
            this.importDialog = $("#"+dialogId).dialog({
                autoOpen: false,
                modal: true,
                closeOnEscape: false,
                resizable: false,
                beforeClose: function () {
                    if (_this.onStop) _this.onStop();
                }
            });

            this.startImporting = function(serverList) {
                if (!(serverList && serverList.length > 0))
                    return alert('No data!');

                if(_this.onStart) _this.onStart();

                var submitServers = [];
                var i = serverList.length;
                while (i--) {
                    var server = serverList[i];
                    //already have, needn't send it, reduce network traffic
                    delete server.batch_id;

                    if ($('#chkSelect_' + i).is(':checked')) {
                        server.snmp_version = $('#snmpVersion' + i).val();
                        server.snmp_port = $('#port' + i).val();
                        server.snmp_community = $('#community' + i).val();
                        if (server.snmp_version == 3) {
                            server.snmp_username = $('#secName' + i).val();
                            server.snmp_auth_level = $('#authLevel' + i).val();
                            server.snmp_auth_protocol = $('#authProtocol' + i).val();
                            server.snmp_password = $('#authPass' + i).val();
                            server.snmp_priv_protocol = $('#privProtocol' + i).val();
                            server.snmp_priv_passphrase = $('#priPass' + i).val();
                        }

                        submitServers.push(server);
                    }
                }

                if (submitServers.length == 0) {
                    if (_this.onStop) _this.onStop();
                    return alert("Please select at least one server.");
                }

                //now ajax waiting server to send snmp query
                $.ajax({
                    url: 'batch_import.php',
                    type: 'POST',
                    data: {
                        <?php echo $name ?> : '<?php echo $tokens ?>',
                        action: 'import',
                        batch_id: _this.batchId,
                        overwrite_exist: $("#overwrite_exist").is(':checked') ? 1 : 0,
                        servers: submitServers
                    },
                    dataType: 'json',
                    success: function (data, textStatus, jqXHR) {
                        if (!data.success) {
                            _this.stopImporting(true);
                            if (data.data +"" == "0") {
                                return alert("no server imported because of all servers are exist.\nand you choosed not overwrite exist item(s).");
                            }

                            return alert(data.message);
                        }
                    },
                    error: function (xhr, textStatus) {
                        _this.stopImporting(true);
                        alert("Unexpected error, please check your network!");
                    }
                })

                _this.importDialog.dialog("option", "buttons", [{
                    text: "Running in Background",
                    click: function() { _this.stopImporting(true); }
                }]);

                _this.importDialog.dialog("open");
                $(".ui-dialog button").last().trigger("focus");
                _this.progressTimer = setInterval(_this.importProgress, 3000);
            }

            this.importProgress = function() {
                $.ajax({
                    url: 'batch_import.php',
                    type: 'POST',
                    data: {
                        <?php echo $name ?> : '<?php echo $tokens ?>',
                        action: 'import_progress',
                        batch_id: _this.batchId
                    },
                    dataType: 'json',
                    success: function (data, textStatus, jqXHR) {
                        if (!data.success) {
                            return alert("get import process error: " + data.message);
                        }

                        var progress = Math.floor((data.data.progress * 100) / data.data.total);
                        _this.progressbar.progressbar("value", progress);
                        if (data.data.progress >= data.data.total) {
                            clearInterval(_this.progressTimer);

                            //redirect to import result page
                            if (!_this.error)
                                window.location.href = "<?php echo $config['url_path']; ?>plugins/hwserver/import_result.php?batch_id=" + _this.batchId;
                        }
                    },
                    error: function (xhr, textStatus) {
                        alert("Unexpected error, please check your network!");
                        _this.stopImporting(true);
                    }
                })
            }

            this.stopImporting = function(error) {
                if (_this.progressTimer)
                    clearInterval(_this.progressTimer);

                _this.importDialog.dialog("option", "buttons", []);
                _this.importDialog.dialog("close");
                _this.progressbar.progressbar("value", false);
                _this.progressLabel.text("Import Selected Servers");
                _this.error = error;
            }

            _this.progressbar.progressbar({
                value: false,
                change: function () {
                    _this.progressLabel.text("Current Progress: " + _this.progressbar.progressbar("value") + "%");
                },
                complete: function () {
                    _this.progressLabel.text("Complete!");
                }
            });
        }

        function batchResultTable(batch_id, serverList) {
            if (importServersList)
                delete importServersList;
            importServersList = serverList;

            $("#batchImportId").val(batch_id);

            var resultTable = $('#tblScanResult');
            var rows = resultTable.find("tr");
            for (var i=0;i<rows.length;i++) {
                if (i<2) continue;
                $(rows[i]).remove();
            }

            var rowClassNames = ["even-alternate", "odd"];
            var i = serverList.length;
            while (i--) {
                var server = serverList[i];
                var latency = 'timeout';
                if (server.ping_latency == 0)
                    latency = '<1ms';
                else if (server.ping_latency > 0)
                    latency = server.ping_latency+'ms';
                var rowClass = rowClassNames[i % 2];
                if (server.status == 1)
                    rowClass = 'pingSuccess';
                else if (server.status == 2)
                    rowClass = 'pingWarning';
                else if (server.status == 3)
                    rowClass = 'pingError';
                var addRow = ' \
                <tr class="' +rowClass+ ' selectable" id="dqline_' +i+ '"> \
                    <td>' +server.ip_address+ '</td> \
                    <?php if ($show_ping) { ?>
                    <td>' +latency+ '</td> \
                    <?php } ?>
                    <td>' +snmpVerList(i, server.snmp_version)+ '</td> \
                    <td><input id="port' +i+ '" value="'+(server.snmp_port?server.snmp_port:161)+ '" class="inputText"></td> \
                    <td><input id="community' +i+ '" value="'+(server.snmp_community?server.snmp_community:'public')+ '" class="inputText"></td> \
                    <td><input id="secName' +i+ '" value="'+(server.snmp_username?server.snmp_username:'')+ '" class="inputText"></td> \
                    <td>' +authProtocolList(i, server.snmp_auth_protocol)+ '</td> \
                    <td><input id="authPass' +i+ '" value="'+(server.snmp_password?server.snmp_password:'')+ '" type="password" class="inputText"></td> \
                    <td>' +priProtocolList(i, server.snmp_priv_protocol)+ '</td> \
                    <td><input id="priPass' +i+ '" value="'+(server.snmp_priv_passphrase?server.snmp_priv_passphrase:'')+ '" type="password" class="inputText"></td> \
                    <td width="1%" align="right"> \
                        <input type="checkbox" name="chkSelect_' +i+ '" id="chkSelect_' +i+ '" ' +(server.status == 1 ? 'checked' : '')+ '> \
                    </th> \
                </tr>';

                resultTable.append(addRow);
            }

            $('#btnImport').show();
        }

        function snmpVerList(i, selected) {
            return ' \
            <select id="snmpVersion' +i+ '"> \
                <option value="1" ' +((1==selected) ? 'selected' : '')+ '>Version 1</option> \
                <option value="2" ' +((2==selected) ? 'selected' : '')+ '>Version 2</option> \
                <option value="3" ' +((3==selected) ? 'selected' : '')+ '>Version 3</option> \
            </select>';
        }

        function authLevelList(i, selected) {
            return ' \
            <select id="authLevel' +i+ '"> \
                <option value="2" ' +((2==selected) ? 'selected' : '')+ '>authNoPriv</option> \
                <option value="3" ' +((3==selected) ? 'selected' : '')+ '>authPriv</option> \
            </select>';
        }

        function authProtocolList(i, selected) {
            selected = selected ? selected.toUpperCase() : '';
            return ' \
            <select id="authProtocol' +i+ '"> \
                <option value="MD5" ' +(('MD5'==selected) ? 'selected' : '')+ '>MD5 (default)</option> \
                <option value="SHA" ' +(('SHA'==selected) ? 'selected' : '')+ '>SHA</option> \
            </select>';
        }

        function priProtocolList(i, selected) {
            selected = selected ? selected.toUpperCase() : '';
            return ' \
            <select id="privProtocol' +i+ '"> \
                <option value="[None]" ' +((!selected || '[None]'==selected) ? 'selected' : '')+ '>[None]</option> \
                <option value="DES" ' +(('DES'==selected) ? 'selected' : '')+ '>DES (default)</option> \
                <option value="AES128" ' +(('AES' == selected || 'AES128' == selected) ? 'selected' : '')+ '>AES</option>  \
            </select>';
        }
    </script>

    <?php
}
?>