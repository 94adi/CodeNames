namespace CodeNames.Models;

public enum StateTransition : byte
{
    NONE,
    GAME_START,
    TEAM_GUESSED_ALL_CARDS,
    TEAM_CHOSE_BLACK_CARD,
    TEAM_GUESSED_ALL_OPPONENT_CARDS,
    TEAM_RAN_OUT_OF_GUESSES
}
