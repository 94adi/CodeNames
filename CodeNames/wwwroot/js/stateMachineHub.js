import { openModal } from './modal.js';

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/stateMachineHub")
    .withAutomaticReconnect([0, 1000, 5000, null])
    .build();

var colorToListDictionary = { Blue: "blue-team-ul", Red: "red-team-ul", Idle: "idle-players-ul" };
var colorToBtnDictionary = { Blue: "blueTeamJoinBtn", Red: "redTeamJoinBtn" };
var idlePlayerIdPrefix = "IdlePlayer-";
var sessionId = null;
var guessButtons = null;

window.addEventListener('load', (event) => {
    connection.start().then(fullfilled, rejected);

    document.getElementById("clueSubmitForm").addEventListener("submit", (e) => {
        e.preventDefault();      
        let clue = $("#inputClue").val();
        let noCardsTarget = $("#inputNumberTarget").val();
        let formData = { Clue: clue, NoCardsTarget: noCardsTarget };
        console.log(formData);
        clueSubmitFormHandler(formData)
    });

    guessButtons = document.querySelectorAll(".guess-btn");

    guessButtons.forEach((btn) => {
        btn.addEventListener("click", (e) => {
            
            let divId = e.target.parentNode.parentNode.getAttribute('id');
            let coordinates = divId.split('-');

            let buttonText = e.target.getAttribute('value');

            let row = coordinates[1];
            let col = coordinates[2];

            connection.invoke("PlayerSubmitGuess", sessionId, row, col);

        });
    });

});

connection.on("InvalidSession", () => {
    alert("Invalid session");
});

connection.on("GameSessionStart", () => {

});

connection.on("RefreshIdlePlayersList", (jsonData) => {
    let idlePlayers = JSON.parse(jsonData);

    updatePlayersList("Idle", idlePlayers);
});

connection.on("RefreshTeamPlayer", (teamColor, selectedTeamJson) => {
    let selectedTeam = JSON.parse(selectedTeamJson);

    updatePlayersList(teamColor, selectedTeam);
});

connection.on("ChangeJoinButtonToSpymaster", (btnColor) =>
{
    let spyMasterBtnId = btnColor + 'SpymasterBtn';

    toggleSpymasterButton(btnColor, true);
    toggleJoinTeamButton(false);
    addSpymasterHandlerLogic(spyMasterBtnId, btnColor);
});

connection.on("AddSpymasterHandlerToBtn", (btnColor) => {
    let spyMasterBtnId = btnColor + 'SpymasterBtn';
    addSpymasterHandlerLogic(spyMasterBtnId, btnColor);
});

connection.on("HideSpymasterButton", (btnColor) => {
    toggleSpymasterButton(btnColor, false);
});

connection.on("HideJoinTeamButton", (btnColor) => {

    toggleJoinTeamButton(false);

});

connection.on("ChangeViewToSpymaster", (cardsToReveal, teamColor) =>
{
    let cardsToRevealArray = JSON.parse(cardsToReveal);
    let isArrayEmpty = (!cardsToRevealArray || (!Array.isArray(cardsToRevealArray)) || cardsToRevealArray.length === 0);

    if (isArrayEmpty) return;

    cardsToRevealArray.forEach((v, i) => {
        let cardId = "#" + v.CardId;
        $(cardId).css("background-color", v.Color);
    });

    toggleSpymasterButton(teamColor, false);
});

connection.on("ReceivedSpymasterClue", (clue) => {
    let word = clue.word;
    let noOfCards = clue.noOfCards;
    $('#displayBanner').text('Word: ' + word + ' | ' + 'Number of targeted cards: ' + noOfCards);
});

connection.on("AwaitingSpymasterState", (color)=> {
    $('#gamePanel').css('background-color', color);
    $('#btnStartGame').hide();
});

connection.on("SpyMasterMode", (color) => {
    console.log('SpyMasterMode');
    $('#btnStartGame').hide();
    $('#gamePanel').css('background-color', color);
    $("#clueSubmitForm").removeClass('d-none');
});

connection.on("HideSpymasterGuessForm", () => {
    console.log("reset spymaster form");
    document.getElementById("clueSubmitForm").reset();
    $("#clueSubmitForm").addClass('d-none');

});

