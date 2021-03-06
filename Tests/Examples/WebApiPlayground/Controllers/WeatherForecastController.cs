﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleAuth.Client.AspNetCore.Attributes;
using SimpleAuth.Client.AspNetCore.Models;
using SimpleAuth.Client.Extensions.PermissionGenerator.Attributes;
using SimpleAuth.Shared.Enums;

namespace WebApiPlayground.Controllers
{
    [ApiController]
    [Route("weatherforecast")]
    [SaModule("weatherforecast", false)]
    [SaGroup("user")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet("0")]
        public IEnumerable<WeatherForecast> GetWithoutPermission()
        {
            return GetSample();
        }

        [HttpGet]
        [SaPermission(Verb.View, "a")]
        [SaPermission(Verb.View, "b")]
        public IEnumerable<WeatherForecast> GetWithControllerModule()
        {
            return GetSample();
        }

        [HttpGet("1c")]
        public async Task<IActionResult> GetWithControllerModule_WithoutDeclared_But_DeclareAtRuntime()
        {
            return await ResponseAsync(
                ClaimsBuilder.FromMetaData<WeatherForecastController>()
                    .WithPermission(Verb.View, "a")
                    .WithPermission(Verb.View, "b")
            );
        }

        [HttpGet("1d")]
        [SaPermission(Verb.View, "a")]
        public async Task<IActionResult> GetWithControllerModule_With_Declared_and_Extra_Addition_At_Runtime()
        {
            return await ResponseAsync(
                ClaimsBuilder.FromMetaData<WeatherForecastController>()
                    .WithPermission(Verb.View, "b")
            );
        }

        private async Task<IActionResult> ResponseAsync(ClaimsBuilder claimsBuilder)
        {
            var missingClaims = await HttpContext.GetMissingClaimsAsync(claimsBuilder);

            if (missingClaims.Any())
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Content = $"Require {missingClaims.First().ClientPermissionModel}"
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
        [SaPermission(Verb.View)]
        public IEnumerable<WeatherForecast> GetWithOverridenModule()
        {
            return GetSample();
        }

        [HttpGet("2c")]
        [SaModule("best", false)] // Override attribute of class
        public async Task<IActionResult> GetWithOverridenModule_WithoutDeclared_But_DeclareAtRuntime()
        {
            return await ResponseAsync(
                ClaimsBuilder.FromMetaData<WeatherForecastController>()
                    .WithPermission(Verb.View)
            );
        }

        [HttpGet("2d")]
        [SaModule("best")] // Override attribute of class
        public async Task<IActionResult> GetWithOverridenModule_CannotDoThis_Because_Module_Is_Restricted()
        {
            return await ResponseAsync(
                ClaimsBuilder.FromMetaData<WeatherForecastController>()
                    .WithPermission(Verb.View)
            );
            // The attribute SaModule has to be removed in order to process this method
        }

        [HttpGet("3")]
        [SaModule("best")] // Override attribute of class
        [SaPermission(Verb.View, "a")]
        [SaPermission(Verb.View, "b")]
        public IEnumerable<WeatherForecast> GetWithMissingPermission()
        {
            return GetSample();
        }

        [HttpGet("3x")]
        [SaModule("best")]
        [SaPermission(Verb.View, "a")]
        [SaPermission(Verb.Edit, "b")]
        public IEnumerable<WeatherForecast> ForTestingPermissionScanner()
        {
            return GetSample();
        }

        private IEnumerable<WeatherForecast> GetSample()
        {
            var rng = new Random();
            return Enumerable
                .Range(1, 5)
                .Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();
        }
    }
}