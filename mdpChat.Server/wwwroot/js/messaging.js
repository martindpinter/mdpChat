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

connection.on("LoginAccepted", function(userNameReceived, channelList) { 
    userName = userNameReceived; 
    channels = channelList;
    // channels = JSON.parse(channelList);
    renderPage();
});

connection.on("GroupJoined", function(groupName, usersInGroup, msgList) {
    currentChannelName = groupName;
    messagesInCurrentChannel = msgList;
    usersInCurrentChannel = usersInGroup;
    renderPage();
});

connection.on("ReceiveUserList", function(channel, userList) { 
    if (currentChannelName == channel) {
        usersInCurrentChannel = JSON.parse(userList);
        renderPage();
    }
});

connection.on("GroupChangeApproved", function(newGroup, msgList, userList) {
    currentChannelName = newGroup;
    messagesInCurrentChannel = msgList;
    usersInCurrentChannel = userList;
    renderPage();
});

connection.on("ReceiveMessageList", function(channel, msgList) {
    if (currentChannelName == channel) {
        messagesInCurrentChannel = [];
        var deserialized = JSON.parse(msgList);
        for (var i = 0; i < deserialized.length; i++) {
            messagesInCurrentChannel.push(deserialized[i]);
        }
        renderPage();
    }
})

connection.on("UserJoinedChannel", function(channel, user) {
    if (currentChannelName == channel) {
        usersInCurrentChannel.push(user);
        renderPage();
    }
});

connection.on("UserLeftChannel", function(channel, user) {
    if (currentChannelName == channel) {
        var indexToRemove = usersInCurrentChannel.indexOf(user);
        if (indexToRemove > -1) {
            usersInCurrentChannel.splice(indexToRemove, 1);
            renderPage();
        }
    }
});

connection.on("UserDisconnected", function(user) {
    var indexToRemove = usersInCurrentChannel.indexOf(user);
    if (indexToRemove > -1) {
        usersInCurrentChannel.splice(indexToRemove, 1);
        renderPage();
    }
});

connection.on("GroupCreated", function(group) {
    channels.push(group);
    renderPage();
});

connection.on("GroupDeleted", function(channel) {
    var indexToRemove = channels.indexOf(channel);
    if (indexToRemove > -1) {
        channels.splice(indexToRemove, 1);
        renderPage();
    }
});

connection.on("MessageReceived", function(channel, message) {
    console.log(currentChannelName + " " + channel);
    if (currentChannelName == channel) {
        messagesInCurrentChannel.push(message);
        console.log("render from MessageReceived");
        renderPage();
    }
    console.log("messagesInCurrentChannel: " + JSON.parse(messagesInCurrentChannel));
});

document.getElementById("txtLogin").onkeydown = function(event) {
    event = event || window.event;
    if (event.keyCode == 13) {  // 13: ENTER
        var msg = document.getElementById("txtLogin").value;
        connection.invoke("OnLogin", msg).catch(function(err) {
            return console.error(err.toString);
        });
        document.getElementById("txtLogin").disabled = true;
        currentChannelName = globalChannelName;
    }
}

function msgSendOnKeyDown(event) {
    event = event || window.event;
    if (event.keyCode == 13) {  // 13: ENTER
        var msg = document.getElementById("txtComposeMessage").value;
        connection.invoke("OnSendMessageToGroup", currentChannelName, msg).catch(function(err) {
            return console.error(err.toString);
        });
        document.getElementById("txtComposeMessage").value = "";
    }
}

function createNewGroup(event) {
    event = event || window.event;
    if (event.keyCode == 13) {
        var msg = document.getElementById("txtCreateGroup").value;
        connection.invoke("OnCreateGroup", msg).catch(function(err) {
            return console.error(err.toString);
        });
        document.getElementById("txtCreateGroup").value = "";
    }
}

function changeGroup(divGroupElement) {
    var newGroup = divGroupElement.innerHTML;
    connection.invoke("OnChangeGroup", newGroup).catch(function(err) {
        return console.error(err.toString);
    });
}

function leaveGroup() {
    connection.invoke("OnLeaveGroup", currentChannelName).catch(function(err) {
        return console.error(err.toString);
    });
    connection.invoke("OnChangeGroup", globalChannelName).catch(function(err) {
        return console.error(err.toString);
    });
}

function renderChannelList(divChannelList) {
    console.log("in renderChannelList, length: " + channels.length);
    if (channels) {
        divChannelList.innerHTML = "";
        for (var i = 0; i < channels.length; i++) {
            var div = document.createElement("div");
            div.innerHTML = channels[i];
            div.className = "handPointer";
            div.onclick = function(localDiv) { return function() { changeGroup(localDiv); } }(div);
            divChannelList.appendChild(div);
        }
        var txtCreateGroup = document.createElement("input");
        txtCreateGroup.type = "text";
        txtCreateGroup.id = "txtCreateGroup";
        txtCreateGroup.onkeydown = createNewGroup;
        txtCreateGroup.placeholder = "Create group";
        divChannelList.appendChild(txtCreateGroup);
    }
}

function renderMessages(divMessages) {
    if (messagesInCurrentChannel) {
        divMessages.innerHTML = "";
        for (var i = 0; i < messagesInCurrentChannel.length; i++) {
            var div = document.createElement("div");
            div.innerHTML = messagesInCurrentChannel[i].authorName + ": " + messagesInCurrentChannel[i].messageBody;
            divMessages.appendChild(div);
        }
    }
}

function renderUserList(divUserList) {
    if (usersInCurrentChannel) {
        usersInCurrentChannel = [...new Set(usersInCurrentChannel)].sort();
        divUserList.innerHTML = "";
        for (var i = 0; i < usersInCurrentChannel.length; i++) {
            var div = document.createElement("div");
            div.innerHTML = usersInCurrentChannel[i];
            divUserList.appendChild(div);
        }
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

    divUserName.innerHTML = "Logged in as <b><u>" + userName + "</u></b>"; 
    divCurrentChannel.innerHTML = currentChannelName;
    divCurrentChannel.className = "sideBySide";
    if (currentChannelName != globalChannelName) {
        var divLeave = document.createElement("div");
        divLeave.onclick = leaveGroup;
        divLeave.className = "handPointer";
        divLeave.innerHTML = "&nbsp;[X]";
        divCurrentChannel.appendChild(divLeave);
    }
    divUserListLabel.innerHTML = "Users In Current Group";

    divChannelList.innerHTML = "Channels";
    divMessages.innerHTML = "Messages";
    divUserList.innerHTML = "Users";

    var txtComposeMessage = document.createElement("input");
    txtComposeMessage.type = "text";
    txtComposeMessage.id = "txtComposeMessage";
    txtComposeMessage.placeholder = "Message";
    txtComposeMessage.onkeydown = msgSendOnKeyDown;
    divFooter.appendChild(txtComposeMessage);

    renderChannelList(divChannelList);
    renderMessages(divMessages);
    renderUserList(divUserList);
}