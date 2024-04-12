var connection = new signalR.HubConnectionBuilder()
                            .withUrl("/hubs/stateMachineHub")
                            //.withAutomaticReconnect([0, 1000, 5000, null])
                            .build();




window.addEventListener('DOMContentLoaded', (event) => {
    connection.start().then(fullfilled, rejected);

});

connection.on("InvalidSession", () => {
    //redirect to invalid session page
    alert("Invalid session");
});

connection.on("GameSessionStart", () => {
        //verify if user can join red/blue team
});

connection.on("RefreshIdlePlayersList", (jsonData) => {
    let idlePlayers = JSON.parse(jsonData);

    updateIdlePlayersList(idlePlayers);
});

function fullfilled() {
    let sessionId = $("#LiveSession_SessionId").val();
    connection.invoke("ReceiveSessionId", sessionId);
}

function rejected() {
    console.log("failed to connect");
}

function updateIdlePlayersList(idlePlayers) {
    let isArrayEmpty = (!idlePlayers || (!Array.isArray(idlePlayers)) || idlePlayers.length === 0);
    
    if (!isArrayEmpty) {
        let idlePlayersUl = document.getElementById("idle-players-ul");
        idlePlayersUl.innerHTML = null;
        idlePlayers.forEach((item, index) => {
            let li = document.createElement("li");
            li.appendChild(document.createTextNode("#" + (++index) + " " + item.Name));
            li.setAttribute('class', 'list-group-item text-center');
            li.setAttribute('style', 'background-color:#E3963E');
            idlePlayersUl.appendChild(li);     
        });
    }
}
