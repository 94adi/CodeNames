namespace CodeNames.Models
{
    //1 live session -> 1 game room
    public class LiveSession
    {
        public Guid SessionId { get; set; }
        public IList<SessionUser> PlayersList { get; set; } = new List<SessionUser>();
        public IList<Team> Teams { get; set; } = new List<Team>();
        public IList<SessionUser> IdlePlayers { get; set; } = new List<SessionUser>();
        public GameRoom GameRoom { get; set; }
        public SessionState SessionState { get; set; } = SessionState.PENDING;
        public Grid Grid { get; set; }
        public List<List<Card>> GridMatrix { get; set; }
        public Clue Clue { get; set; }
        public Color ActiveTeam { get; set; }
        public IDictionary<Color, int> NumberOfTeamActiveCards { get; set; }
        
        public LiveSession()
        {
            SessionId = Guid.NewGuid();
            ActiveTeam = Color.Neutral;
            NumberOfTeamActiveCards = new Dictionary<Color, int>();
        }
    }
}
