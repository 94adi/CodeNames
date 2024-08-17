namespace CodeNames.Models.ViewModels;

public class IndexVM
{

    public IndexVM()
    {
        GameRoomsInvitaionLinks = new Dictionary<int, string>();
    }

    public IEnumerable<GameRoom>? GameRooms { get; set; }
    public IDictionary<int, string> GameRoomsInvitaionLinks { get; set; }

}
