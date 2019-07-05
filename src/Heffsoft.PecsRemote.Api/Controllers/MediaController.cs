using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace Heffsoft.PecsRemote.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IEventLogService eventLogService;
        private readonly IMediaService mediaService;

        public MediaController(IEventLogService eventLogService, IMediaService mediaService)
        {
            this.eventLogService = eventLogService;
            this.mediaService = mediaService;
        }

        [HttpGet, Route("{id:int}")]
        public IActionResult Play(Int32 id)
        {
            Media media = mediaService.GetMedia(id);
            if (media != null)
            {
                String redirectTo = media.Url;

                if (IsYouTubeUrl(media.Url))
                {
                    String ytUrl = ProcessYouTubeUrl(media.Url);
                    if (ytUrl != null)
                    {
                        redirectTo = ytUrl;
                    }
                }

                eventLogService.Log("Media", $"Redirecting client to media url", redirectTo);
                return Redirect(redirectTo);
            }

            eventLogService.Log("Media", $"Media Id '{id}' Not Found.");
            return NotFound();
        }

        [HttpPost]
        public IActionResult Create([FromBody]MediaCreation media)
        {
            mediaService.AddMedia()
        }

        private Boolean IsYouTubeUrl(String url)
        {
            Regex ytRegex = new Regex(@"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$");
            return ytRegex.IsMatch(url);
        }

        private String ProcessYouTubeUrl(String url)
        {
            try
            {
                String id = YoutubeClient.ParseVideoId(url);
                YoutubeClient client = new YoutubeClient();
                MediaStreamInfoSet streamInfoSet = client.GetVideoMediaStreamInfosAsync(id).Result;

                MuxedStreamInfo streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
                return streamInfo.Url;
            }
            catch
            {
                return null;
            }
        }
    }
}