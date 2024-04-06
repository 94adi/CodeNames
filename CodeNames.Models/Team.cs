using Microsoft.AspNet.Identity.EntityFramework;
using System.Drawing;

namespace CodeNames.Models
{
    public class Team 
    {
        public Color Color { get; set; }
        public string Name { get; set; } = "";
        public IList<GameLog> GameLog { get; set; }
        public IDictionary<string, string> Players { get; set; }
    }
}
