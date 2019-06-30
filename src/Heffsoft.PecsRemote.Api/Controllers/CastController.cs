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
    public class CastController : ControllerBase
    {
        private readonly ICastService castService;

        public CastController(ICastService castService)
        {
            this.castService = castService;
        }

        [HttpGet, Route("devices")]
        public async Task<IActionResult> GetCastDevices()
        {
            var devices = await castService.GetCastReceivers();

            if(devices.Any())
            {
                var device = devices.First();
                if (await castService.ConnectToCastReceiver(device.Id) == true)
                {
                    await castService.CastMedia(new Uri("https://r1---sn-u2bpouxgoxu-ntqy.googlevideo.com/videoplayback?expire=1561889466&ei=WTYYXfPdM-uEssUP8PS0kAQ&ip=115.64.253.208&id=o-ABygt5Q_wM6xabGY8-kHBPx9nclOo7AogERssPXrhXqI&itag=18&source=youtube&requiressl=yes&mm=31%2C29&mn=sn-u2bpouxgoxu-ntqy%2Csn-ntqe6n7r&ms=au%2Crdu&mv=m&pl=22&initcwndbps=908750&mime=video%2Fmp4&gir=yes&clen=21400157&ratebypass=yes&dur=237.354&lmt=1549223579478416&mt=1561867724&fvip=1&c=WEB&txp=5531432&sparams=expire%2Cei%2Cip%2Cid%2Citag%2Csource%2Crequiressl%2Cmime%2Cgir%2Cclen%2Cratebypass%2Cdur%2Clmt&sig=ALgxI2wwRQIgO8evOMFdm8dfyuOY8qknucfFy5Dzi0y_Bm2sWL7wsgsCIQDsASoC8yyqGSgcLnHNbIffHeYv3c9PWgWbtN5xXIhBCQ%3D%3D&lsparams=mm%2Cmn%2Cms%2Cmv%2Cpl%2Cinitcwndbps&lsig=AHylml4wRAIgeLSP57imc8aBORElXPYdLPdedvVBKSbif1TYAmTmkR4CIDlx_It-uJ4mWiFlhUGgxGV7U3kj1dVsnKu1z8vAO1LH"));
                }
            }

            return Ok(devices);
        }
    }
}