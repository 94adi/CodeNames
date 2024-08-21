namespace CodeNames.Controllers;

[Authorize(Roles = StaticDetails.ROLE_ADMIN)]
public class GameRoomController : Controller
{
    private readonly IGameRoomRepository _gameRoomRepository;

    public GameRoomController(IGameRoomRepository gameRoomRepository)
    {
        _gameRoomRepository = gameRoomRepository;
    }

    public IActionResult Index()
    {
        var model = new GameRoomIndexVM();
        model.PageTitle = "Game Rooms List";
        var gameRooms = _gameRoomRepository.GetAll();
        if(gameRooms != null && gameRooms.Count() > 0)
        {
            model.GameRooms = (List<GameRoom>)gameRooms;
            model.HasItems = true;
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult AddEditGameRoom(int? id)
    {
        GameRoomVM model = new();
        model.GameRoomId = id;
        if (id.HasValue)
        {
            model.PageTitle = "Update Game Room";
            model.ActionButtonName = "Update";
            model.GameRoom = _gameRoomRepository.Get(id.Value);
        }
        else
        {
            model.PageTitle = "Insert Game Room";
            model.ActionButtonName = "Create";
            model.GameRoom = new();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddEditGameRoom(GameRoom gameRoom)
    {
        if (ModelState.IsValid)
        {
            try
            {
                if(gameRoom.InvitationCode == Guid.Empty)
                {
                    gameRoom.InvitationCode = Guid.NewGuid();
                }
                _gameRoomRepository.Add(gameRoom);
                _gameRoomRepository.Save();
            }
            catch(Exception ex)
            {
                return View(gameRoom);
            }

            return RedirectToAction(nameof(Index));
        }

        return View(gameRoom);
    }
}