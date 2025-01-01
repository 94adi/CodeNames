using Microsoft.Extensions.Options;

namespace CodeNames.Areas.Game.Controllers;

[Area("Game")]
[Authorize]
public class SessionController : Controller
{
    private readonly IGridGenerator _gridGeneratorService;
    private readonly IGameRoomService _gameRoomService;
    private readonly IHubContext<StateMachineHub> _stateMachineHubContext;
    private readonly ILiveGameSessionService _liveGameSessionService;
    private LiveSession _liveSessionModel;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly GameParametersOptions _gameParametersOptions;

    public SessionController(IGridGenerator gridGeneratorService,
        IGameRoomService gameRoomService,
        IHubContext<StateMachineHub> stateMachineHubContext,
        ILiveGameSessionService liveGameSessionService,
        UserManager<IdentityUser> userManager,
        IOptions<GameParametersOptions> gameParametersOptions)
    {
        _gridGeneratorService = gridGeneratorService;
        _gameRoomService = gameRoomService;
        _stateMachineHubContext = stateMachineHubContext;
        _liveGameSessionService = liveGameSessionService;
        _userManager = userManager;
        _gameParametersOptions = gameParametersOptions.Value;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> LiveSession(string gameRoom, string invitationCode)
    {
        if (string.IsNullOrEmpty(gameRoom) || string.IsNullOrEmpty(invitationCode))
            //TO DO: Redirect to access forbidden page
            return RedirectToAction(nameof(Index));

        var hasGameRoomaAccess = _gameRoomService.IsInvitationCodeValid(gameRoom, new Guid(invitationCode));

        if (!hasGameRoomaAccess)
            return Forbid();

        LiveSession model = null;

        var gameRoomObj = _gameRoomService.GetRoomByName(gameRoom);

        if(gameRoomObj == null)
            return RedirectToAction(nameof(Index));

        var liveGameSession = _liveGameSessionService.GetByGameRoom(gameRoomObj.Id);

        var sessionId = liveGameSession?.SessionId;

        //TO DO: store this data in a persistance layer
        model = GameSessionDictioary.GetSession(sessionId.HasValue ? sessionId.Value : Guid.Empty);

        if(model == null)
        {
            model = new LiveSession();
            model.GameRoom = gameRoomObj;

            PopulateLiveSessionModel(model);

            GameSessionDictioary.AddSession(model);

            liveGameSession = new LiveGameSession
            {
                GameRoomId = model.GameRoom.Id,
                SessionId = model.SessionId,
            };

            _liveGameSessionService.AddEditLiveSession(liveGameSession);
        }

        ViewBag.ColorDictionary = ColorHelper.ColorToHexDict;

        LiveSessionVM viewModel = new LiveSessionVM();

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Forbid();

        viewModel.UserId = user.Id;

        PopulateLiveSessionVM(viewModel, model);

        return View(viewModel);
    }


    [HttpPost]
    public IActionResult Clue()
    {
        string test = "tGgesgegse";
        return Ok();
    }

    [AllowAnonymous]
    public IActionResult AccessForbidden(string gameRoom)
    {
        ViewBag.GameRoomName = gameRoom;
        return View();
    }

    private void PopulateLiveSessionModel(LiveSession model)
    {

        model.SessionState = SessionState.PENDING;
        //TO DO: Get rid of red/blue team
        model.Teams.Add(new RedTeam());
        model.Teams.Add(new BlueTeam());
        model.Grid = _gridGeneratorService.Generate();
        model.NumberOfTeamActiveCards.Add(Color.Red, _gameParametersOptions.RedTeamCards);
        model.NumberOfTeamActiveCards.Add(Color.Blue, _gameParametersOptions.BlueTeamCards);
    }

    private void PopulateLiveSessionVM(LiveSessionVM viewModel, LiveSession model)
    {
        viewModel.LiveSession = model;

        foreach (var team in model.Teams)
        {
            viewModel.Teams.Add(team.Color, team);
        }

        if(model.SessionState == SessionState.SPYMASTER_BLUE ||
            model.SessionState == SessionState.GUESS_BLUE)
        {
            viewModel.BackgroundColor = Color.BackgroundBlue;
        }
        else if(model.SessionState == SessionState.SPYMASTER_RED ||
            model.SessionState == SessionState.GUESS_RED)
        {
            viewModel.BackgroundColor = Color.BackgroundRed;
        }

        var player = model.PlayersList
                .Where(p => p.Id == viewModel.UserId)
                .FirstOrDefault();

        bool isPlayerAlreadyJoined = player != null ? true : false;

        if (isPlayerAlreadyJoined)
        {
            viewModel.InitialRun = false;

            viewModel.IsUserSpymaster = player.IsSpymaster;

            viewModel.UserTeamColor = player.TeamColor;

            if (player.IsSpymaster)
            {
                viewModel.HideBlueSpymasterButton = true;
                viewModel.HideRedSpymasterButton = true;
                viewModel.HideJoinBlueTeamButton = true;
                viewModel.HideJoinRedTeamButton = true;
            }
            else if(model.SessionState == SessionState.START && player.TeamColor == Color.Red)
            {
                viewModel.HideBlueSpymasterButton = true;
                viewModel.HideRedSpymasterButton = false;
                viewModel.HideJoinBlueTeamButton = true;
                viewModel.HideJoinRedTeamButton = true;
            }
            else if(model.SessionState == SessionState.START && player.TeamColor == Color.Blue)
            {
                viewModel.HideBlueSpymasterButton = false;
                viewModel.HideRedSpymasterButton = true;
                viewModel.HideJoinBlueTeamButton = true;
                viewModel.HideJoinRedTeamButton = true;
            }
            else
            {
                viewModel.HideBlueSpymasterButton = true;
                viewModel.HideRedSpymasterButton = true;
                viewModel.HideJoinBlueTeamButton = true;
                viewModel.HideJoinRedTeamButton = true;
            }
        }
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
