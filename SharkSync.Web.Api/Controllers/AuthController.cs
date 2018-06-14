using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharkSync.Web.Api.Services;
using SharkSync.Web.Api.ViewModels;
using SharkSync.Interfaces.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SharkSync.Web.Api.Controllers
{
    public class AuthController : Controller
    {
        ILogger Logger { get; set; }

        AuthService AuthService { get; set; }

        AppSettings AppSettings { get; set; }

        public AuthController(ILogger<AuthController> logger, AuthService authService, IOptions<AppSettings> appSettingsOptions)
        {
            Logger = logger;
            AuthService = authService;
            AppSettings = appSettingsOptions.Value;
        }

        [HttpGet()]
        [Route("Api/Auth/Start")]
        public Task Start(string provider)
        {
            return HttpContext.ChallengeAsync(provider, new AuthenticationProperties() { RedirectUri = Url.Action("Complete") });
        }

        [HttpGet()]
        [Route("Api/Auth/Complete")]
        public IActionResult Complete()
        {
            return Redirect($"{AppSettings.ClientAppRootUrl}/Console/Apps");
        }

        [HttpGet()]
        [Route("Api/Auth/Details")]
        [Authorize]
        public async Task<IActionResult> Details()
        {
            var loggedInAccount = await AuthService.GetLoggedInAccountAsync(User);
            if (loggedInAccount == null)
                return Unauthorized();

            var vm = new AuthDetailsViewModel()
            {
                LoggedInUser = new UserDetailsViewModel
                {
                    Id = loggedInAccount.Id,
                    Name = loggedInAccount.Name,
                    EmailAddress = loggedInAccount.EmailAddress,
                    AvatarUrl = loggedInAccount.AvatarUrl
                }
            };

            // Populate the AvatarUrl with a stock image if we don't have one
            if (string.IsNullOrWhiteSpace(loggedInAccount.AvatarUrl))
            {
                if (!string.IsNullOrWhiteSpace(loggedInAccount.GitHubId))
                    vm.LoggedInUser.AvatarUrl = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyRpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMy1jMDExIDY2LjE0NTY2MSwgMjAxMi8wMi8wNi0xNDo1NjoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNiAoTWFjaW50b3NoKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDpFNTE3OEEyQTk5QTAxMUUyOUExNUJDMTA0NkE4OTA0RCIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDpFNTE3OEEyQjk5QTAxMUUyOUExNUJDMTA0NkE4OTA0RCI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOkU1MTc4QTI4OTlBMDExRTI5QTE1QkMxMDQ2QTg5MDREIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOkU1MTc4QTI5OTlBMDExRTI5QTE1QkMxMDQ2QTg5MDREIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+m4QGuQAAAyRJREFUeNrEl21ojWEYx895TDPbMNlBK46IUiNmPvHBSUjaqc0H8pF5+aDUKPEBqU2NhRQpX5Rv5jWlDIWlMCv7MMSWsWwmb3tpXub4XXWdPHvc9/Gc41nu+nedc7/8r/99PffLdYdDPsvkwsgkTBwsA/PADJCnzX2gHTwBt8Hl7p537/3whn04XoDZDcpBlk+9P8AFcAghzRkJwPF4zGGw0Y9QS0mAM2AnQj77FqCzrtcwB1Hk81SYojHK4DyGuQ6mhIIrBWB9Xm7ug/6B/nZrBHBegrkFxoVGpnwBMSLR9EcEcC4qb8pP14BWcBcUgewMnF3T34VqhWMFkThLJAalwnENOAKiHpJq1FZgI2AT6HZtuxZwR9GidSHtI30jOrbawxlVX78/AbNfhHlomEUJJI89O2MqeE79T8/nk8nMBm/dK576hZgmA3cp/R4l9/UeSxiHLVIlNm4nFfT0bxyuIj7LHRTKai+zdJobwMKzcZSJb0ePV5PKN+BqAAKE47UlMnERELMM3EdYP/yrd+XYb2mOiYBiQ8OQnoRBlXrl9JZix7D1pHTazu4MoyBcnYamqAjIMTR8G4FT8LuhLsexXYYjICBiqhQBvYb6fLZIJCjPypVvaOoVAW2WcasCnL2Nq82xHJNSqlCeFcDshaPK0twkAhosjZL31QYw+1rlMpWGMArl23SBsZZO58F2tlJXmjOXS+s4WGvpMiBJT/I2PInZ6lIs9/hBsNS1hS6BG0DSqmYEDRlCXQrmy50P1oDRKTSegmNbUsA0zDMwRhPJXeCE3vWLPQMvan6X8AgIa1vcR4AkGZkDR4ejJ1UHpsaVI0g2LInpOsNFUud1rhxSV+fzC9Woz2EZkWQuja7/B+jUrgtIMpy9YCW4n4K41YfzRneW5E1KJTe4B2Zq1Q5EHEtj4U3AfEzR5SVY4l7QYQPJdN2as7RKBF0BPZqqH4VgMAMBL8Byxr7y8zCZiDlnOcEKIPmUpgB5Z2ww5RdOiiRiNajUmWda5IG6WbhsyY2fx6m8gLcoJDJFkH219M3We1+cnda93pfycZpIJEL/s/wSYADmOAwAQgdpBAAAAABJRU5ErkJggg==";
                else if (!string.IsNullOrWhiteSpace(loggedInAccount.GoogleId))
                    vm.LoggedInUser.AvatarUrl = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAB1FBMVEUAAAD/AADsQzXrQzbqQzXpRDTqQzXqQjXqQzXqRDToRjbqQzXqQzXqQzXqRDXrRTHxRznqQzXpQzXqQzXpQzTjOTnrQTTqQzXqQzToRDPpQjfqQzXqQzXpQzbsRDjqQzXoRDfqQzXqQzXrQzXqRDXqQjXqQzbqQzXqQzXqQzTsQjn/rxDqRDTqQzXqRDXoRjr8vAX5sQrsTDHrQjT//wD7vAT7ugbvZif6vgX1jRjqQzX7vAT5sAo/jss9kb77uwRAieH6vQX6vQX6vAX6vQVBiOnkuA4/i9r6vAWstCQ1qVI3pFI9k7E+j8n4vAZvrT00qFM0p1M/jso+lLn8vAXkug1CqU00qFQ8lK45l6ffvxg3qFI0qFM9lqpBieU/jc00qFQzqFM1p09An2A0plk/jdA5lqwtpVo0qFM0qFM1qVQ1qFM1p1I0p1IzqVI1qFM1qFM+j8gzplM0qFNAi91Cl6o1qVM0qFM/jNQ7m602qFE0qFM1qFMxpVI0qVM0qFM0qFM0qFQktkk4p1A0qFM0qVM0qVMzo1IA/wA2p1M0qFIzqFIzp1QzqFI0p1PqQzX7vAVChfRChu9BhvA0qFNChfJBhfM1p1o9krs5mpQ3oHf////8WgVEAAAAj3RSTlMAATVylKafh24xIZXl5o8aEpD5940JJ9nWLUby8Tkp8Dje+rt8YF/7/pwbIOX2VhaV58xZAeP7xTfK63Kv47x8+6VgpGH+sfU1y+wcfd/7xv6d36SU6NJYYjEg5fdL/OePnx0IcPxQEdz7vX5gXXOl7tko8PgbRPH6OCbYzB+O+PaJByCT4+YZATRwpIZtMQ4TRwgAAAABYktHRJvv2FeEAAAAB3RJTUUH4QgKAjghFnOx6QAAAWBJREFUOMtjYCABMDIxs7CysXNwMmKV5uLm6YcCXj5+DGkBQaF+JCAsIooqLybejwYkJJHlpaTR5ftlZJHk5eQx5RWQ7VeEiiopq6iqqSiro+lnEIRIa2hqQf3DiqKfQVsHLK+rhxDSR/GBgaERSL8xrvAzMZ1gZq7Rr4kzgC0mAIGllRZOBdYgBRNsYFxbZGAHdgJYgT1MwURk4AAScQQrcMKqYBJIxBmswAWrgsmErHBFONINqwJ3kIgHSN7TyxvVbz5gBb7QgPLzD5gSiKogCKwgGMwOCQ2bMmVKQDiyfMRUkPy0SDAnKnoKCMQgqYiNAxsQD+UmgBVMCUhMgvCTU1LB8lPToArSMyAqpmRmZefk5uUXTJk+A6SgEG5iUfEUdDBz8sSSUoSdZeUYKmZVVCK7uqoaXUFNLaq/0+vqkaUbGpsw0kVzSytMui2hHWvS6ejsaulO7Ont6yAlywMAh+DsfszQdOIAAAAldEVYdGRhdGU6Y3JlYXRlADIwMTctMDgtMTBUMDI6NTY6MzMrMDA6MDAy1cN5AAAAJXRFWHRkYXRlOm1vZGlmeQAyMDE3LTA4LTEwVDAyOjU2OjMzKzAwOjAwQ4h7xQAAAABJRU5ErkJggg==";
                else if (!string.IsNullOrWhiteSpace(loggedInAccount.MicrosoftId))
                    vm.LoggedInUser.AvatarUrl = "data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjwhLS0gR2VuZXJhdG9yOiBBZG9iZSBJbGx1c3RyYXRvciAyMi4xLjAsIFNWRyBFeHBvcnQgUGx1Zy1JbiAuIFNWRyBWZXJzaW9uOiA2LjAwIEJ1aWxkIDApICAtLT4NCjxzdmcgdmVyc2lvbj0iMS4xIiBpZD0iQ2FscXVlXzEiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiIHg9IjBweCIgeT0iMHB4Ig0KCSB3aWR0aD0iNDM5cHgiIGhlaWdodD0iNDM5cHgiIHZpZXdCb3g9IjAgMCA0MzkgNDM5IiBzdHlsZT0iZW5hYmxlLWJhY2tncm91bmQ6bmV3IDAgMCA0MzkgNDM5OyIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSI+DQo8c3R5bGUgdHlwZT0idGV4dC9jc3MiPg0KCS5zdDB7ZmlsbDojRjNGM0YzO30NCgkuc3Qxe2ZpbGw6I0YzNTMyNTt9DQoJLnN0MntmaWxsOiM4MUJDMDY7fQ0KCS5zdDN7ZmlsbDojMDVBNkYwO30NCgkuc3Q0e2ZpbGw6I0ZGQkEwODt9DQo8L3N0eWxlPg0KPHJlY3QgY2xhc3M9InN0MCIgd2lkdGg9IjQzOSIgaGVpZ2h0PSI0MzkiLz4NCjxyZWN0IHg9IjE3IiB5PSIxNyIgY2xhc3M9InN0MSIgd2lkdGg9IjE5NCIgaGVpZ2h0PSIxOTQiLz4NCjxyZWN0IHg9IjIyOCIgeT0iMTciIGNsYXNzPSJzdDIiIHdpZHRoPSIxOTQiIGhlaWdodD0iMTk0Ii8+DQo8cmVjdCB4PSIxNyIgeT0iMjI4IiBjbGFzcz0ic3QzIiB3aWR0aD0iMTk0IiBoZWlnaHQ9IjE5NCIvPg0KPHJlY3QgeD0iMjI4IiB5PSIyMjgiIGNsYXNzPSJzdDQiIHdpZHRoPSIxOTQiIGhlaWdodD0iMTk0Ii8+DQo8L3N2Zz4NCg==";
            }

            return ModelState.GetJsonResultWithValidationErrors(vm);
        }

        [HttpGet()]
        [Route("Api/Auth/Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return Ok();
        }
        
        [HttpGet()]
        [Route("Api/Auth/Login")]
        public IActionResult Login()
        {
            // Return unauthorized to trigger the login page in the client
            return Unauthorized();
        }

        [HttpGet()]
        [Route("Api/Auth/AccessDenied")]
        public IActionResult AccessDenied()
        {
            // Return Forbidden to inform the user they can't access this page
            return Forbid();
        }
    }
}
