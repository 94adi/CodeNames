namespace CodeNames.Models
{
    public class Team 
    {
        public Color Color { get; set; }
        public string Name { get; set; } = "";
        public IList<GameLog> GameLog { get; set; }
        public IList<SessionUser> Players { get; set; }
        public int NumberOfActiveCards { get; set; }
    }
}
