namespace CodeNames.CodeNames.Core.Services.GridGenerator;

public class GridGenerator : IGridGenerator
{
    private readonly GameParametersOptions _gameParametersOptions;
    private readonly int _gridSize;
    private List<string> _words;
    private IDictionary<Color, int> _colorRequirements;

    public GridGenerator(IOptions<GameParametersOptions> gameParametersOptions)
    {
        _gameParametersOptions = gameParametersOptions.Value;
        _gridSize = _gameParametersOptions.NumberOfRows * _gameParametersOptions.NumberOfColumns;
        _words = WordsListSingleton.Instance.Words;
        _colorRequirements = new Dictionary<Color, int>();

        PopulateColorRequirements();
    }

    public Grid Generate()
    {
        var randomWords = PickRandomWords();
        var generatedBlocks = GenerateBlocks(randomWords).ToList();

        return new Grid
        {
            Cards = generatedBlocks,
            Rows = _gameParametersOptions.NumberOfRows,
            Columns = _gameParametersOptions.NumberOfColumns
        };
        
    }

    private List<string> PickRandomWords()
    {
        Random randomGenerator = new Random();
        List<string> result = new List<string>();
        int wordsLength = _words.Count;

        for (int i = 0; i < _gridSize; i++)
        {
            int randomIndex = randomGenerator.Next(wordsLength);
            result.Add(_words[randomIndex]);
        }

        return result;
    }

    private HashSet<Card> GenerateBlocks(IList<string> words)
    {
        var randomColumns = new HashSet<int>();
        var randomRows = new HashSet<int>();

        var blockCollection = new HashSet<Card>();

        var randomSelector = new Random();

        GenerateArray(_gameParametersOptions.NumberOfColumns, out randomColumns);
        GenerateArray(_gameParametersOptions.NumberOfRows, out randomRows);

        for(int i = 0; i < _gameParametersOptions.NumberOfRows; i++)
        {
            for(int j = 0; j < _gameParametersOptions.NumberOfColumns; j++)
            {
                Color cardColor = Color.None;
                GenerateColor(ref cardColor);
                string word = _words[randomSelector.Next(_words.Count)];
                int row = randomRows.ElementAt(i);
                int column = randomColumns.ElementAt(j);

                var currentBlock = new Card
                {
                    CardId = $"cardAt-{row}-{column}",
                    Row = row,
                    Column = column,
                    Content = word,
                    Color = cardColor,
                    ColorHex = ColorHelper.ColorToHexDict[cardColor],
                    IsRevealed = false
                };

                blockCollection.Add(currentBlock);
            }
        }

        return blockCollection;
    }

    private void GenerateArray(int size, out HashSet<int> generatedArray)
    {
        Random randomGenerator = new Random();
        bool shouldRegenerate = false;
        
        generatedArray = new ();

        for (int i = 0; i < size; i++)
        {
            do
            {
                var randomValue = randomGenerator.Next(size);
                if (generatedArray.Contains(randomValue))
                {
                    shouldRegenerate = true;
                }
                else
                {
                    shouldRegenerate = false;
                    generatedArray.Add(randomValue);
                }
            } while (shouldRegenerate);
        }
    }

    private void GenerateColor(ref Color cardColor)
    {
        //TO DO: Improve method
        Random randGen = new Random();
        var colorReturned = false;

        if(_colorRequirements[Color.Red] == 0 &&
           _colorRequirements[Color.Blue] == 0 &&
           _colorRequirements[Color.Neutral] == 0 &&
           _colorRequirements[Color.Black] == 0)
           {               
                throw new Exception("Can't generate color because there's no quota left");
           }

        do
        {
            var randomResult = randGen.Next(4);
            if ((randomResult == 0) && (_colorRequirements[Color.Red] > 0))
            {
                _colorRequirements[Color.Red]--;
                cardColor = Color.Red;
                colorReturned = true;
            }

            else if ((randomResult == 1) && (_colorRequirements[Color.Blue] > 0))
            {
                _colorRequirements[Color.Blue]--;
                cardColor = Color.Blue;
                colorReturned = true;
            }
            else if ((randomResult == 2) && (_colorRequirements[Color.Neutral] > 0))
            {
                _colorRequirements[Color.Neutral]--;
                cardColor = Color.Neutral;
                colorReturned = true;
            }
            else if (_colorRequirements[Color.Black] > 0)
            {
                _colorRequirements[Color.Black]--;
                cardColor = Color.Black;
                colorReturned = true;
            }
        } while (!colorReturned);         
    }

    private void PopulateColorRequirements()
    {
        _colorRequirements[Color.Red] = _gameParametersOptions.RedTeamCards;
        _colorRequirements[Color.Blue] = _gameParametersOptions.BlueTeamCards;
        _colorRequirements[Color.Black] = _gameParametersOptions.BlackCards;
        _colorRequirements[Color.Neutral] = _gridSize - 
            (_gameParametersOptions.RedTeamCards + 
            _gameParametersOptions.BlueTeamCards + 
            _gameParametersOptions.BlackCards);
    }
}
