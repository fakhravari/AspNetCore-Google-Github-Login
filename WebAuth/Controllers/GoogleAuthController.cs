using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAuth.Controllers
{
    [Route("auth/google")]
    public class GoogleAuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public GoogleAuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            var stateValue = Guid.NewGuid().ToString();

            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("Callback", "GoogleAuth", null, Request.Scheme),
                Items = { { "prompt", "select_account" } }
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> CallbackAsync()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                var error = result.Failure?.Message;
                var properties = result.Properties?.Items;
                return BadRequest($"Authentication failed: {error}");
            }

            var Name = result.Principal.FindFirst(ClaimTypes.Name).Value;
            var Email = result.Principal.FindFirst(ClaimTypes.Email).Value;
            var NameIdentifier = result.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var GivenName = User.FindFirst(ClaimTypes.GivenName)?.Value;
            var Surname = User.FindFirst(ClaimTypes.Surname)?.Value;

            return Json(new { Name, Email, NameIdentifier, GivenName, Surname });
        }
    }
}
