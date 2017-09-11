using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CsInvite.Models;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using CsInvite.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using CsInvite.Messaging.Steam;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using CsInvite.Models.ViewModels;
using CsInvite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CsInvite.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        private ILogger logger;
        private Steam steam;
        private string steamApiKey;

        public AccountController(Steam steam, SignInManager<User> signInManager, UserManager<User> userManager, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this.steam = steam;
            this.signInManager = signInManager;
            this.userManager = userManager;
            steamApiKey = configuration["SteamApiKey"];
            this.logger = loggerFactory.CreateLogger<AccountController>();
        }

        [HttpGet("~/Login"), HttpGet("Account/Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["returnUrl"] = returnUrl;

            return View(await HttpContext.GetExternalProvidersAsync());
        }

        [HttpPost("~/LoginExternal"), HttpPost("Account/LoginExternal")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginExternal([FromForm] string provider, string returnUrl = null)
        {
            ViewData["returnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(provider))
            {
                return BadRequest();
            }

            if (!await HttpContext.IsProviderSupportedAsync(provider))
            {
                return BadRequest();
            }
            
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, "/Account/Callback");
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Callback(string returnUrl = null, string remoteError = null)
        {
            Console.WriteLine("Callback: "+User?.Identity?.IsAuthenticated);

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            else if (result.RequiresTwoFactor || result.IsLockedOut)
            {
                return RedirectToAction(nameof(Login));
            }
            else
            {
                var steamId = new Uri(info.ProviderKey).Segments.Last();

                string displayName = "";
                using (var client = new HttpClient())
                {
                    // Query steam user summary endpoint
                    var response = await client.GetAsync($"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={steamApiKey}&amp;steamids={steamId}");

                    // If result not OK, throw error
                    response.EnsureSuccessStatusCode();

                    // Deserialize json and return player DTO
                    var stringResponse = await response.Content.ReadAsStringAsync();

                    // Get display name
                    var player = JsonConvert.DeserializeObject<SteamPlayerSummaryRootObject>(stringResponse).Response.Players[0];
                }

                var identity = info.Principal.Identity;
                var user = new User
                {
                    UserName = displayName,
                    SteamId = steamId
                };
                var userManagerResult = await userManager.CreateAsync(user);
                if (userManagerResult.Succeeded)
                {
                    userManagerResult = await userManager.AddLoginAsync(user, info);
                    if (userManagerResult.Succeeded)
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        logger.LogInformation(6, "User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }

                return RedirectToAction(nameof(Login));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var user = await userManager.GetUserAsync(User);

            ViewData["Maps"] = Maps.ActiveMaps;
            return View(new AccountSettingsViewModel
            {
                PermaBan = user.Permaban
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(AccountSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                user.Permaban = model.PermaBan;
                await userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet(), HttpPost()]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            logger.LogInformation(4, "User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
