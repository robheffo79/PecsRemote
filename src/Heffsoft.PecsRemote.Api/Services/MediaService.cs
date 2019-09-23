using Heffsoft.PecsRemote.Api.Data.Models;
using Heffsoft.PecsRemote.Api.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using YoutubeExplode;
using YoutubeExplode.Models;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class MediaService : IMediaService
    {
        private const Double DEFAULT_THUMBNAIL_OFFSET = 0.125D;

        private readonly IUserService userService;
        private readonly IContentService contentService;
        private readonly IDataContext dataContext;
        private readonly IDataRepository<Media> mediaRepo;

        public MediaService(IUserService userService, IContentService contentService, IDataContext dataContext)
        {
            this.userService = userService;
            this.contentService = contentService;
            this.dataContext = dataContext;

            this.mediaRepo = this.dataContext.GetRepository<Media>();
        }

        public Media CreateMedia(String name, Uri url)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"{nameof(name)} is empty.");

            if (url == null)
                throw new ArgumentNullException(nameof(url));

            Media media = new Media()
            {
                Name = name,
                Image = Guid.Empty,
                Url = url.ToString(),
                Enabled = true,
                Created = DateTime.UtcNow,
                File = Guid.Empty,
                Duration = GetDuration(url),
                CreatedByUserId = userService.CurrentUser?.Id ?? -1,
                LastUpdated = DateTime.UtcNow,
                LastUpdatedByUserId = userService.CurrentUser?.Id ?? -1
            };

            media.Id = mediaRepo.Insert<Int32>(media);
            return media;
        }

        public Media CreateMedia(String name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"{nameof(name)} is empty.");

            Media media = new Media()
            {
                Name = name,
                Image = Guid.Empty,
                Url = null,
                Enabled = false,
                Created = DateTime.UtcNow,
                File = Guid.Empty,
                Duration = TimeSpan.Zero,
                CreatedByUserId = userService.CurrentUser?.Id ?? -1,
                LastUpdated = DateTime.UtcNow,
                LastUpdatedByUserId = userService.CurrentUser?.Id ?? -1
            };

            media.Id = mediaRepo.Insert<Int32>(media);
            return media;
        }

        private TimeSpan GetDuration(Uri url)
        {
            if (url.IsYouTubeUrl())
            {
                String id = YoutubeClient.ParseVideoId(url.ToString());
                YoutubeClient client = new YoutubeClient();
                Video videoInfo = client.GetVideoAsync(id).Result;
                return videoInfo.Duration;
            }

            return TimeSpan.Zero;
        }

        public void DeleteMedia(Int32 id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            mediaRepo.Delete(id);
        }

        public void DeleteMedia(Media media)
        {
            if (media == null)
                throw new ArgumentNullException(nameof(media));

            if (media.Id <= 0)
                throw new InvalidOperationException($"{nameof(media)} has an invalid id.");

            mediaRepo.Delete(media.Id);
        }

        public void DisableMedia(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            Media media = mediaRepo.Get(id);
            if (media != null)
            {
                media.Enabled = false;
                mediaRepo.Update(media);
            }
        }

        public void DisableMedia(Media media)
        {
            if (media == null)
                throw new ArgumentNullException(nameof(media));

            if (media.Id <= 0)
                throw new InvalidOperationException($"{nameof(media)} has an invalid id.");

            media.Enabled = false;
            mediaRepo.Update(media);
        }

        public void EnableMedia(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            Media media = mediaRepo.Get(id);
            if (media != null)
            {
                media.Enabled = true;
                mediaRepo.Update(media);
            }
        }

        public void EnableMedia(Media media)
        {
            if (media == null)
                throw new ArgumentNullException(nameof(media));

            if (media.Id <= 0)
                throw new InvalidOperationException($"{nameof(media)} has an invalid id.");

            media.Enabled = true;
            mediaRepo.Update(media);
        }

        public Media GetMedia(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            return mediaRepo.Get(id);
        }

        public IEnumerable<Media> GetMedia()
        {
            return mediaRepo.GetAll();
        }

        public void GenerateImage(Int32 id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            Media media = mediaRepo.Get(id);
            if (media != null)
            {
                media.Image = contentService.GenerateThumbnail(media.File, DEFAULT_THUMBNAIL_OFFSET);
                mediaRepo.Update(media);
            }
        }

        public void GenerateImage(Media media)
        {
            if (media == null)
                throw new ArgumentNullException(nameof(media));

            media.Image = contentService.GenerateThumbnail(media.File, DEFAULT_THUMBNAIL_OFFSET);
            mediaRepo.Update(media);
        }

        public void SetImage(Int32 id, Stream image, String mimeType)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            Media media = mediaRepo.Get(id);
            if (media != null)
            {
                media.Image = contentService.SaveThumbnail(image, mimeType);
                mediaRepo.Update(media);
            }
        }

        public void SetImage(Media media, Stream image, String mimeType)
        {
            if (media == null)
                throw new ArgumentNullException(nameof(media));

            media.Image = contentService.SaveThumbnail(image, mimeType);
            mediaRepo.Update(media);
        }

        public void SetContent(Int32 id, Stream content, String mimeType)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            Media media = mediaRepo.Get(id);
            if (media != null)
            {
                media.Image = contentService.SaveMedia(content, mimeType);
                mediaRepo.Update(media);
            }
        }

        public void SetContent(Media media, Stream content, String mimeType)
        {
            if (media == null)
                throw new ArgumentNullException(nameof(media));

            media.Image = contentService.SaveMedia(content, mimeType);
            mediaRepo.Update(media);
        }

        public void IncrementViews(Int32 id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            Media media = mediaRepo.Get(id);
            if (media != null)
            {
                media.ViewCount++;
                mediaRepo.Update(media);
            }
        }

        public void IncrementViews(Media media)
        {
            if (media == null)
                throw new ArgumentNullException(nameof(media));

            media.ViewCount++;
            mediaRepo.Update(media);
        }
    }
}
