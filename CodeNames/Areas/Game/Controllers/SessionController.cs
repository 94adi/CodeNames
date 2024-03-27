using CodeNames.CodeNames.Core.Services.GameRoomService;
using CodeNames.CodeNames.Core.Services.GridGenerator;
using CodeNames.Models;
using CodeNames.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CodeNames.Areas.Game.Controllers
{
    [Area("Game")]
    //[Authorize]
    public class SessionController : Controller
    {
        private readonly IGridGenerator _gridGeneratorService;
        private readonly IGameRoomService _gameRoomService;
        //private readonly IHubContext<> _gameHubContext;

        public SessionController(IGridGenerator gridGeneratorService,
            IGameRoomService gameRoomService)
        {
            _gridGeneratorService = gridGeneratorService;
            _gameRoomService = gameRoomService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult LiveSession(string gameRoom, string invitationCode)
        {
            if (string.IsNullOrEmpty(gameRoom) || string.IsNullOrEmpty(invitationCode))
            {
                //TO DO: Redirect to access forbidden page
                return RedirectToAction(nameof(Index));
            }

            LiveSession model = new LiveSession();

            model.GameRoom = _gameRoomService.GetRoomByName(gameRoom);

            if (model.GameRoom == null)
            {
                return NotFound();
            }

            var hasGameRoomaAccess = _gameRoomService.IsInvitationCodeValid(gameRoom, new Guid(invitationCode));

            if (!hasGameRoomaAccess)
            {
                return Forbid();
            }

            PopulateLiveSessionModel(model);

            ViewBag.ColorDictionary = StaticDetails.ColorToHexDict;

            return View(model);
        }

        public IActionResult LiveSessionTest()
        {
            LiveSession model = new LiveSession();

            PopulateLiveSessionModel(model);

            ViewBag.ColorDictionary = StaticDetails.ColorToHexDict;

            return View(model);
        }

        public IActionResult LiveSessionBootStrap()
        {
            LiveSession model = new LiveSession();

            PopulateLiveSessionModel(model);

            ViewBag.ColorDictionary = StaticDetails.ColorToHexDict;

            return View(model);
        }

        public IActionResult AccessForbidden(string gameRoom)
        {
            ViewBag.GameRoomName = gameRoom;
            return View();
        }

        private void PopulateLiveSessionModel(LiveSession model)
        {

            model.GameState = GameState.Init;
            model.RedTeam = new Dictionary<Team, IdentityUser>();
            model.BlueTeam = new Dictionary<Team, IdentityUser>();
            model.Grid = _gridGeneratorService.Generate();
        }

        private List<List<Block>> ConvertListToMatrix(Grid grid)
        {
            int rows = grid.Rows;
            int cols = grid.Columns;
            int counter = 0;
            List<List<Block>> result = new();

            for (int i = 0; i < rows; i++)
            {
                var newRow = new List<Block>();
                for (int j = 0; j < cols; j++)
                {
                    newRow.Add(grid.Blocks[counter]);
                    counter++;
                }
                result.Add(newRow);
            }

            return result;
        }
    }
}
