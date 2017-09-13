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
using CsInvite.Website.Models.Steam;

namespace CsInvite.Website.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        private ApplicationDbContext db;
        private ILogger logger;
        private Steam steam;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, ApplicationDbContext db, Steam steam, ILoggerFactory loggerFactory)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.logger = loggerFactory.CreateLogger<AccountController>();
            this.db = db;
            this.steam = steam;
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

                SteamPlayerSummary player = await steam.GetSteamPlayer(steamId);

                var identity = info.Principal.Identity;
                var user = db.Users.FirstOrDefault(u => u.SteamId == steamId);
                var userExists = user != null;
                if (!userExists)
                {
                    user = new User
                    {
                        SteamId = steamId
                    };
                }
                user.UserName = player?.PersonaName;
                user.AvatarUrl = player?.AvatarFull;

                IdentityResult userManagerResult = null;
                if (!userExists)
                {
                    userManagerResult = await userManager.CreateAsync(user);
                }
                if (userExists || userManagerResult.Succeeded)
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
