namespace Jareth.Core.Models;

public class Speaker
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
