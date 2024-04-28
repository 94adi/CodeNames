using CodeNames.CodeNames.Core.Services.GameRoomService;
using CodeNames.CodeNames.Core.Services.GridGenerator;
using CodeNames.Core.Services.LiveGameSessionService;
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
        private readonly ILiveGameSessionService _liveGameSessionService;
        private LiveSession _liveSessionModel;

        public SessionController(IGridGenerator gridGeneratorService,
            IGameRoomService gameRoomService,
            IHubContext<StateMachineHub> stateMachineHubContext,
            ILiveGameSessionService liveGameSessionService)
        {
            _gridGeneratorService = gridGeneratorService;
            _gameRoomService = gameRoomService;
            _stateMachineHubContext = stateMachineHubContext;
            _liveGameSessionService = liveGameSessionService;
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

            var hasGameRoomaAccess = _gameRoomService.IsInvitationCodeValid(gameRoom, new Guid(invitationCode));

            if (!hasGameRoomaAccess)
            {
                return Forbid();
            }

            LiveSession model = null;

            var gameRoomObj = _gameRoomService.GetRoomByName(gameRoom);

            if(gameRoomObj != null)
            {
                var liveGameSession = _liveGameSessionService.GetByGameRoom(gameRoomObj.Id);

                var sessionId = liveGameSession?.SessionId;

                model = GameSessionDictioary.GetSession(sessionId.HasValue ? sessionId.Value : Guid.Empty);
            }

            if(model == null)
            {
                model = new LiveSession();

                model.GameRoom = _gameRoomService.GetRoomByName(gameRoom);

                if (model.GameRoom == null)
                {
                    return NotFound();
                }
                //no need to populate it IF IT ALREADY EXISTS!!!
                PopulateLiveSessionModel(model);
                //add live session to db
                GameSessionDictioary.AddSession(model);

                LiveGameSession liveGameSession = new LiveGameSession
                {
                    GameRoomId = model.GameRoom.Id,
                    SessionId = model.SessionId,
                };
                _liveGameSessionService.AddEditLiveSession(liveGameSession);
            }

            ViewBag.ColorDictionary = StaticDetails.ColorToHexDict;

            LiveSessionVM viewModel = new LiveSessionVM();

            viewModel.LiveSession = model;

            foreach(var team in model.Teams)
            {
                viewModel.Teams.Add(team.Color, team);
            }

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult Clue()
        {
            string test = "tGgesgegse";
            return Ok();
        }
        public IActionResult AccessForbidden(string gameRoom)
        {
            ViewBag.GameRoomName = gameRoom;
            return View();
        }

        private void PopulateLiveSessionModel(LiveSession model)
        {

            model.SessionState = SessionState.PENDING;
            model.Teams.Add(new RedTeam());
            model.Teams.Add(new BlueTeam());
            model.Grid = _gridGeneratorService.Generate();
        }

        private List<List<Card>> ConvertListToMatrix(Grid grid)
        {
            int rows = grid.Rows;
            int cols = grid.Columns;
            int counter = 0;
            List<List<Card>> result = new();

            for (int i = 0; i < rows; i++)
            {
                var newRow = new List<Card>();
                for (int j = 0; j < cols; j++)
                {
                    newRow.Add(grid.Cards[counter]);
                    counter++;
                }
                result.Add(newRow);
            }

            return result;
        }
    }
}
