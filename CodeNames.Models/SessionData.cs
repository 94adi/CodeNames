namespace CodeNames.Models;

public record SessionData(
    Guid SessionId,
    SessionUser Player,
    Team PlayerTeam,
    Team OtherTeam,
    string[] TeamIds,
    string[] OtherTeamIds,
    Card GuessedCard,
    int Row,
    int Col);

