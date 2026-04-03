namespace Jareth.Core.Models;

public class Folder
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int MeetingCount { get; set; }
}
