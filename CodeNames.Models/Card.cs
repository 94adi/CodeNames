namespace CodeNames.Models
{
    public class Card
    {
        public string? CardId { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public string? Content { get; set; }
        public Color Color { get; set; }
        public string? ColorHex { get; set; }
        public bool IsRevealed { get; set; }
    }
}
