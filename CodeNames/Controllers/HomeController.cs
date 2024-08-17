using System.Diagnostics;

namespace CodeNames.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGameRoomService _gameRoomService;

        public HomeController(ILogger<HomeController> logger,
            IGameRoomService gameRoomService)
        {
            _logger = logger;
            _gameRoomService = gameRoomService;
        }

        public IActionResult Index()
        {
            var viewModel = new IndexVM();

            viewModel.GameRooms = _gameRoomService.GetGameRooms();

            PopulateIndexVM(viewModel);

            return View(viewModel);
        }

        private void PopulateIndexVM(IndexVM viewModel)
        {
            if (viewModel.GameRooms == null)
                return;

            foreach(var gameRoom in viewModel.GameRooms)
            {
                var invitationUrl = $"Game/Session/LiveSession?gameRoom={gameRoom.Name}&invitationCode={gameRoom.InvitationCode}";
                viewModel.GameRoomsInvitaionLinks.Add(gameRoom.Id, invitationUrl);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
