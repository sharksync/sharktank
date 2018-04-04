using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharkSync.Web.Api.ViewModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using SharkTank.Interfaces.Repositories;
using SharkTank.Interfaces.Entities;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SharkSync.Web.Api.Services;

namespace SharkSync.Web.Api.Controllers
{
    public class AuthController : Controller
    {
        ILogger Logger { get; set; }

        AuthService AuthService { get; set; }

        public AuthController(ILogger<AuthController> logger, AuthService authService)
        {
            Logger = logger;
            AuthService = authService;
        }

        [HttpGet()]
        [Route("Auth/Start")]
        public IActionResult Start(string returnUrl)
        {
            // TODO: Validate return URL is the correct domain

            return Challenge(new AuthenticationProperties() { RedirectUri = Url.Action("Complete", new { returnUrl }) });
        }

        [HttpGet()]
        [Route("Auth/Complete")]
        public IActionResult Complete(string returnUrl)
        {
            // TODO: Validate return URL is the correct domain

            return Redirect(returnUrl);
        }
    }
}
