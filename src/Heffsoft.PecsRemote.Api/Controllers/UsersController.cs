using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Heffsoft.PecsRemote.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;

        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpPost, Route("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticationRequest authenticationRequest)
        {
            if(String.IsNullOrWhiteSpace(authenticationRequest.Username) || String.IsNullOrWhiteSpace(authenticationRequest.Password))
                return BadRequest();

            String token = userService.AuthenticateUser(authenticationRequest.Username, authenticationRequest.Password);
            if (String.IsNullOrWhiteSpace(token))
                return Unauthorized();

            return Ok(new { token });
        }
    }
}