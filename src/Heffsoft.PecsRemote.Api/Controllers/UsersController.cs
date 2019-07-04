﻿using Heffsoft.PecsRemote.Api.Interfaces;
using Heffsoft.PecsRemote.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Heffsoft.PecsRemote.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IEventLogService eventLogService;
        private readonly IUserService userService;

        public UsersController(IEventLogService eventLogService, IUserService userService)
        {
            this.eventLogService = eventLogService;
            this.userService = userService;
        }

        [HttpPost, Route("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticationRequest authenticationRequest)
        {
            if(String.IsNullOrWhiteSpace(authenticationRequest.Username) || String.IsNullOrWhiteSpace(authenticationRequest.Password))
                return BadRequest();

            String token = userService.AuthenticateUser(authenticationRequest.Username, authenticationRequest.Password);
            if (String.IsNullOrWhiteSpace(token))
            {
                eventLogService.Log("Users", $"Failed login attempt for user '{authenticationRequest.Username}'");
                return Unauthorized();
            }

            eventLogService.Log("Users", $"Successful login attempt for user '{authenticationRequest.Username}'");
            return Ok(new { token });
        }
    }
}