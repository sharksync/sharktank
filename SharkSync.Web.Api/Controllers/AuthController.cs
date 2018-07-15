using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharkSync.Web.Api.Services;
using SharkSync.Web.Api.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace SharkSync.Web.Api.Controllers
{
    public class AuthController : Controller
    {
        ILogger Logger { get; set; }

        AuthService AuthService { get; set; }

        IAntiforgery Antiforgery { get; set; }

        IHttpClientFactory HttpClientFactory { get; set; }

        public AuthController(ILogger<AuthController> logger, AuthService authService, IAntiforgery antiforgery, IHttpClientFactory httpClientFactory)
        {
            Logger = logger;
            AuthService = authService;
            Antiforgery = antiforgery;
            HttpClientFactory = httpClientFactory;
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
            //return Redirect($"{AppSettings.ClientAppRootUrl}/Console/LoginComplete");
            return Redirect($"/Console/LoginComplete");
        }

        [HttpGet()]
        [Route("Api/Auth/Details")]
        [Authorize]
        public async Task<IActionResult> Details()
        {
            var loggedInAccount = await AuthService.GetLoggedInAccountAsync(User);
            if (loggedInAccount == null)
                return Unauthorized();

            var tokens = Antiforgery.GetAndStoreTokens(HttpContext);

            var vm = new AuthDetailsViewModel()
            {
                LoggedInUser = new UserDetailsViewModel
                {
                    Id = loggedInAccount.Id,
                    Name = loggedInAccount.Name,
                    EmailAddress = loggedInAccount.EmailAddress,
                    XSRFToken = tokens.RequestToken,
                    HasAvatarUrl = !string.IsNullOrWhiteSpace(loggedInAccount.AvatarUrl),
                    AccountType = !string.IsNullOrWhiteSpace(loggedInAccount.GitHubId) ? "Github" :
                                    !string.IsNullOrWhiteSpace(loggedInAccount.GoogleId) ? "Google" :
                                    !string.IsNullOrWhiteSpace(loggedInAccount.MicrosoftId) ? "Microsoft" :
                                    "Unknown"

                }
            };

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
        [Route("Api/Auth/ProfilePicture")]
        public async Task<IActionResult> ProfilePicture()
        {
            var loggedInAccount = await AuthService.GetLoggedInAccountAsync(User);
            if (loggedInAccount == null)
                return Unauthorized();

            string avatarUrl = loggedInAccount.AvatarUrl;

            if (!string.IsNullOrWhiteSpace(avatarUrl))
            {
                var client = HttpClientFactory.CreateClient();
                var response = await client.GetAsync(avatarUrl);
                response.EnsureSuccessStatusCode();

                // Since we are in a lambda, we can't stream the bytes live, copy it into memory first
                var bytes = await response.Content.ReadAsByteArrayAsync();
                Logger.LogInformation($"Read profile image into byte[] at size: {bytes.Length}");
                return File(bytes, response.Content.Headers.ContentType.MediaType);
            }

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
