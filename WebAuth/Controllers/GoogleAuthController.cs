using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

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
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("Callback", "GoogleAuth", null, Request.Scheme)
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback()
        {
            var result = await HttpContext.AuthenticateAsync();
            if (!result.Succeeded)
            {
                return BadRequest("Authentication failed.");
            }

            var accessToken = result.Properties.GetTokenValue("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Failed to retrieve access token.");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync("https://www.googleapis.com/userinfo/v2/me");
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Could not retrieve profile info from Google.");
                }

                var profileJson = await response.Content.ReadAsStringAsync();
                var profileData = JsonSerializer.Deserialize<GoogleProfile>(profileJson);

                return Ok(profileData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error.");
            }
        }


    }

    public class GoogleProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
    }
}
