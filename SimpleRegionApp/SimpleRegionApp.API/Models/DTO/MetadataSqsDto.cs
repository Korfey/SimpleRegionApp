namespace SimpleRegionApp.API.Models.DTO;

public record MetadataSqsDto(string Name, long ImageSize, string FileExtension, DateTime LastUpdate, string DownloadLink);

