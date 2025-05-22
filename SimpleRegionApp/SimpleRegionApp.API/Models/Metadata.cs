namespace SimpleRegionApp.Models;

public class Metadata
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public long ImageSize { get; set; }
    public required string FileExtension { get; set; }
    public DateTime LastUpdate { get; set; }
}
