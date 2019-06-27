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
    public class SettingsController : ControllerBase
    {
        private readonly IHostService hostService;
        private readonly IConfiguration configuration;

        public SettingsController(IHostService hostService, IConfiguration configuration)
        {
            this.hostService = hostService;
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            SystemSettings settings = new SystemSettings()
            {
                Host = await hostService.GetHostSettings(),
                Network = await hostService.GetNetworkSettings(),
                Cast = new CastSettings()
                {
                    CastDevice = configuration.GetValue<String>("Cast:DeviceId", String.Empty)
                }
            };

            return Ok(settings);
        }
    }
}