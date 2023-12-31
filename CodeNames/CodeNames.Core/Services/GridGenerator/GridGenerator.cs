using CodeNames.Models;
using CodeNames.Repository;
using Microsoft.Extensions.Options;

namespace CodeNames.CodeNames.Core.Services.GridGenerator
{
    public class GridGenerator : IGridGenerator
    {
        private readonly GameParametersOptions _gameParametersOptions;
        private readonly int _gridSize;
        private List<string> _words;

        public GridGenerator(IOptions<GameParametersOptions> gameParametersOptions)
        {
            _gameParametersOptions = gameParametersOptions.Value;
            _gridSize = _gameParametersOptions.NumberOfRows * _gameParametersOptions.NumberOfColumns;
            _words = WordsListSingleton.Instance.Words;
        }

        public Grid Generate()
        {
            var randomWords = PickRandomWords();
            var generatedBlocks = GenerateBlocks(randomWords);

            return new Grid
            {
                Blocks = generatedBlocks,
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

        private IList<Block> GenerateBlocks(IList<string> words)
        {
            var randomColumns = new List<int>();
            var randomRows = new List<int>();

            var blockCollection = new List<Block>();

            var randomSelector = new Random();

            GenerateArray(_gameParametersOptions.NumberOfColumns, out randomColumns);
            GenerateArray(_gameParametersOptions.NumberOfRows, out randomRows);

            for(int i = 0; i < _gameParametersOptions.NumberOfRows; i++)
            {
                for(int j = 0; j < _gameParametersOptions.NumberOfColumns; j++)
                {
                    var currentBlock = new Block
                    {
                        Row = randomRows[i],
                        Column = randomColumns[j],
                        Content = _words[randomSelector.Next(_words.Count)],
                        Color = GenerateColor()
                    };

                    blockCollection.Add(currentBlock);
                }
            }

            return blockCollection;
        }

        private void GenerateArray(int size, out List<int> generatedArray)
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

        private Color GenerateColor()
        {
            if(_gameParametersOptions.RedTeamCards > 0)
            {
                _gameParametersOptions.RedTeamCards--;
                return Color.Red;
            }

            if(_gameParametersOptions.BlueTeamCards > 0)
            {
                _gameParametersOptions.BlueTeamCards--;
                return Color.Blue;
            }
            if(_gameParametersOptions.BlackCards > 0)
            {
                _gameParametersOptions.BlackCards--;
                return Color.Black;
            }

            return Color.Neutral;
        }
    }
}
