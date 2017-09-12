using CsInvite.Website.Extensions;
using CsInvite.Website.Models;
using CsInvite.Website.Models.ViewModels.Account;
using CsInvite.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CsInvite.Website.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        private ApplicationDbContext db;
        private ILogger logger;
        private string steamApiKey;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, ApplicationDbContext db, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            steamApiKey = configuration["SteamApiKey"];
            this.logger = loggerFactory.CreateLogger<AccountController>();
            this.db = db;
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
                var steamId = ulong.Parse(new Uri(info.ProviderKey).Segments.Last());

                SteamPlayerSummaryDto player = null;
                using (var client = new HttpClient())
                {
                    // Query steam user summary endpoint
                    var response = await client.GetAsync($"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={steamApiKey}&steamids={steamId}");

                    // If result not OK, throw error
                    response.EnsureSuccessStatusCode();

                    // Deserialize json and return player DTO
                    var stringResponse = await response.Content.ReadAsStringAsync();

                    // Get display name
                    player = JsonConvert.DeserializeObject<SteamPlayerSummaryRootObject>(stringResponse).Response.Players[0];
                }

                var identity = info.Principal.Identity;
                var user = new User
                {
                    UserName = player?.PersonaName,
                    AvatarUrl = player?.PersonaName,
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
            return View(new SettingsViewModel
            {
                PermaBan = user.Permaban
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(SettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                user.Permaban = model.PermaBan;
                await userManager.UpdateAsync(user);
                await db.SaveChangesAsync();
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
