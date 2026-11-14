namespace MantuGames.Models;

public class PlayerProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public string Color { get; set; } = "#22D3EE";
    public string AvatarImage { get; set; } = "a1.png"; // A1-A7 avatar image
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Initial =>
        string.IsNullOrEmpty(Name) ? "?" : Name.Substring(0, 1).ToUpperInvariant();
}
