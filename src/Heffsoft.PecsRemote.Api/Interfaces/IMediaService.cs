using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IMediaService
    {
        Media AddMedia(String name, Guid image, Uri url);

        void DeleteMedia(Int32 id);
        void EnableMedia(Int32 id);
        void DisableMedia(Int32 id);

        void DeleteMedia(Media media);
        void EnableMedia(Media media);
        void DisableMedia(Media media);

        Media GetMedia(Int32 id);
        IEnumerable<Media> GetMedia();
    }
}
