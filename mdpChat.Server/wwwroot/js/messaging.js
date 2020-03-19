"use strict";

var connection = new signalR.HubConnectionBuilder()
                        .withUrl("/ChatHub")
                        .withAutomaticReconnect()
                        .build();

connection.onreconnecting((error) => {
    console.log("SignalR attempting to reconnect...");
})                        

connection.on("ReceiveMessage", function(msg) {
    AppendMessage(msg);
});

connection.start().then(function() {
    console.log('Connected!');
}).catch(function(err) {
    return console.error(err.toString());
})

document.getElementById("txtComposeMessage").onkeydown = function(event) {
    event = event || window.event;
    if (event.keyCode == 13) {  // 13: ENTER
        var msg = document.getElementById("txtComposeMessage").value;
        connection.invoke("SendMessageToAll", msg).catch(function(err) {
            return console.error(err.toString);
        });
        document.getElementById("txtComposeMessage").value = "";
    }
}

function AppendMessage(msg) {
    var div = document.createElement("div");
    div.innerHTML = msg + "<hr />";
    document.getElementById("divMessages").appendChild(div);
}