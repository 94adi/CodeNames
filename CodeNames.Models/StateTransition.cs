using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNames.Models
{
    public enum StateTransition : byte
    {
        NONE,
        GAME_START,
        TEAM_GUESSED_ALL_CARDS,
        TEAM_CHOSE_BLACK_CARD,
        TEAM_GUESSED_ALL_OPPONENT_CARDS
    }
}
