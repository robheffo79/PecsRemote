using FFMpegCore;
using FFMpegCore.FFMPEG;
using Heffsoft.PecsRemote.Api.Data.Models;
using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class ContentService : IContentService
    {
        private readonly String mediaPath;

        private readonly String thumbnailPath;
        private readonly String thumbnailFormat;
        private readonly Int32 thumbnailWidth;
        private readonly Int32 thumbnailHeight;
        private readonly Double thumbnailQuality;

        private readonly IDataContext dataContext;
        private readonly IDataRepository<Content> contentRepo;

        public ContentService(IDataContext dataContext, IConfiguration configuration)
        {
            this.dataContext = dataContext;
            this.contentRepo = dataContext.GetRepository<Content>();

            mediaPath = configuration.GetValue<String>("content:media:path");

            thumbnailPath = configuration.GetValue<String>("content:thumbnails:path");
            thumbnailFormat = configuration.GetValue<String>("content:thumbnails:format");
            thumbnailWidth = configuration.GetValue<Int32>("content:thumbnails:width");
            thumbnailHeight = configuration.GetValue<Int32>("content:thumbnails:height");
            thumbnailQuality = configuration.GetValue<Double>("content:thumbnails:quality");
        }

        public void DeleteThumbnail(Guid thumbnailId)
        {
            Delete(thumbnailId);
        }

        public void DeleteMedia(Guid videoId)
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
            return Save(data, mimeType, thumbnailPath);
        }

        public Guid SaveMedia(Stream data, String mimeType)
        {
            return Save(data, mimeType, mediaPath);
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
            switch (mimeType.Trim().ToLower())
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

        public Guid GenerateThumbnail(Guid mediaId, Double offset)
        {
            if (mediaId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(mediaId));

            if (offset < 0.00D || offset > 1.00D)
                throw new ArgumentOutOfRangeException(nameof(offset));

            Content mediaContent = contentRepo.Get(mediaId);
            if (mediaContent == null)
                throw new Exception($"Media '{mediaId}' not found.");

            FileInfo tempFile = new FileInfo(Path.GetTempFileName());
            VideoInfo videoInfo = new VideoInfo(mediaContent.Filename);
            TimeSpan imageOffetTimeStamp = new TimeSpan((Int64)(videoInfo.Duration.Ticks * offset));

            FFMpeg processor = new FFMpeg();
            Bitmap image = processor.Snapshot(videoInfo, tempFile, new System.Drawing.Size(thumbnailWidth, thumbnailHeight), imageOffetTimeStamp, false);

            using (MemoryStream ms = new MemoryStream())
            {
                String mime = null;

                switch (thumbnailFormat.ToLower())
                {
                    case "jpg":
                        mime = "image/jpeg";
                        ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                        EncoderParameters parameters = new EncoderParameters(1);
                        parameters.Param[0] = new EncoderParameter(Encoder.Quality, Math.Clamp((Int64)(100.0D * thumbnailQuality), 0L, 100L));
                        image.Save(ms, jpegEncoder, parameters);
                        break;

                    case "png":
                        mime = "image/png";
                        image.Save(ms, ImageFormat.Png);
                        break;

                    case "bmp":
                        mime = "image/bmp";
                        image.Save(ms, ImageFormat.Bmp);
                        break;
                }

                ms.Position = 0;
                return SaveThumbnail(ms, mime);
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public String GetThumbnailUrl(Guid thumbnailId)
        {
            return "/images/thumb-missing.png";
        }

        public String GetMediaUrl(Guid videoId)
        {
            return "/videos/video.mp4";
        }
    }
}
