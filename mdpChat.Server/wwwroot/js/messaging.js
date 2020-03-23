"use strict";

const globalChatRoomName = "General";
var currentGroup;

class ServerUpdate {
    usersInGeneralChat;    
}


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

connection.on("ReceiveMessageFromGroup", function(authorName, groupName, msg) {
    AppendMessage("[" + groupName + "] " + authorName + ": " + msg);
});

connection.start().then(function() {
    console.log('Connected!');
}).catch(function(err) {
    return console.error(err.toString());
});

document.getElementById("txtComposeMessage").onkeydown = function(event) {
    event = event || window.event;
    if (event.keyCode == 13) {  // 13: ENTER
        var msg = document.getElementById("txtComposeMessage").value;
        connection.invoke("OnSendMessageToGroup", currentGroup, msg).catch(function(err) {
            return console.error(err.toString);
        });
        document.getElementById("txtComposeMessage").value = "";
    }
}

document.getElementById("txtLoginUserName").onkeydown = function(event) {
    event = event || window.event;
    if (event.keyCode == 13) {  // 13: ENTER
        var msg = document.getElementById("txtLoginUserName").value;
        connection.invoke("OnLogIn", msg).catch(function(err) {
            return console.error(err.toString);
        });
        document.getElementById("txtLoginUserName").disabled = true;
        currentGroup = globalChatRoomName;
        LoadInformation();
    }
}

function LoadInformation() {
    document.getElementById("divCurrentRoomName").innerHTML = currentGroup;
}

function AppendMessage(msg) {
    var div = document.createElement("div");
    div.innerHTML = msg + "<hr />";
    document.getElementById("divMessages").appendChild(div);
}