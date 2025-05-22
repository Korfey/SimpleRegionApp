using SimpleRegionApp.API.Models.DTO;
using SimpleRegionApp.Models;

namespace SimpleRegionApp.API.Models.Mappers;

public static class MetadataMapper
{
    public static MetaDataResponse ToMetaDataResponse(this Metadata metadata)
    {
        return new MetaDataResponse(
            metadata.Name,
            metadata.ImageSize,
            metadata.FileExtension,
            metadata.LastUpdate);
    }

    public static Metadata ToMetadata(this MetaDataResponse metaDataResponse)
    {
        return new Metadata
        {
            Name = metaDataResponse.Name,
            ImageSize = metaDataResponse.ImageSize,
            FileExtension = metaDataResponse.FileExtension,
            LastUpdate = metaDataResponse.LastUpdate
        };
    }
}
