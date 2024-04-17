namespace CodeNames.Models
{
    public class Team 
    {
        public Color Color { get; set; }
        public string Name { get; set; } = "";
        public IList<GameLog> GameLog { get; set; } = new List<GameLog>();
        public IList<SessionUser> Players { get; set; } = new List<SessionUser>();
        public int NumberOfActiveCards { get; set; }
    }
}
