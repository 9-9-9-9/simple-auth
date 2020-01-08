using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleAuth.Client.Models;
using SimpleAuth.Client.Services;
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Models;

namespace WebApiPlayground.Controllers
{
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IJsonService _jsonService;

        public AuthController(IJsonService jsonService)
        {
            _jsonService = jsonService;
        }

        private async Task<IEnumerable<RoleModel>> GetRoleModels()
        {
            await Task.CompletedTask;
            return YieldResults();

            //
            IEnumerable<RoleModel> YieldResults()
            {
                yield return Rm("g.a.e.t.weatherforecast", Permission.View);
                yield return Rm("g.a.e.t.weatherforecast.*", Permission.View);
                yield return Rm("g.a.e.t.best", Permission.View);
                yield return Rm("g.a.e.t.best.a", Permission.View);
            }

            //
            RoleModel Rm(string roleId, Permission permission) => new RoleModel
            {
                Role = roleId,
                Permission = permission.Serialize()
            };
        }

        [AllowAnonymous]
        [HttpGet, HttpPost, Route("si")]
        public async Task<IActionResult> SignIn()
        {
            var roleModels = await GetRoleModels();

            const string email = "9-9-9-9";
            
            var claimsIdentity = new ClaimsIdentity(
                new []
                {
                    new Claim(nameof(SimpleAuthorizationClaims), _jsonService.Serialize(new SimpleAuthorizationClaims
                    {
                        Claims = roleModels.Select(x => new SimpleAuthorizationClaim(x)).ToList()
                    })), 
                },
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