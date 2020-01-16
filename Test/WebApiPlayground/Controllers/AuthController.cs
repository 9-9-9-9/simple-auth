using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleAuth.Client.Models;
using SimpleAuth.Client.Services;

namespace WebApiPlayground.Controllers
{
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        public const string UserId = "theone";
        
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserAuthService _userAuthService;

        public AuthController(IServiceProvider serviceProvider, IUserAuthService userAuthService)
        {
            _serviceProvider = serviceProvider;
            _userAuthService = userAuthService;
        }

        [AllowAnonymous]
        [HttpGet, HttpPost, Route("si")]
        public async Task<IActionResult> SignIn()
        {
            var saUserModel = await _userAuthService.GetUserAsync(UserId);

            var claim = await saUserModel.GenerateSimpleAuthClaimAsync(_serviceProvider);
            if (claim == default)
                return NotFound();
            
            var claimsIdentity = new ClaimsIdentity(
                new[] { claim },
                CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                IssuedUtc = DateTimeOffset.Now,
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Ok();
        }

        [Route("so")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}