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

namespace SharkSync.Web.Api.Controllers
{
    [Route("Account/Apps")]
    public class ApplicationController : Controller
    {
        ILogger Logger { get; set; }

        IAccountRepository AccountRepository { get; set; }

        IApplicationRepository ApplicationRepository { get; set; }

        public ApplicationController(ILogger<ApplicationController> logger, IAccountRepository accountRepository, IApplicationRepository appRepository)
        {
            Logger = logger;
            AccountRepository = accountRepository;
            ApplicationRepository = appRepository;
        }

        private static Guid accountId = new Guid("c2133cb4-48bb-473c-8415-1d55bc4d49c4");

        [HttpGet()]
        public async Task<ActionResult> GetAsync()
        {
            var apps = await ApplicationRepository.ListByAccountIdAsync(accountId);

            var vm = new ApplicationListResponseViewModel
            {
                Applications = apps.Select(a => new ApplicationViewModel(a))
            };

            return ModelState.GetJsonResultWithValidationErrors(vm);
        }

        [HttpPost()]
        public async Task<ActionResult> PostAsync(string name)
        {
            var app = await ApplicationRepository.AddAsync(name, accountId);

            var vm = new ApplicationGetResponseViewModel() { Application = new ApplicationViewModel(app) };

            return ModelState.GetJsonResultWithValidationErrors(vm);
        }

        [HttpDelete()]
        public async Task<ActionResult> DeleteAsync(Guid id)
        {
            await ApplicationRepository.DeleteAsync(id);

            return ModelState.GetJsonResultWithValidationErrors();
        }

    }
}
