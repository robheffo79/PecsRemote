using Heffsoft.PecsRemote.Api.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Heffsoft.PecsRemote.Api.Interfaces
{
    public interface IMediaService
    {
        Media CreateMedia(String name);
        Media CreateMedia(String name, Uri url);

        void GenerateImage(Int32 id);
        void GenerateImage(Media media);

        void SetImage(Int32 id, Stream image, String mimeType);
        void SetImage(Media media, Stream image, String mimeType);

        void SetContent(Int32 id, Stream content, String mimeType);
        void SetContent(Media media, Stream content, String mimeType);

        void IncrementViews(Int32 id);
        void IncrementViews(Media media);

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
