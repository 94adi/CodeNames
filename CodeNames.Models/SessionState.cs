namespace CodeNames.Models
{
    public enum SessionState : Byte
    {
        Pending,
        Init, //blue/red team joins groups, choose spymaster
        Start, //blue will start first
        SpymasterRed,
        GuessRed,
        SpymasterBlue,
        GuessBlue,
        Finished,
        Failed
    }
}
