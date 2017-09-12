// Use JQuery's $(document).ready to execute the script when the document is loaded.
// All variables and functions are also hidden from the global scope.

$(document).ready(function() {
   // Namespace shortcut
   var ns = com_huawei_vcenterpluginui;

   // Get current object and return if document is loaded before context is set
   var objectId = WEB_PLATFORM.getObjectId();
   if (!objectId) {
      return;
   }

   // REST url to retrieve a list of properties
   var dataUrl = ns.buildDataUrl(objectId,
         ["name", "overallStatus", "hardware.systemInfo.model"]);

   // JQuery call to the DataAccessController in the Java plugin
   $.getJSON(dataUrl, function(data) {
      // data object contains the properties listed above
      var color = data.overallStatus;
      $("h3").text("Host Info for " + data.name);
      $("#hostInfo").html(
            "<p><b>Status: </b><span style='color:"+color+"'>" +data.overallStatus +
            "</span></p><p><b>Model: </b>" + data['hardware.systemInfo.model'] + "</p>");
   })
   .fail(function(jqXHR, status, error) {
      var response = jqXHR.responseJSON;
      alert( "Error: " + response.message +
            "\nCause: " + response.cause);
   });

});
