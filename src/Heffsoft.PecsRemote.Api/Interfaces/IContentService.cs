using System;
using System.IO;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IContentService
    {
        Guid GenerateThumbnail(Guid mediaId, Double offset);
        Guid SaveThumbnail(Stream data, String mimeType);
        void DeleteThumbnail(Guid thumbnailId);
        String GetThumbnailUrl(Guid thumbnailId);

        Guid SaveMedia(Stream data, String mimeType);
        void DeleteMedia(Guid mediaId);
        String GetMediaUrl(Guid mediaId);
    }
}
