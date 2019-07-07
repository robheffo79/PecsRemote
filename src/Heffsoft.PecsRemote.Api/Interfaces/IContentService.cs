using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IContentService
    {
        Guid GenerateThumbnail(Guid videoId, Double offset);
        Guid SaveThumbnail(Stream data, String mimeType);
        void DeleteThumbnail(Guid thumbnailId);
        String GetThumbnailUrl(Guid thumbnailId);

        Guid SaveVideo(Stream data, String mimeType);
        void DeleteVideo(Guid videoId);
        String GetVideoUrl(Guid videoId);
    }
}
