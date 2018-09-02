using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleStoreService;
using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleStoreWeb.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : Controller
    {
        private static Regex ipRex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
        private readonly HttpClient httpClient;

        public CartsController(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        private async Task<string> ResolveAddress()
        {
            var partitionResolver = ServicePartitionResolver.GetDefault();
            var resolveResults = await partitionResolver.ResolveAsync(
                new Uri("fabric:/SimpleStoreApplication/SimpleStoreService"), new ServicePartitionKey(1), new System.Threading.CancellationToken());

            var endpoint = resolveResults.GetEndpoint();
            var endpointObject = JsonConvert.DeserializeObject<JObject>(endpoint.Address);
            var address = ((JObject)endpointObject.Property("Endpoints").Value)[""].Value<string>();

            return address;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            HttpResponseMessage response = await this.httpClient.GetAsync(await ResolveAddress() + "/api/ShoppingCarts");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return this.StatusCode((int)response.StatusCode);
            var cart = JsonConvert.DeserializeObject<ShoppingCart>(await response.Content.ReadAsStringAsync());
            return Json(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]ShoppingCartItem item)
        {
            StringContent postContent = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json");
            postContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await this.httpClient.PostAsync(await ResolveAddress() + "/api/ShoppingCarts", postContent);
            return new OkResult();
        }

        [HttpDelete("{productName}")]
        public async Task<IActionResult> Delete(string productName)
        {
            HttpResponseMessage response = await this.httpClient.DeleteAsync(await ResolveAddress() + "/api/ShoppingCarts/" + productName);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return this.StatusCode((int)response.StatusCode);
            return new OkResult();
        }

    }
}