var reg=/-/g;
$("h3,td,button,a").each(function(){
    var id = $(this).attr("id");
    if(id!=undefined){
	    id = id.replace(reg,'.');
	    $(this).text(ns.getString(id));
    }
});