using CodeNames.Models;
using CodeNames.Repository;

namespace CodeNames.CodeNames.Core.Services.GridGenerator
{
    public class GridGenerator : IGridGenerator
    {
        private int _cols;
        private int _rows;
        private int _gridSize;
        private List<string> _words;

        public GridGenerator(int cols, int rows)
        {
            _cols = cols;
            _rows = rows;
            _gridSize = _cols * _rows;
            _words = WordsListSingleton.Instance.Words;
        }

        public Grid Generate(int size)
        {
            var randomWords = PickRandomWords();
            var generatedBlocks = GenerateBlocks(randomWords);

            return new Grid
            {
                Blocks = generatedBlocks
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

            GenerateArray(_cols, out randomColumns);
            GenerateArray(_rows, out randomColumns);

            for(int i = 0; i < _rows; i++)
            {
                for(int j = 0; j <  _cols; j++)
                {
                    var currentBlock = new Block
                    {
                        Row = randomRows[i],
                        Column = randomColumns[j],
                        Content = _words[randomSelector.Next(_words.Count)]
                        //To add color
                    };

                    blockCollection.Add(currentBlock);
                }
            }

            return null;
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
                    var column = randomGenerator.Next(size);
                    if (generatedArray.Contains(column))
                    {
                        shouldRegenerate = true;
                    }
                    else
                    {
                        shouldRegenerate = false;
                        generatedArray.Add(column);
                    }
                } while (shouldRegenerate);
            }
        }

        //rules
        //red team: 8 cards
        //blue team: 9 cards
        //1 black card
        //rest of cards are neutral
   
    }
}
