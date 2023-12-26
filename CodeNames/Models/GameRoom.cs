using System.ComponentModel.DataAnnotations;

namespace CodeNames.Models
{
    public class GameRoom
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int? MaxNoPlayers { get; set; }
    }
}
