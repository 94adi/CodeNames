namespace CodeNames.Models
{
    public enum SessionState : Byte
    {
        Pending,
        Init, //blue/red team joins groups, choose spymaster
        Start, //blue will start first
        Red,
        Blue,
        Finished,
        Failed
    }
}
