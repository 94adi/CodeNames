using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNames.Models
{
    public class LiveGameSession
    {
        [Key]
        public int Id { get;set; }

        public GameRoom GameRoom { get; set; }

        [ForeignKey("GameRoom")]
        public int GameRoomId { get; set; }

        public Guid SessionId {  get; set; }
    }
}
