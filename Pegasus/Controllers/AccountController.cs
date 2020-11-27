﻿using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Pegasus.Extensions;
using Pegasus.Library.Api;
using Pegasus.Library.JwtAuthentication;
using Pegasus.Library.Models.Account;
using Pegasus.Models.Account;
using Pegasus.Services;


namespace Pegasus.Controllers
{
    [Authorize(Roles = "PegasusUser")]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IApiHelper _apiHelper;
        private readonly IJwtTokenAccessor _tokenAccessor;
        private readonly IAccountsEndpoint _accountsEndpoint;
        private readonly IAuthenticationEndpoint _authenticationEndpoint;
        private readonly SignInManager _signInManager;

        public AccountController(ILogger<AccountController> logger, IApiHelper apiHelper, IJwtTokenAccessor tokenAccessor, 
            IAccountsEndpoint accountsEndpoint, IAuthenticationEndpoint authenticationEndpoint, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _apiHelper = apiHelper;
            _tokenAccessor = tokenAccessor;
            _accountsEndpoint = accountsEndpoint;
            _authenticationEndpoint = authenticationEndpoint;
            _signInManager = new SignInManager(httpContextAccessor, accountsEndpoint, apiHelper, tokenAccessor);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var credentials = new UserCredentials { Username = model.Email, Password = model.Password };
                var authenticatedUser = await _authenticationEndpoint.Authenticate(credentials);

                if (authenticatedUser.Authenticated)
                {
                    var signInResult = await _signInManager.SignInOrTwoFactor(authenticatedUser);
                    if (signInResult.Success)
                    {
                        _logger.LogInformation("User logged in.");
                        returnUrl ??= Url.Content("~/");
                        return RedirectToLocal(returnUrl);
                    }
                    if (signInResult.RequiresTwoFactor)
                    {
                        return RedirectToAction(nameof(LoginWith2Fa), new { returnUrl });
                    }
                }
                
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> LoginWith2Fa(string returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2FaViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2Fa(LoginWith2FaViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (userId == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var verify2FaToken = new VerifyTwoFactorModel
            {
                UserId = userId,
                Code = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty),
                RememberMachine = model.RememberMachine
            };

            var result = await _accountsEndpoint.VerifyTwoFactorTokenAsync(verify2FaToken);
            if (result.Verified)
            {
                var authenticatedUser = await _authenticationEndpoint.Authenticate2Fa(userId);
                var accessTokenResult = _tokenAccessor.GetAccessTokenWithClaimsPrincipal(authenticatedUser);

                if (model.RememberMachine)
                {
                    await _signInManager.RememberTwoFactorClientAsync(userId);
                }
                //Need to re-sign in with 2fa
                await HttpContext.SignOutAsync();
                await HttpContext.SignInAsync(accessTokenResult.ClaimsPrincipal, accessTokenResult.AuthenticationProperties);

                _apiHelper.AddTokenToHeaders(accessTokenResult.AccessToken);

                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", authenticatedUser.Username);
                returnUrl ??= Url.Content("~/");
                return LocalRedirect(returnUrl);
            }

            _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.",  userId);
            ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _apiHelper.RemoveTokenFromHeaders();
            _logger.LogInformation("User logged out.");
            ViewData["ReturnUrl"] = returnUrl;
            return RedirectToLocal("/Account/Login");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                model.BaseUrl = Url.ResetPasswordBaseUrl(Request.Scheme);
                await _accountsEndpoint.ForgotPassword(model);
                return View("ForgotPasswordConfirmation");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string userId = null, string code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                var resetPasswordViewModel = new ResetPasswordModel
                {
                    UserId = userId,
                    ResetCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                };

                return View(resetPasswordViewModel);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _accountsEndpoint.ResetPassword(model);

            if (response.Succeeded)
            {
                return View("ResetPasswordConfirmation");
            }

            //TODO Does this not leave us security vulnerable. i.e. Email/User exists?
            foreach (var error in response.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
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
