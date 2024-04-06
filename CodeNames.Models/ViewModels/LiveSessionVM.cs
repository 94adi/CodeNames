using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNames.Models.ViewModels
{
    public class LiveSessionVM
    {
        public LiveSession LiveSession { get; set; }

        public IDictionary<Color, Team> Teams { get; set; } = new Dictionary<Color, Team>();
    }
}
