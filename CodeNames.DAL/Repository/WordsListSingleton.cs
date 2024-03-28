
using CodeNames.Data;
using System.Reflection;

namespace CodeNames.Repository
{
    public sealed class WordsListSingleton 
    {
        private static readonly WordsListSingleton _instance = new WordsListSingleton();

        private List<string> _wordsCollection;

        private WordsListSingleton()
        {
            _wordsCollection = new List<string>();
        }
        
        public static WordsListSingleton Instance { get => _instance; }

        public List<string> Words { get
            {
                if(_wordsCollection.Count == 0)
                {
                    _wordsCollection = (List<string>)GetAllWords();
                }
                return _wordsCollection;
            } 
        }

        private IList<string> GetAllWords()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var fileName = "wordslist.txt";
            var fullPath = Path.Combine(path, fileName);
            var words = new List<string>();

            using(var streamReader = new StreamReader(fullPath))
            {
                while (!streamReader.EndOfStream)
                {
                    string word = streamReader.ReadLine();
                    if (!string.IsNullOrEmpty(word))
                    {
                        words.Add(word);
                    }
                }
            }          
            return words;
        }
    }
}
