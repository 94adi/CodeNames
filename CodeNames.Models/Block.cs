namespace CodeNames.Models
{
    public class Block
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Content { get; set; }
        public Color Color { get; set; }
        public bool IsClicked { get; set; }
    }
}
