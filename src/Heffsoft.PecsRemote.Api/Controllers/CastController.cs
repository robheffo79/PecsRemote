using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Heffsoft.PecsRemote.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CastController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ICastService castService;

        public CastController(ICastService castService, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.castService = castService;
        }

        [HttpGet, Route("devices")]
        public async Task<IActionResult> GetCastDevices()
        {
            var devices = await castService.GetCastReceivers();
            if (devices == null)
                return NoContent();

            return Ok(devices);
        }

        public async Task<IActionResult> Cast([FromBody]CastUrl castUrl)
        {
            if (castUrl == null)
                return BadRequest(new { error = $"{nameof(castUrl)} is missing." });

            if (String.IsNullOrWhiteSpace(castUrl.Url))
                return BadRequest();

            if(castService.CurrentReceiver == null)
            {
                String id = configuration.GetValue<String>("cast:deviceid", null);
                if (id == null)
                    return BadRequest(new { error = $"No default cast device has been configured." });

                await castService.ConnectToCastReceiver(id);
            }

            if (Uri.TryCreate(castUrl.Url, UriKind.Absolute, out Uri uri) == false)
                return BadRequest(new { error = $"Requested url is invalid." });

            await castService.CastMedia(uri);
            return NoContent();
        }
    }
}