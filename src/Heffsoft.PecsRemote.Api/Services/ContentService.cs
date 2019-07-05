using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class ContentService : IContentService
    {
        private const String THUMBNAIL_PATH = "/var/pecsremote/thumbs";
        private const String VIDEO_PATH = "/var/pecsremote/videos";

        private readonly IDataContext dataContext;
        private readonly IDataRepository<Content> contentRepo;

        public void DeleteThumbnail(Guid thumbnailId)
        {
            Delete(thumbnailId);
        }

        public void DeleteVideo(Guid videoId)
        {
            Delete(videoId);
        }

        private void Delete(Guid id)
        {
            Content content = contentRepo.Get(id);
            if (content != null)
            {
                File.Delete(content.Filename);
                contentRepo.Delete(content);
            }
        }

        public Guid SaveThumbnail(Stream data, String mimeType)
        {
            return Save(data, mimeType, THUMBNAIL_PATH);
        }

        public Guid SaveVideo(Stream data, String mimeType)
        {
            return Save(data, mimeType, VIDEO_PATH);
        }

        private Guid Save(Stream data, String mimeType, String path)
        {
            Guid id = data.HashGuid();
            String filename = Path.Combine(path, $"{id}{ExtensionForMime(mimeType)}");

            using (FileStream fs = File.OpenWrite(filename))
            {
                data.CopyTo(fs);
            }

            contentRepo.Insert<Guid>(new Content()
            {
                Id = id,
                MimeType = mimeType,
                Filename = filename,
                Size = data.Length
            });

            return id;
        }

        private String ExtensionForMime(String mimeType)
        {
            switch(mimeType.Trim().ToLower())
            {
                case "audio/aac":
                    return ".aac";

                case "video/x-msvideo":
                    return ".avi";

                case "image/bmp":
                    return ".bmp";

                case "image/gif":
                    return ".gif";

                case "image/vnd.microsoft.icon":
                    return ".ico";

                case "image/jpeg":
                    return ".jpg";

                case "audio/midi":
                case "audio/x-midi":
                    return ".mid";

                case "audio/mpeg":
                    return ".mp3";

                case "video/mpeg":
                    return ".mpg";

                case "audio/ogg":
                    return ".oga";

                case "video/ogg":
                    return ".ogv";

                case "application/ogg":
                    return ".ogx";

                case "image/png":
                    return ".png";

                case "image/svg+xml":
                    return ".svg";

                case "image/tiff":
                    return ".tif";

                case "video/mp2t":
                    return ".ts";

                case "audio/wav":
                    return ".wav";

                case "audio/webm":
                    return ".weba";

                case "video/webm":
                    return ".webv";

                case "image/webp":
                    return ".webp";

                case "video/3gpp":
                case "audio/3gpp":
                    return ".3gp";

                case "video/3gpp2":
                case "audio/3gpp2":
                    return ".3g2";

                case "video/x-flv":
                    return ".flv";

                case "video/mp4":
                    return ".mp4";

                case "application/x-mpegurl":
                    return ".m3u8";

                case "video/quicktime":
                    return ".mov";

                case "video/x-ms-wmv":
                    return ".wmv";
            }

            return "";
        }
    }
}
