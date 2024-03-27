using Microsoft.AspNetCore.Identity;
using System.Media;

namespace CodeNames.Models
{
    //1 live session -> 1 game room
    public class LiveSession
    {
        //All the players in the current session
        public IList<IdentityUser> PlayersList { get; set; }
        public IDictionary<Team, IdentityUser> RedTeam { get; set; }
        public IDictionary<Team, IdentityUser> BlueTeam { get; set; }
        //playes who joined session but are not part of a team
        public IList<IdentityUser> IdlePlayers { get; set; }
        public GameRoom GameRoom { get; set; }
        public GameState GameState { get; set; } = GameState.Init;
        public Grid Grid { get; set; }
        public List<List<Block>> GridMatrix { get; set; }
        public Clue Clue { get; set; }
        public int NumberOfCardsTargeted { get; set; }
        public int NoOfRedCards { get; set; }
        public int NoOfBlueCards { get; set; }

    }
}
