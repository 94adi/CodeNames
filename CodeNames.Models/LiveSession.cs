using Microsoft.AspNet.Identity.EntityFramework;
using System.Media;

namespace CodeNames.Models
{
    //1 live session -> 1 game room
    public class LiveSession
    {
        public IList<IdentityUser> PlayersList { get; set; } = new List<IdentityUser>();
        public IList<Team> Teams { get; set; } = new List<Team>();
        public IList<IdentityUser> IdlePlayers { get; set; } = new List<IdentityUser>();
        public GameRoom GameRoom { get; set; }
        public GameState GameState { get; set; } = GameState.Init;
        public Grid Grid { get; set; }
        public List<List<Block>> GridMatrix { get; set; }
        public Clue Clue { get; set; }
        //public int NumberOfCardsTargeted { get; set; }
        public IDictionary<Team, int> NoOfCards { get;set; } = new Dictionary<Team, int>();
    }
}
