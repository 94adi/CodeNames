var connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/stateMachineHub")
    //.withAutomaticReconnect([0, 1000, 5000, null])
    .build();

var colorToListDictionary = { Blue: "blue-team-ul", Red: "red-team-ul", Idle: "idle-players-ul" };
var colorToBtnDictionary = { Blue: "blueTeamJoinBtn", Red: "redTeamJoinBtn" };
var idlePlayerIdPrefix = "IdlePlayer-";
var sessionId = null;
window.addEventListener('DOMContentLoaded', (event) => {
    connection.start().then(fullfilled, rejected);
});

connection.on("InvalidSession", () => {
    //redirect to invalid session page
    alert("Invalid session");
});

connection.on("GameSessionStart", () => {
    //verify if user can join red/blue team
    //make join team button visible
    //document.getElementsByClassName('blue-team-join-div').

});

connection.on("RefreshIdlePlayersList", (jsonData) => {
    let idlePlayers = JSON.parse(jsonData);

    updatePlayersList("Idle", idlePlayers);
});

connection.on("RefreshTeamPlayer", (teamColor, selectedTeamJson) => {
    let selectedTeam = JSON.parse(selectedTeamJson);

    updatePlayersList(teamColor, selectedTeam);
});

connection.on("ChangeJoinButtonToSpymaster", (btnColor) => {

    let spyMasterBtnId = btnColor + 'Spymaster';
    let spyMasterBtnDiv = btnColor + '-spymaster-join-div';
    $('.' + spyMasterBtnDiv).removeClass('d-none');
    document.getElementById(spyMasterBtnId).addEventListener("click", function () {
        $('.' + spyMasterBtnDiv).addClass('d-none');
        //handle user registration to be spymaster
        connection.invoke("TransformUserToSpymaster", sessionId, btnColor);
    });
});

connection.on("ChangeViewToSpymaster", (cardsToReveal) => {
    let cardsToRevealArray = JSON.parse(cardsToReveal);
    let isArrayEmpty = (!cardsToRevealArray || (!Array.isArray(cardsToRevealArray)) || cardsToRevealArray.length === 0);

    if (isArrayEmpty) return;

    cardsToRevealArray.forEach((v, i) => {
        let cardId = "#" + v.CardId;
        $(cardId).css("background-color", v.Color);
    });
});

function fullfilled() {
    sessionId = $("#LiveSession_SessionId").val();

    connection.invoke("ReceiveSessionId", sessionId);

    let blueTeamJoinBtn = $("#blueTeamJoinBtn");
    let redTeamJoinBtn = $("#redTeamJoinBtn");

    blueTeamJoinBtn.on("click", function (e) {
        e.preventDefault();
        let color = blueTeamJoinBtn.attr("data-value");
        $('.Blue-team-join-div').hide();
        $('.Red-team-join-div').hide();
        connection.invoke("UserJoinedTeam", sessionId, color);
    });

    redTeamJoinBtn.on("click", function (e) {
        e.preventDefault();
        let color = redTeamJoinBtn.attr("data-value");
        $('.Blue-team-join-div').hide();
        $('.Red-team-join-div').hide();
        connection.invoke("UserJoinedTeam", sessionId, color);
    });
}

function rejected() {
    console.log("failed to connect");
}

function removePlayerFromIdleList(playerId) {
    let li = document.getElementById(playerId);
    li.parentNode.removeChild(li);

}
function updatePlayersList(type, playersList) {

    let isArrayEmpty = (!playersList || (!Array.isArray(playersList)) || playersList.length === 0);

    if (isArrayEmpty) {
        let ul = document.getElementById(colorToListDictionary[type]);

        if (ul == null) return;

        ul.innerHTML = null;

        return;
    } 

    let ul = document.getElementById(colorToListDictionary[type]);

    if (ul == null) return;

    ul.innerHTML = null;
    playersList.forEach((item, index) => {
        let i = index;
        let li = document.createElement("li");
        li.appendChild(document.createTextNode("#" + (++i) + " " + item.Name));
        li.setAttribute('class', 'list-group-item text-center');
        li.setAttribute('style', 'background-color:#E3963E'); //TO DO: use bg color based on team color!
        ul.appendChild(li);
    });
}


