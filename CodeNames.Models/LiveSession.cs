using Microsoft.AspNet.Identity.EntityFramework;
using System.Media;

namespace CodeNames.Models
{
    //1 live session -> 1 game room
    public class LiveSession
    {
        public Guid SessionId { get; set; }
        public IDictionary<string,string> PlayersList { get; set; } = new Dictionary<string, string>();
        public IList<Team> Teams { get; set; } = new List<Team>();
        public IDictionary<string, string> IdlePlayers { get; set; } = new Dictionary<string, string>();
        public GameRoom GameRoom { get; set; }
        public SessionState SessionState { get; set; } = SessionState.Init;
        public Grid Grid { get; set; }
        public List<List<Block>> GridMatrix { get; set; }
        public Clue Clue { get; set; }
        //public int NumberOfCardsTargeted { get; set; }
        public IDictionary<Team, int> NoOfCards { get;set; } = new Dictionary<Team, int>();
        
        public LiveSession()
        {
            SessionId = Guid.NewGuid();
        }
    }
}
