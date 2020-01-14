using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleAuth.Client.AspNetCore.Attributes;
using SimpleAuth.Client.AspNetCore.Models;
using SimpleAuth.Client.AspNetCore.Services;
using SimpleAuth.Client.Models;
using SimpleAuth.Client.Services;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Shared.Enums;

namespace WebApiPlayground.Controllers
{
    [ApiController]
    [Route("weatherforecast")]
    [SaModule("weatherforecast", false)]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ISimpleAuthConfigurationProvider _simpleAuthConfigurationProvider;
        private readonly ITenantProvider _tenantProvider;
        private readonly IAuthenticationInfoProvider _authenticationInfoProvider;

        public WeatherForecastController(ISimpleAuthConfigurationProvider simpleAuthConfigurationProvider,
            ITenantProvider tenantProvider, IAuthenticationInfoProvider authenticationInfoProvider)
        {
            _simpleAuthConfigurationProvider = simpleAuthConfigurationProvider;
            _tenantProvider = tenantProvider;
            _authenticationInfoProvider = authenticationInfoProvider;
        }

        [HttpGet("0")]
        public IEnumerable<WeatherForecast> GetWithoutPermission()
        {
            return GetSample();
        }

        [HttpGet]
        [SaPermission(Permission.View, "a")]
        [SaPermission(Permission.View, "b")]
        public IEnumerable<WeatherForecast> GetWithControllerModule()
        {
            return GetSample();
        }

        [HttpGet("1c")]
        public async Task<IActionResult> GetWithControllerModule_WithoutDeclared_But_DeclareAtRuntime()
        {
            return await ResponseAsync(
                new ClaimsBuilder()
                    .WithModule(
                        GetType().GetMethod(
                            nameof(GetWithControllerModule_WithoutDeclared_But_DeclareAtRuntime)
                        )
                    )
                    .WithPermission(Permission.View, "a")
                    .WithPermission(Permission.View, "b")
            );
        }

        [HttpGet("1d")]
        [SaPermission(Permission.View, "a")]
        public async Task<IActionResult> GetWithControllerModule_With_Declared_and_Extra_Addition_At_Runtime()
        {
            return await ResponseAsync(
                    ClaimsBuilder.FromMetaData(
                        GetType().GetMethod(
                            nameof(GetWithControllerModule_With_Declared_and_Extra_Addition_At_Runtime)
                        )
                    )
                    .WithPermission(Permission.View, "b")
            );
        }

        private async Task<IActionResult> ResponseAsync(ClaimsBuilder claimsBuilder)
        {
            var claims = await _authenticationInfoProvider.GetClaims(HttpContext);
            var requireClaims = claimsBuilder.Build(_simpleAuthConfigurationProvider,
                await _tenantProvider.GetTenantAsync(HttpContext)).ToArray();

            var missingClaims = (
                    await claims.GetMissingClaimsAsync(requireClaims, HttpContext.RequestServices)
                )
                .OrEmpty()
                .ToArray();

            string Join(IEnumerable<SimpleAuthorizationClaim> col) =>
                string.Join(',', col.SelectMany(x => x.ClientRoleModel.SubModules));

            Console.WriteLine($"{nameof(requireClaims)}: {requireClaims.Length}. {Join(requireClaims)}");
            Console.WriteLine($"{nameof(missingClaims)}: {missingClaims.Length}. {Join(missingClaims)}");

            if (missingClaims.Any())
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Content = $"Require {missingClaims[0].ClientRoleModel}"
                };
            }

            return new ContentResult
            {
                StatusCode = StatusCodes.Status200OK,
                Content = JsonSerializer.Serialize(GetSample())
            };
        }

        [HttpGet("2")]
        [SaModule("best")] // Override attribute of class
        [SaPermission(Permission.View)]
        public IEnumerable<WeatherForecast> GetWithOverridenModule()
        {
            return GetSample();
        }

        [HttpGet("2c")]
        [SaModule("best", false)] // Override attribute of class
        public async Task<IActionResult> GetWithOverridenModule_WithoutDeclared_But_DeclareAtRuntime()
        {
            return await ResponseAsync(
                new ClaimsBuilder()
                    .LoadFromMeta(
                        GetType().GetMethod(
                            nameof(GetWithOverridenModule_WithoutDeclared_But_DeclareAtRuntime)
                        )
                    )
                    .WithPermission(Permission.View)
            );
        }

        [HttpGet("2d")]
        [SaModule("best")] // Override attribute of class
        public async Task<IActionResult> GetWithOverridenModule_CannotDoThis_Because_Module_Is_Restricted()
        {
            return await ResponseAsync(
                new ClaimsBuilder()
                    .WithModule(
                        GetType().GetMethod(
                            nameof(GetWithOverridenModule_CannotDoThis_Because_Module_Is_Restricted)
                        )
                    )
                    .WithPermission(Permission.View)
            );
            // The attribute SaModule has to be removed in order to process this method
        }

        [HttpGet("3")]
        [SaModule("best")] // Override attribute of class
        [SaPermission(Permission.View, "a")]
        [SaPermission(Permission.View, "b")]
        public IEnumerable<WeatherForecast> GetWithMissingPermission()
        {
            return GetSample();
        }

        private IEnumerable<WeatherForecast> GetSample()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();
        }
    }
}