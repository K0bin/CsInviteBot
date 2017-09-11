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

namespace CsInvite.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        private ILogger logger;
        private Steam steam;

        public AccountController(Steam steam, SignInManager<User> signInManager, UserManager<User> userManager, ILoggerFactory loggerFactory)
        {
            this.steam = steam;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.logger = loggerFactory.CreateLogger<AccountController>();
        }

        [HttpGet("~/Login")]
        public async Task<IActionResult> Login()
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return View(await HttpContext.GetExternalProvidersAsync());
        }

        [HttpPost("~/Login")]
        public async Task<IActionResult> Login([FromForm] string provider)
        {
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
                var identity = info.Principal.Identity;
                var user = new User
                {
                    UserName = identity.Name
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

        [HttpGet(), HttpPost()]
        public IActionResult Logout()
        {
            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            return SignOut(new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme);
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