connection.on("ActivateCards", () => {
    guessButtons.forEach((btn) => {
        btn.classList.remove('disabled');
        //btn.removeClass('disabled');
        console.log("enabled buttons");
    })
});

connection.on("DeactivateCards", () => {
    guessButtons.forEach((btn) => {
        btn.classList.addClass('disabled');
        //btn.addClass('disabled');
    })
});

connection.on("GameLost", () => {
    $('#displayBanner').text('');
    $('#displayBanner').text('Your team lost the game :(');
});

connection.on("GameWon", () => {
    $('#displayBanner').text('');
    $('#displayBanner').text('Your team won the game :)');
});

connection.on("GameFailureSignal", () => {
    $('#displayBanner').text('');
    $('#displayBanner').text('GAME FAILURE');
    //alert('Game ended because one of the spymaster left the game :(');
})

connection.on("EndSession", () => {
    //connection.stop();
});

connection.on("CardGuess", (row, col, color) => {

    let cardId = "cardAt-" + row + "-" + col;
    document.getElementById(cardId).style.backgroundColor = color;
    //$(cardId).css('bakcground-color', color);
});

connection.on("OpenSpymasterModal", (color) => {
    openSpymasterModal(color);
});

connection.on("OpenGameOverModal", (color) => {
    openGameOverModal(color);
});


function fullfilled() {

    sessionId = $("#LiveSession_SessionId").val();

    connection.invoke("ReceiveSessionId", sessionId);

    let blueTeamJoinBtn = $("#blueTeamJoinBtn");
    let redTeamJoinBtn = $("#redTeamJoinBtn");

    blueTeamJoinBtn.on("click", function (e) {
        e.preventDefault();
        let color = blueTeamJoinBtn.attr("data-value");
        toggleJoinTeamButton(false);
        connection.invoke("UserJoinedTeam", sessionId, color);
    });

    redTeamJoinBtn.on("click", function (e) {
        e.preventDefault();
        let color = redTeamJoinBtn.attr("data-value");
        toggleJoinTeamButton(false);
        connection.invoke("UserJoinedTeam", sessionId, color);
    });
}

function rejected() {
    console.log("failed to connect");
}

function toggleSpymasterButton(btnColor, show = false) {
    let spyMasterBtnDiv = btnColor + '-spymaster-join-div';

    if (show) {
        $('.' + spyMasterBtnDiv).removeClass('d-none');
    }
    else {
        $('.' + spyMasterBtnDiv).addClass('d-none');
    }
}

function toggleJoinTeamButton(btnColor, show = false)
{
    let joinRedTeam = $('.Red-team-join-div');
    let joinBlueTeam = $('.Blue-team-join-div');

    if (show)
    {
        joinRedTeam.removeClass('d-none');
        joinBlueTeam.removeClass('d-none');
    }
    else
    {
        joinRedTeam.addClass('d-none');
        joinBlueTeam.addClass('d-none');
    }
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
        console.log("team color: " + item.TeamColor);
        let i = index;
        let li = document.createElement("li");
        li.appendChild(document.createTextNode("#" + (++i) + " " + item.Name + "(" + item.UserStatus + ")"));
        li.setAttribute('class', 'list-group-item text-center');
        li.setAttribute('style', 'background-color:' + item.TeamColor);
        ul.appendChild(li);
    });
}

function clueSubmitFormHandler(data) {

    let clue = data.Clue;
    let noCardsTarget = data.NoCardsTarget;

    connection.invoke("SpymasterSubmitGuess", sessionId, clue, noCardsTarget);
}

function addSpymasterHandlerLogic(btnId, btnColor)
{
    document.getElementById(btnId).addEventListener("click", function () {
        toggleSpymasterButton(btnColor, false);

        connection.invoke("TransformUserToSpymaster", sessionId, btnColor);
    });
}

function openSpymasterModal(spymasterColor) {
    let title = spymasterColor + " Spymaster left";

    openModal("modalSpymasterLeft",
        title,
        "The game had to end as a result of that action");
}

function openGameOverModal(teamWon) {
    openModal("modalGameOver",
    "Game over",
    teamWon + " won the game!",)
}