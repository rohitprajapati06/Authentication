﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApiDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecureController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult PublicEndpoint()
        {
            return Ok("This is a public endpoint.");
        }

        [HttpGet("private")]
        [Authorize]
        public IActionResult PrivateEndpoint()
        {
            return Ok("This is a private endpoint, accessible only with a valid JWT.");
        }
    }
}
