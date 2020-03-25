var connection = new signalR.HubConnectionBuilder()
                        .withUrl("/ChatHub")
                        .withAutomaticReconnect()
                        .build();

connection.start().then(function() {
    console.log('Connected!');
}).catch(function(err) {
    return console.error(err.toString());
});

connection.onreconnecting((error) => {
    console.log("SignalR attempting to reconnect...");
})                        

const globalChannelName = "General";
var userName = "";
var currentChannelName = "";

var channels = [];
var usersInCurrentChannel = [];
var messagesInCurrentChannel = [];

connection.on("LoginAccepted", function(userNameReceived) { 
    userName = userNameReceived; 
    renderPage();
});

connection.on("ReceiveUserList", function(channel, userList) {
    if (currentChannelName == channel) {
        usersInCurrentChannel = [];
        for (var i = 0; i < userList.length; i++) {
            usersInCurrentChannel.push(userList[i]);
        }
        renderPage();
    }
});

connection.on("ReceiveChannelList", function(channelList) {
    channels = [];
    for (var i = 0; i < channelList.length; i++) {
        channels.push(channelList[i]);
    }
    renderPage();
});

connection.on("ReceiveMessageList", function(channel, msgList) {
    if (currentChannelName == channel) {
        messagesInCurrentChannel = [];
        for (var i = 0; i < msgList.length; i++) {
            messagesInCurrentChannel.push(msgList[i]);
        }
        renderPage();
    }
})

connection.on("UserJoined", function(channel, user) {
    if (currentChannelName == channel) {
        usersInCurrentChannel.push(user);
        renderPage();
    }
});

connection.on("UserLeft", function(channel, user) {
    if (currentChannelName == channel) {
        var indexToRemove = usersInCurrentChannel.indexOf(user);
        if (indexToRemove > -1) {
            usersInCurrentChannel.splice(indexToRemove, 1);
            renderPage();
        }
    }
});

connection.on("ChannelCreated", function(channel) {
    channels.push(group);
    renderPage();
});

connection.on("ChannelDeleted", function(channel) {
    var indexToRemove = channels.indexOf(channel);
    if (indexToRemove > -1) {
        channels.splice(indexToRemove, 1);
        renderPage();
    }
});

connection.on("MessageReceived", function(channel, message) {
    if (currentChannelName == channel) {
        messagesInCurrentChannel.push(message);
        renderPage();
    }
});

// document.getElementById("txtComposeMessage").onkeydown = function(event) {
//     event = event || window.event;
//     if (event.keyCode == 13) {  // 13: ENTER
//         var msg = document.getElementById("txtComposeMessage").value;
//         connection.invoke("OnSendMessageToGroup", currentGroup, msg).catch(function(err) {
//             return console.error(err.toString);
//         });
//         document.getElementById("txtComposeMessage").value = "";
//     }
// }

document.getElementById("txtLogin").onkeydown = function(event) {
    event = event || window.event;
    if (event.keyCode == 13) {  // 13: ENTER
        var msg = document.getElementById("txtLogin").value;
        connection.invoke("OnLogin", msg).catch(function(err) {
            return console.error(err.toString);
        });
        document.getElementById("txtLogin").disabled = true;
        currentChannelName = globalChannelName;
        // renderPage(); ???
    }
}


function renderPage() {
    var divMain = document.getElementById("divMain");
    // divMain.className = "centered"; // defined in cshtml tag
    divMain.innerHTML = "";

    var divChat = document.createElement("div");
    divChat.id = "divChat";
    divChat.className = "centered";

    var divHeader = document.createElement("div");
        var divUserName = document.createElement("div");
        var divCurrentChannel = document.createElement("div");
        var divUserListLabel = document.createElement("div");
    divHeader.id = "divHeader";
        divUserName.id = "divUserName";
        divCurrentChannel.id = "divCurrentChannel";
        divUserListLabel.id = "divUserListLabel";
    divHeader.appendChild(divUserName);
    divHeader.appendChild(divCurrentChannel);
    divHeader.appendChild(divUserListLabel);

    var divMid = document.createElement("div");
        var divChannelList = document.createElement("div");
        var divMessages = document.createElement("div");
        var divUserList = document.createElement("div");
    divMid.id = "divMid";
        divChannelList.id = "divChannelList";
        divMessages.id = "divMessages";
        divUserList.id = "divUserList";
    divMid.appendChild(divChannelList);
    divMid.appendChild(divMessages);
    divMid.appendChild(divUserList);

    var divFooter = document.createElement("div");
    divFooter.id = "divFooter";
    divFooter.className = "centered";

    divChat.appendChild(divHeader);
    divChat.appendChild(divMid);
    divChat.appendChild(divFooter);

    divMain.appendChild(divChat);

    divUserName.innerHTML = userName; // "mdpChat";
    divCurrentChannel.innerHTML = "ChannelName";
    divUserListLabel.innerHTML = "UsersInGroup";

    divChannelList.innerHTML = "Channels";
    divMessages.innerHTML = "Messages";
    divUserList.innerHTML = "Users";

    var txtMessage = document.createElement("input");
    txtMessage.type = "text";
    txtMessage.id = "txtMessage";
    divFooter.appendChild(txtMessage);
}
