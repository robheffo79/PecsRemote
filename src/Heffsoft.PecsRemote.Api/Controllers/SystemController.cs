using Heffsoft.PecsRemote.Api.Data.Models;
using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly IEventLogService eventLogService;
        private readonly IHostService hostService;
        private readonly IThermalService thermalService;

        public SystemController(IEventLogService eventLogService, IHostService hostService, IThermalService thermalService)
        {
            this.eventLogService = eventLogService;
            this.hostService = hostService;
            this.thermalService = thermalService;

            thermalService.StartControl();
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

        [HttpGet, Route("temperature")]
        public async Task<IActionResult> GetSystemTemperature()
        {
            Double temperature = 0.00D;
            await Task.Run(() => { temperature = thermalService.Temperature; });

            return Ok(temperature);
        }

        [HttpGet, Route("signalStrength")]
        public async Task<IActionResult> GetWiFiStrength()
        {
            Double? signalStrength = await hostService.GetWiFiStrength();
            return Ok(new { ssid = "SSID Name", signalStrength });
        }

        [HttpGet, Route("languages")]
        public async Task<IActionResult> GetLanguages()
        {
            IEnumerable<Language> languages = await hostService.GetLanguages();
            return Ok(languages);
        }

        [HttpPost, Route("update")]
        public async Task<IActionResult> ApplyUpdates()
        {
            eventLogService.Log("System", "Began applying updates.");
            await hostService.ApplyUpdates();
            return NoContent();
        }
    }
}