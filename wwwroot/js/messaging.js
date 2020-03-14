"use strict";

var userName = "";

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

connection.on("ReceiveUserName", function(msg) {
    userName = msg;
});

connection.start().then(function() {
    console.log('Connected!');
    connection.invoke("RequestUserName").catch(function(err) {
        return console.error(err.toString());
    });
    event.preventDefault();
}).catch(function(err) {
    return console.error(err.toString());
})

document.getElementById("txtComposeMessage").onkeydown = function(event) {
    event = event || window.event;
    if (event.keyCode == 13) {  // 13: ENTER
        var msg = document.getElementById("txtComposeMessage").value;
        SendMessageToServer(msg);
        document.getElementById("txtComposeMessage").value = "";
    }
}

function SendMessageToServer(msg) {
    if (userName) {
        connection.invoke("SendMessageToAll", userName, msg).catch(function(err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    } else {
        console.log("UserName is not set.");
    }
}

function AppendMessage(msg) {
    var div = document.createElement("div");
    div.innerHTML = msg + "<hr />";
    document.getElementById("divMessages").appendChild(div);
}