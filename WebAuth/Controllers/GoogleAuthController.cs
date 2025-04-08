using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace WebAuth.Controllers;

[Route("auth/google")]
public class GoogleAuthController : Controller
{
    [HttpGet("login")]
    public IActionResult Login()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("Callback"),
            Items =
            {
                { "prompt", "select_account" }
            }
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }


    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            var error = result.Failure?.Message;
            var properties = result.Properties?.Items;
            return BadRequest($"Authentication failed: {error}");
        }

        var claims = result.Principal.Claims.Select(v => new { Value = v.Value, Key = v.ValueType.ToString() }).ToList();

        return Json(claims);
    }
}
