namespace CodeNames.Models
{
    public enum GameState : Byte
    {
        Init, //blue/red team joins groups, choose spymaster
        Start, //blue will start first
        Red,
        Blue,
        Finished
    }
}
