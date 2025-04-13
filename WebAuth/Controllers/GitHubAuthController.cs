using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAuth.Controllers
{
    [Route("auth/gitHub")]
    public class GitHubAuthController : Controller
    {
        [HttpGet("login")]
        public IActionResult Login()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("Callback", "GitHubAuth", null, Request.Scheme),
                Items = { { "prompt", "select_account" } }
            };

            return Challenge(properties, "GitHub");
        }

        [HttpGet("callback")]
        public async Task<IActionResult> CallbackAsync()
        {
            var result = await HttpContext.AuthenticateAsync();

            if (!result.Succeeded)
            {
                var error = result.Failure?.Message;
                var properties = result.Properties?.Items;
                return BadRequest($"Authentication failed: {error}");
            }

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;

            var NameIdentifier = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var Name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var FullName = claims.FirstOrDefault(c => c.Type == "urn:github:name")?.Value;
            var ProfileUrl = claims.FirstOrDefault(c => c.Type == "urn:github:url")?.Value;
            var AvatarUrl = claims.FirstOrDefault(c => c.Type == "urn:github:avatar")?.Value;


            return Json(new { Name, Email, NameIdentifier, FullName, ProfileUrl, AvatarUrl });
        }
    }
}
