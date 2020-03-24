"use strict";

const globalChatRoomName = "General";
var currentGroup = "";


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

connection.on("ReceiveMessagesOfGroup", function(groupName, msgList) {
    console.log("ReceiveMessageOfGroup " + groupName + " " + msgList);
    if (currentGroup == groupName) {
        ReloadMessages(msgList);
    }
});

connection.on("ReceiveUsersInGroup", function(groupName, usersList) {
    console.log("userslist: " + usersList);
    if (currentGroup == groupName) {
        ReloadUsersInGroup(usersList);
    }
});

connection.on("ReceiveGroupsList", function(groupsList) {
    console.log("in ReceiveGroupsList: " + groupsList);
    ReloadGroupsList(groupsList);
});
// klienseknek kikuldeni az uj messageket (meg etc 2-3 command: onUserJoined, onUserLeft, onNewMessages, onGroupCreated, etc)

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

    // get online users in group
    connection.invoke("OnGetUsersInGroup", currentGroup).catch(function(err) {
        return console.error(err.toString);
    });

    // get list of groups
    connection.invoke("OnGetGroupsList").catch(function(err) {
        return console.error(err.toString);
    });
}

function ReloadUsersInGroup(usersList) {
    var parsed = JSON.parse(usersList);
    for (var i = 0; i < parsed.length; i++) {
        console.log(i);
        var div = document.createElement("div");
        div.innerHTML = parsed[i].Name;
        document.getElementById("divUsersInCurrentGroup").appendChild(div);
    }
}

function ReloadGroupsList(groupsList) {
    var parsed = JSON.parse(groupsList);
    for (var i = 0; i < parsed.length; i++) {
        var div = document.createElement("div");
        div.innerHTML = parsed[i].Name;
        document.getElementById("divGroups").appendChild(div);
    }
}

function ReloadMessages(messages) {
    console.log("messages: " + messages);
    document.getElementById("divMessages").innerHTML = "";
    var parsed = JSON.parse(messages);
    for (var i = 0; i < parsed.length; i++) {
        var msg = "[" + parsed[i].AuthorId + "]: " + parsed[i].MessageBody;
        AppendMessage(msg)
    }
}

function AppendMessage(msg) {
    var div = document.createElement("div");
    div.innerHTML = msg + "<hr />";
    document.getElementById("divMessages").appendChild(div);
}