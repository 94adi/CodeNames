using CodeNames.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodeNames.Controllers
{
    public class GameController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LiveSession(string sessionRoom)
        {
            //use IHubContext to inject Hub into controller and communicate with the clients in a group
            //use the ping pong between Server and Client to transition states, source of truth (current state) should be on the server
            LiveSessionModel model = new LiveSessionModel();
            return View(model);
        }

        private void PopulateLiveSessionModel(LiveSessionModel model)
        {
            model.GameState = GameState.Init;
            model.RedTeam = new Dictionary<Team, IdentityUser>();
            model.BlueTeam = new Dictionary<Team, IdentityUser>();
            //model.GameRoom = use GameRoom repo and retrieve it by sessionRoom name (or maybe guid)
            //
        }
    }
}
