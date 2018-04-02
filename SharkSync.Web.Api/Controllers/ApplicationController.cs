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
using SharkSync.Web.Api.Services;

namespace SharkSync.Web.Api.Controllers
{
    [Route("Account/Apps")]
    public class ApplicationController : Controller
    {
        ILogger Logger { get; set; }

        AuthService AuthService { get; set; }

        IApplicationRepository ApplicationRepository { get; set; }

        public ApplicationController(ILogger<ApplicationController> logger, AuthService authService, IApplicationRepository appRepository)
        {
            Logger = logger;
            AuthService = authService;
            ApplicationRepository = appRepository;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var loggedInAccount = await AuthService.GetLoggedInAccountAsync(User);
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
            var app = await ApplicationRepository.AddAsync(name, loggedInAccount.Id);

            var vm = new ApplicationGetResponseViewModel() { Application = new ApplicationViewModel(app) };

            return ModelState.GetJsonResultWithValidationErrors(vm);
        }

        [HttpDelete()]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            // AUTH CHECK HERE

            await ApplicationRepository.DeleteAsync(id);

            return ModelState.GetJsonResultWithValidationErrors();
        }

    }
}
