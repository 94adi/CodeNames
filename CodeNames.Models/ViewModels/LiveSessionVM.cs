using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNames.Models.ViewModels
{
    public class LiveSessionVM
    {
        public LiveSessionVM()
        {
            BackgroundColor = Color.InitialNeutralBackground;
            UserTeamColor = Color.None;
            IsUserSpymaster = false;
            HideRedSpymasterButton = false;
            HideBlueSpymasterButton = false;
            HideJoinRedTeamButton = false;
            HideJoinBlueTeamButton = false;
            InitialRun = true;
        }

        public LiveSession LiveSession { get; set; }

        public IDictionary<Color, Team> Teams { get; set; } = new Dictionary<Color, Team>();

        //Set this one according to the current state of the session
        public Color BackgroundColor { get; set; }

        public string UserId { get; set; }
        public Color UserTeamColor { get; set; }
        public bool IsUserSpymaster { get; set; }
        public bool HideRedSpymasterButton { get; set; }
        public bool HideBlueSpymasterButton { get; set; }
        public bool HideJoinRedTeamButton { get; set; }
        public bool HideJoinBlueTeamButton { get; set; }
        public bool InitialRun { get; set; }
    }
}
