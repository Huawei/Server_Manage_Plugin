/**
 * Created by Jack on 2017/4/17.
 */

getOrCreateDialog = function (id) {
    $box = $('#' + id);
    if (!$box.length) {
        $box = $('<div id="' + id + '"><p></p></div>').hide().appendTo('body');
    }
    return $box;
}

customAlert = function (message, title, options, callback) {
    callback = callback || function () { };

    var defaultDlg = {
        modal: true,
        resizable: false,
        buttons: {
            Ok: function () {
                $(this).dialog('close');
                return (typeof callback == 'string') ?
                    window.location.href = callback :
                    callback();
            }
        },
        title: title,
        show: 'fade',
        hide: 'fade',
        minHeight: 50,
        dialogClass: 'modal-shadow'
    };

    $alert = getOrCreateDialog('alert');

    $("p", $alert)
        .attr("title", title)
        .html(message);

    $alert.dialog($.extend({}, defaultDlg));
}

window.alert = function (message) {
    customAlert(message, 'Alert');
}