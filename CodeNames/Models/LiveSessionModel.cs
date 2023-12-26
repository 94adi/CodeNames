using Microsoft.AspNetCore.Identity;
using System.Media;

namespace CodeNames.Models
{
    public class LiveSessionModel
    {
        public IList<IdentityUser> PlayersList { get; set; }
        public IDictionary<Team, IdentityUser> RedTeam { get; set; }
        public IDictionary<Team, IdentityUser> BlueTeam { get; set; }
        public GameRoom GameRoom { get; set; }
        public GameState GameState { get; set; } = GameState.Init;
        public Grid Grid { get; set; }
        public string Clue { get; set; }
        public int NumberOfCardsTargeted { get; set; }

    }
}
