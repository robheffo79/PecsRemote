using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IContentService
    {
        Guid SaveThumbnail(Stream data, String mimeType);
        void DeleteThumbnail(Guid thumbnailId);

        Guid SaveVideo(Stream data, String mimeType);
        void DeleteVideo(Guid videoId);
    }
}
