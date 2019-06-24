using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Services
{
    public class MediaService : IMediaService
    {
        private readonly IUserService userService;
        private readonly IDataContext dataContext;

        public MediaService(IUserService userService, IDataContext dataContext)
        {
            this.userService = userService;
            this.dataContext = dataContext;
        }

        public Media AddMedia(String name, Guid image, Uri url)
        {
            Media media = new Media()
            {
                Name = name,
                Image = image,
                Url = url.ToString(),
                Enabled = true,
                Created = DateTime.UtcNow,
                CreatedByUserId = userService.CurrentUser?.Id ?? -1,
                LastUpdated = DateTime.UtcNow,
                LastUpdatedByUserId = userService.CurrentUser?.Id ?? -1
            };

            IDataRepository<Media> mediaRepo = dataContext.GetRepository<Media>();
            media.Id = mediaRepo.Insert(media);
            return media;
        }

        public void DeleteMedia(int id)
        {
            throw new NotImplementedException();
        }

        public void DeleteMedia(Media media)
        {
            throw new NotImplementedException();
        }

        public void DisableMedia(int id)
        {
            throw new NotImplementedException();
        }

        public void DisableMedia(Media media)
        {
            throw new NotImplementedException();
        }

        public void EnableMedia(int id)
        {
            throw new NotImplementedException();
        }

        public void EnableMedia(Media media)
        {
            throw new NotImplementedException();
        }

        public Media GetMedia(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Media> GetMedia()
        {
            throw new NotImplementedException();
        }
    }
}
