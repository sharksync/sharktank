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

namespace SharkSync.Web.Api.Controllers
{
    public class AuthController : Controller
    {
        ILogger Logger { get; set; }

        public AuthController(ILogger<AuthController> logger)
        {
            Logger = logger;
        }

        [HttpGet()]
        [Route("Auth/Start")]
        public IActionResult Start()
        {
            return Challenge(new AuthenticationProperties() { RedirectUri = Url.Action("Complete") });
        }

        [HttpGet()]
        [Route("Auth/Complete")]
        public IActionResult Complete()
        {
            var vm = new AuthCompleteViewModel();

            if (User.Identity.IsAuthenticated)
            {
                vm.Name = User.FindFirst(c => c.Type == ClaimTypes.Name)?.Value;
                vm.Login = User.FindFirst(c => c.Type == "urn:github:login")?.Value;
                vm.AvatarUrl = User.FindFirst(c => c.Type == "urn:github:avatar")?.Value;
                vm.GitHubUrl = User.FindFirst(c => c.Type == "urn:github:url")?.Value;
            }

            return ModelState.GetJsonResultWithValidationErrors(vm);
        }
    }
}
