using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Heffsoft.PecsRemote.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly IHostService hostService;

        public SystemController(IHostService hostService)
        {
            this.hostService = hostService;
        }

        [HttpGet, Route("status")]
        public IActionResult GetStatus()
        {
            return Ok(new { hostService.Uptime });
        }

        [HttpGet, Route("updates")]
        public async Task<IActionResult> GetUpdatesAvailable()
        {
            Int32 updateCount = await hostService.GetUpdatesAvailable();
            return Ok(updateCount);
        }

        [HttpPost, Route("updates")]
        public async Task<IActionResult> ApplyUpdates()
        {
            await hostService.ApplyUpdates();
            return NoContent();
        }
    }
}