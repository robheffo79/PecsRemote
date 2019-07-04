using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class MediaService : IMediaService
    {
        private readonly IUserService userService;
        private readonly IDataContext dataContext;
        private readonly IDataRepository<Media> mediaRepo;

        public MediaService(IUserService userService, IDataContext dataContext)
        {
            this.userService = userService;
            this.dataContext = dataContext;

            this.mediaRepo = this.dataContext.GetRepository<Media>();
        }

        public Media AddMedia(String name, Guid image, Uri url)
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
                Image = image,
                Url = url.ToString(),
                Enabled = true,
                Created = DateTime.UtcNow,
                FilePath = null,
                Duration = GetDuration(url),
                CreatedByUserId = userService.CurrentUser?.Id ?? -1,
                LastUpdated = DateTime.UtcNow,
                LastUpdatedByUserId = userService.CurrentUser?.Id ?? -1
            };

            media.Id = mediaRepo.Insert(media);
            return media;
        }

        private TimeSpan GetDuration(Uri url)
        {
            if(url.IsYouTubeUrl())
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
    }
}
