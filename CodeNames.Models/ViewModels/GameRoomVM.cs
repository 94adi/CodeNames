namespace CodeNames.Models.ViewModels
{
    public class GameRoomVM
    {
        public string PageTitle {  get; set; }
        public string ActionButtonName { get; set; }
        public GameRoom GameRoom { get; set; }
        public int? GameRoomId { get; set; }
    }
}
