using CalculatorService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;

namespace CalculatorWeb.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public ActionResult<int> Add(int a, int b)
        {
            var calculatorClient = ServiceProxy.Create<ICalculatorService>(
                new Uri("fabric:/CalculatorApplication/CalculatorService"));
            return calculatorClient.Add(a, b).Result;
        }

        [HttpGet]
        public ActionResult<int> Subtract(int a, int b)
        {
            var calculatorClient = ServiceProxy.Create<ICalculatorService>(
                new Uri("fabric:/CalculatorApplication/CalculatorService"));
            return calculatorClient.Subtract(a, b).Result;
        }
    }
}
