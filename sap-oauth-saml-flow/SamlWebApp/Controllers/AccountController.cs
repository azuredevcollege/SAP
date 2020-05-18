using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SamlWebApp.Models;

namespace SamlWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public static string Token {get; set;}
        public static string Token64 {get; set;}

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        public IActionResult Login()
        {
            var redirecturl = Url.Content("~/");
            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = redirecturl
                },
                WsFederationDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var redirectUrl = Url.Content("~/");
            return SignOut(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                WsFederationDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GetAccessToken()
        {
            var parameters = new Dictionary<string, string>
            {
                {"grant_type", "urn:ietf:params:oauth:grant-type:saml2-bearer"},
                {"client_id", "<ClientID>"},
                {"client_secret", "<ClientSecret>"},
                {"assertion", Token64},
                {"scope", "openid <add additional scopes as needed>"}
            };
            var client = new HttpClient();
            var result = await client.PostAsync("https://login.microsoftonline.com/<TenantID>/oauth2/v2.0/token", new FormUrlEncodedContent(parameters));
            var accesstoken = result.Content.ReadAsStringAsync();
            return View("Index");
        }

        public void LoginSamlp()
        {
            var redirecturl = "https://localhost:5001/account/consumesamlp";
            var samlEndpoint = "https://login.microsoftonline.com/<TenantID>/saml2";
            var request = new Samlp.AuthRequest(
                "spn:<App ID>",
                redirecturl
            );

            Response.Redirect(request.GetRedirectUrl(samlEndpoint));
        }

        public void ConsumeSamlp()
        {
            var result = Request.Form["SAMLResponse"];
            var authResp = new Samlp.AuthResponse();
            authResp.LoadXmlFromBase64(result);

            Token = authResp.GetAssertion();
            Token64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(Token), Base64FormattingOptions.None);
        }
    }
}
