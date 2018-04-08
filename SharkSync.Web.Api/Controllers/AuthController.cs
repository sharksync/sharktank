using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharkSync.Web.Api.Services;
using System.Threading.Tasks;

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
        [Route("Api/Auth/Start")]
        public IActionResult Start(string provider, string returnUrl)
        {
            // TODO: Validate return URL is the correct domain

            return Challenge(new AuthenticationProperties() { RedirectUri = Url.Action("Complete", new { returnUrl }) }, provider);
        }

        [HttpGet()]
        [Route("Api/Auth/Complete")]
        public IActionResult Complete(string returnUrl)
        {
            // TODO: Validate return URL is the correct domain

            return Redirect(returnUrl);
        }

        [HttpGet()]
        [Route("Api/Auth/Logout")]
        public async Task<IActionResult> Logout(string returnUrl)
        {
            // TODO: Validate return URL is the correct domain

            await HttpContext.SignOutAsync();

            return Ok();
        }
    }
}
