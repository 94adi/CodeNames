namespace CodeNames.Models
{
    public class GameLog
    {
        public string UserName { get; set; }

        public string Action { get; set; }

        public string Word {  get; set; }

        public override string ToString()
        {
            return $"{UserName} {Action} {Word}";
        }
    }
}
