using CodeNames.CodeNames.Core.Services.GameRoomService;
using CodeNames.CodeNames.Core.Services.GridGenerator;
using CodeNames.Hubs;
using CodeNames.Models;
using CodeNames.Models.ViewModels;
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
        private readonly IHubContext<StateMachineHub> _stateMachineHubContext;
        private LiveSession _liveSessionModel;

        public SessionController(IGridGenerator gridGeneratorService,
            IGameRoomService gameRoomService,
            IHubContext<StateMachineHub> stateMachineHubContext)
        {
            _gridGeneratorService = gridGeneratorService;
            _gameRoomService = gameRoomService;
            _stateMachineHubContext = stateMachineHubContext;
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

            GameSessionDictioary.AddSession(model);

            ViewBag.ColorDictionary = StaticDetails.ColorToHexDict;

            LiveSessionVM viewModel = new LiveSessionVM();

            viewModel.LiveSession = model;

            foreach(var team in model.Teams)
            {
                viewModel.Teams.Add(team.Color, team);
            }

            return View(viewModel);
        }

        public IActionResult AccessForbidden(string gameRoom)
        {
            ViewBag.GameRoomName = gameRoom;
            return View();
        }

        private void PopulateLiveSessionModel(LiveSession model)
        {

            model.SessionState = SessionState.Pending;
            model.Teams.Add(new RedTeam());
            model.Teams.Add(new BlueTeam());
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
