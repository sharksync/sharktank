using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharkSync.Web.Api.ViewModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using SharkSync.Interfaces.Repositories;
using SharkSync.Interfaces.Entities;
using System.Net;
using SharkSync.Web.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace SharkSync.Web.Api.Controllers
{
    [Authorize]
    [ValidateAntiForgeryToken]
    [Route("Api/Apps")]
    public class ApplicationController : Controller
    {
        ILogger Logger { get; set; }

        AuthService AuthService { get; set; }

        IApplicationRepository ApplicationRepository { get; set; }

        IChangeRepository ChangeRepository { get; set; }

        public ApplicationController(ILogger<ApplicationController> logger, AuthService authService, IApplicationRepository appRepository, IChangeRepository changeRepository)
        {
            Logger = logger;
            AuthService = authService;
            ApplicationRepository = appRepository;
            ChangeRepository = changeRepository;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var loggedInAccount = await AuthService.GetLoggedInAccountAsync(User);
            if (loggedInAccount == null)
                return Unauthorized();

            var apps = await ApplicationRepository.ListByAccountIdAsync(loggedInAccount.Id);

            var vm = new ApplicationListResponseViewModel
            {
                Applications = apps.Select(a => new ApplicationViewModel(a))
            };

            return ModelState.GetJsonResultWithValidationErrors(vm);
        }

        [HttpPost()]
        public async Task<IActionResult> PostAsync(string name)
        {
            var loggedInAccount = await AuthService.GetLoggedInAccountAsync(User);
            if (loggedInAccount == null)
                return Unauthorized();

            var app = await ApplicationRepository.AddAsync(name, loggedInAccount.Id);

            await ChangeRepository.CreateChangeTableForApp(app.Id);

            var vm = new ApplicationGetResponseViewModel() { Application = new ApplicationViewModel(app) };

            return ModelState.GetJsonResultWithValidationErrors(vm);
        }

        [HttpDelete()]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var loggedInAccount = await AuthService.GetLoggedInAccountAsync(User);
            if (loggedInAccount == null)
                return Unauthorized();

            var app = await ApplicationRepository.GetByIdAsync(id);

            if (app == null)
                return NotFound();

            if (app.AccountId != loggedInAccount.Id)
                return Forbid();
            
            await ApplicationRepository.DeleteAsync(id);

            await ChangeRepository.DeleteChangeTableForApp(id);

            return ModelState.GetJsonResultWithValidationErrors();
        }

    }
}
