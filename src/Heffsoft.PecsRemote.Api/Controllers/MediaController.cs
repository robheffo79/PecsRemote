using Heffsoft.PecsRemote.Api.Data.Models;
using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using Heffsoft.PecsRemote.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace Heffsoft.PecsRemote.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IEventLogService eventLogService;
        private readonly IMediaService mediaService;
        private readonly IContentService contentService;

        public MediaController(IEventLogService eventLogService, IMediaService mediaService, IContentService contentService)
        {
            this.eventLogService = eventLogService;
            this.mediaService = mediaService;
            this.contentService = contentService;
        }

        [HttpGet, Route("")]
        public IActionResult GetMedia()
        {

            return Ok(mediaService.GetMedia().Select(m => new
            {
                Id = m.Id,
                Name = m.Name,
                Url = contentService.GetMediaUrl(m.File),
                Thumbnail = contentService.GetThumbnailUrl(m.Image),
                Duration = FormatDuration(m.Duration),
                ViewCount = m.ViewCount
            }));
        }

        private String FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays > 1.00D)
            {
                return duration.ToString("d\\.hh\\:mm\\:ss");
            }

            if (duration.TotalHours > 1.00D)
            {
                return duration.ToString("hh\\:mm\\:ss");
            }

            return duration.ToString("mm\\:ss");
        }

        [HttpGet, Route("{id:int}")]
        public IActionResult Play(Int32 id, Boolean inc = true)
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

                if (inc)
                {
                    mediaService.IncrementViews(id);
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
            //mediaService.AddMedia()

            return Ok();
        }

        [HttpGet, Route("suggest")]
        public IActionResult SuggestTitle(String q)
        {
            if (Uri.TryCreate(q, UriKind.Absolute, out Uri url))
            {
                if (url.IsYouTubeUrl())
                {
                    String id = YoutubeClient.ParseVideoId(url.ToString());
                    YoutubeClient client = new YoutubeClient();
                    Video video = client.GetVideoAsync(id).Result;
                    return Ok(new { title = video.Title });
                }
            }

            return NoContent();
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