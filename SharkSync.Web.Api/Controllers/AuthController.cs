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
        [AllowAnonymous]
        public IActionResult Start()
        {
            return Challenge(new AuthenticationProperties() { RedirectUri = Url.Action("Complete") });
        }

        [HttpGet()]
        [Route("Auth/Complete")]
        public async Task<IActionResult> Complete()
        {
            var loggedInAccount = await AuthService.GetLoggedInAccountAsync(User);

            var vm = new AuthCompleteViewModel()
            {
                Id = loggedInAccount.Id,
                Name = loggedInAccount.Name,
                EmailAddress = loggedInAccount.EmailAddress,
                GithubId = loggedInAccount.GithubId,
                AvatarUrl = loggedInAccount.AvatarUrl
            };

            return ModelState.GetJsonResultWithValidationErrors(vm);
        }
    }
}
