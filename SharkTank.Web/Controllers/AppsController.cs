using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharkSync.Scale;
using SharkSync.Scale.Tables;

namespace SharkTank.Web.Controllers
{
    [Route("api/[controller]")]
    public class AppsController : Controller
    {
        ILogger Logger { get; set; }

        IScaleContext ScaleContext { get; set; }

        public AppsController(ILogger<AppsController> logger, IScaleContext scaleContext)
        {
            Logger = logger;
            ScaleContext = scaleContext;
        }

        [HttpGet()]
        public async Task<IEnumerable<AppViewModel>> GetAsync()
        {
            //List<Application> apps = await ScaleContext.Query<Application>(ScaleContext.SystemPartition, "application", limit: 50);

            //return apps.Select(a => new AppViewModel
            //{
            //    AppId = a.Id,
            //    AccessKey = a.AccessKey
            //});
            return new List<AppViewModel>
            {
                new AppViewModel() { AppId = Guid.NewGuid(), AccessKey = Guid.NewGuid() },
                new AppViewModel() { AppId = Guid.NewGuid(), AccessKey = Guid.NewGuid() },
            };
        }

        [HttpDelete()]
        public async Task DeleteAsync(Guid id)
        {
            await Task.FromResult(1);
        }

        public class AppViewModel
        {
            public Guid AppId { get; set; }
            public Guid AccessKey { get; set; }
        }
    }
}
