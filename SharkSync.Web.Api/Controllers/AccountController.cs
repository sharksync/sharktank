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
    public class AccountController : Controller
    {
        ILogger Logger { get; set; }

        IAccountRepository AccountRepository { get; set; }

        IApplicationRepository ApplicationRepository { get; set; }

        public AccountController(ILogger<AccountController> logger, IAccountRepository accountRepository, IApplicationRepository appRepository)
        {
            Logger = logger;
            AccountRepository = accountRepository;
            ApplicationRepository = appRepository;
        }

        [Route("account/apps")]
        [HttpGet()]
        public async Task<ApplicationListResponseViewModel> GetAsync()
        {
            Guid accountId = new Guid("c2133cb4-48bb-473c-8415-1d55bc4d49c4");
            var apps = await ApplicationRepository.ListByAccountIdAsync(accountId);

            var vm = new ApplicationListResponseViewModel
            {
                Applications = apps.Select(a => new ApplicationViewModel(a))
            };

            return vm;
        }

        [Route("account/apps")]
        [HttpPost()]
        public async Task<ApplicationGetResponseViewModel> PostAsync(string appName)
        {
            Guid appId = new Guid("eca29d74-a255-445c-a71d-ad74be90d9c7");
            var app = await ApplicationRepository.GetByIdAsync(appId);

            return new ApplicationGetResponseViewModel() { Application = new ApplicationViewModel(app) };
        }

        [Route("account/apps")]
        [HttpDelete()]
        public async Task DeleteAsync(Guid id)
        {
            await Task.FromResult(1);
        }

    }
}
