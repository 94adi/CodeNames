var connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/stateMachineHub")
    .withAutomaticReconnect([0, 1000, 5000, null])
    .build();

var colorToListDictionary = { Blue: "blue-team-ul", Red: "red-team-ul", Idle: "idle-players-ul" };
var colorToBtnDictionary = { Blue: "blueTeamJoinBtn", Red: "redTeamJoinBtn" };
var idlePlayerIdPrefix = "IdlePlayer-";
var sessionId = null;
var guessButtons = null;

window.addEventListener('DOMContentLoaded', (event) => {
    connection.start().then(fullfilled, rejected);

    document.getElementById("clueSubmitForm").addEventListener("submit", (e) => {
        e.preventDefault();      
        let clue = $("#inputClue").val();
        let noCardsTarget = $("#inputNumberTarget").val();
        let formData = { Clue: clue, NoCardsTarget: noCardsTarget };
        console.log(formData);
        clueSubmitFormHandler(formData)
    });

    document.getElementById("btnStartGame").addEventListener("click", (e) => {
        e.preventDefault();
        console.log("clicked start game");
        connection.invoke("StartGame", sessionId);
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

connection.on("ChangeJoinButtonToSpymaster", (btnColor) => {

    let spyMasterBtnId = btnColor + 'SpymasterBtn';
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

    //remove become spymaster button for team

});

connection.on("RemoveSpymasterButton", (teamColor) => {
    let id = "#" + teamColor + "SpymasterBtn";
    console.log(id);
    $(id).hide();
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
        btn.addClass('disabled');
        //btn.removeClass('disabled');
        console.log("enabled buttons");
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

connection.on("CardGuess", (row, col, color) => {

    let cardId = "cardAt-" + row + "-" + col;
    document.getElementById(cardId).style.backgroundColor = color;
    //$(cardId).css('bakcground-color', color);

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

function clueSubmitFormHandler(data) {

    let clue = data.Clue;
    let noCardsTarget = data.NoCardsTarget;

    connection.invoke("SpymasterSubmitGuess", sessionId, clue, noCardsTarget);
}


