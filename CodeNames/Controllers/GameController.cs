using CodeNames.CodeNames.Core.Services.GridGenerator;
using CodeNames.Models;
using CodeNames.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodeNames.Controllers
{
    public class GameController : Controller
    {
        private readonly IGridGenerator _gridGeneratorService;
        

        public GameController(IGridGenerator gridGeneratorService)
        {
            _gridGeneratorService = gridGeneratorService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LiveSession(string sessionRoom)
        {
            //1. Verify if user is allowed to join session (is formally member of group / there is enough space)
            //2. Hub will coordinate the game
            LiveSessionModel model = new LiveSessionModel();

            PopulateLiveSessionModel(model);

            ViewBag.ColorDictionary = StaticDetails.ColorToHexDict;

            return View(model);
        }

        private void PopulateLiveSessionModel(LiveSessionModel model)
        {
            model.GameState = GameState.Init;
            model.RedTeam = new Dictionary<Team, IdentityUser>();
            model.BlueTeam = new Dictionary<Team, IdentityUser>();
            model.Grid = _gridGeneratorService.Generate();
            //model.GameRoom = use GameRoom repo and retrieve it by sessionRoom name (or maybe guid)
        }

        private List<List<Block>> ConvertListToMatrix(Grid grid)
        {
            int rows = grid.Rows;
            int cols = grid.Columns;
            int counter = 0;
            List<List<Block>> result = new();

            for(int i = 0; i < rows; i++)
            {
                var newRow = new List<Block>();
                for(int j = 0; j < cols; j++)
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
