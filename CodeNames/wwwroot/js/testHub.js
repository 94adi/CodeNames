var connection = new signalR.HubConnectionBuilder()
                            .withUrl("/hubs/testHub", signalR.HttpTransportType.WebSockets)
                            .build();


connection.on("updateTotalUsers", (totalUsers) => {

    document.getElementById("userCount").innerText = totalUsers;
});


function fulfilled() {
    console.log("successfully connected!");
    //connection.invoke("")
}

function rejected() {
    console.log("failed to connect");
}

connection.start().then(fulfilled, rejected);