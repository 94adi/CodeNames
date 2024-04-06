var connection = new signalR.HubConnectionBuilder()
                            .withUrl("/hubs/stateMachineHub")
                            //.withAutomaticReconnect([0, 1000, 5000, null])
                            .build();




window.addEventListener('DOMContentLoaded', (event) => {
    connection.start().then(fullfilled, rejected);

});

connection.on("InvalidSession", () => {
    //use toastr
    alert("Invalid session");
});

connection.on("GameSessionStart", (jsonData) => {
    //generate button to allow users to join team
    console.log(jsonData);
});



function fullfilled() {
    console.log("Connected!");
    let sessionId = $("#LiveSession_SessionId").val();
    connection.invoke("ReceiveSessionId", sessionId);
}

function rejected() {
    console.log("failed to connect");
}


