using Microsoft.AspNetCore.Identity;
using System.Drawing;

namespace CodeNames.Models
{
    public class Team 
    {
        public Color Color { get; set; }
        public string Name { get; set; } = "";
        public IList<IdentityUser> Players { get; set; }
    }
}
