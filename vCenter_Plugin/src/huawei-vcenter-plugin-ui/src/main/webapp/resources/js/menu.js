$("#menu_div").load("../menu.html", function (response, status, xhr) {
        $('#menu_div').html(response);
        $("#menu-eSight").text(ns.getString("menu.eSight"));
        $("#menu-server").text(ns.getString("menu.server"));
        $("#menu-server-list").text(ns.getString("menu.server.list"));
        $("#menu-server-power").text(ns.getString("menu.server.power"));
        $("#menu-server-sourceManage").text(ns.getString("menu.server.sourceManage"));
        $("#menu-server-moduleManage").text(ns.getString("menu.server.moduleManage"));
        $("#menu-server-taskManage").text(ns.getString("menu.server.taskManage"));
    });