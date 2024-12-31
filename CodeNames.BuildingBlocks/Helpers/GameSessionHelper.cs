using CodeNames.Models;

namespace CodeNames.BuildingBlocks.Helpers;

public static class GameSessionHelper
{
    public static bool IsGameOngoing(LiveSession session)
    {
        bool result = ((session.SessionState == SessionState.SPYMASTER_BLUE) ||
            (session.SessionState == SessionState.GUESS_BLUE) ||
            (session.SessionState == SessionState.SPYMASTER_RED) ||
            (session.SessionState == SessionState.GUESS_RED));

        return result;
    }
}
