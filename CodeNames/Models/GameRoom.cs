using System.ComponentModel.DataAnnotations;

namespace CodeNames.Models
{
    public class GameRoom
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int? MaxNoPlayers { get; set; }
        public Guid InvitationCode { get; set; }
    }
}
