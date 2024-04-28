namespace CodeNames.Models
{
    public enum SessionState : byte
    {
        UNKNOWN,
        PENDING,
        INIT, 
        START, 
        SPYMASTER_BLUE,
        GUESS_BLUE,
        SPYMASTER_RED,
        GUESS_RED,
        BLUE_WON,
        RED_WON,
        FAILURE
    }
}
