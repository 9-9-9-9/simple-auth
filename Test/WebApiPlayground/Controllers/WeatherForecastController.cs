using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SimpleAuth.Client.AspNetCore.Attributes;
using SimpleAuth.Shared.Enums;

namespace WebApiPlayground.Controllers
{
    [ApiController]
    [Route("weatherforecast")]
    [SaModule("weatherforecast", false)]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        
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
        
        [HttpGet("2")]
        [SaModule("best")] // Override attribute of class
        [SaPermission(Permission.View)]
        public IEnumerable<WeatherForecast> GetWithOverridenModule()
        {
            return GetSample();
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