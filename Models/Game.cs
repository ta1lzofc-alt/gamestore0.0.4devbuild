namespace GameStore.Desktop.Models;

public class Game
{
    public long GameId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string Genres { get; set; } = "";
    public string CoverUrl { get; set; } = "";
}