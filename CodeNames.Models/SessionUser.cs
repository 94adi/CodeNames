namespace CodeNames.Models;

public class SessionUser
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ConnectionId {  get; set; }
    public bool IsSpymaster { get; set; } = false;
    public Color TeamColor { get; set; }
    public UserStatus UserStatus { get; set; }
}
