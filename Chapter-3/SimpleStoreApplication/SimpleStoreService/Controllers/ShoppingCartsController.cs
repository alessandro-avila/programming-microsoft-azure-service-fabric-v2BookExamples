using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System.Threading.Tasks;

namespace SimpleStoreService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartsController : Controller
    {
        private readonly IReliableStateManager mStateManager;
        private const string ANONYMOUS = "anonymous";
        private const string ShoppingCart = "shoppingCart";

        public ShoppingCartsController(IReliableStateManager stateManager)
        {
            this.mStateManager = stateManager;
        }

        private string GetUserIdentity()
        {
            if (HttpContext.User != null
                && HttpContext.User.Identity != null
                && !string.IsNullOrEmpty(HttpContext.User.Identity.Name))
            {
                return HttpContext.User.Identity.Name;
            }
            else
            {
                return ANONYMOUS;
            }
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var myDictionary = await mStateManager.GetOrAddAsync<IReliableDictionary<string, ShoppingCart>>(ShoppingCart);
            using (var tx = mStateManager.CreateTransaction())
            {
                var result = await myDictionary.TryGetValueAsync(tx, GetUserIdentity());
                if (result.HasValue)
                {
                    return Json(new
                    {
                        Total = result.Value.Total,
                        Items = result.Value.GetItems()
                    });
                }
                else
                {
                    return Json(new ShoppingCart());
                }
            }
        }

        [HttpPost]
        public async void Post([FromBody] ShoppingCartItem item)
        {
            await AddToCart(item);
        }

        private async Task AddToCart(ShoppingCartItem item)
        {
            var myDictionary = await mStateManager.GetOrAddAsync<IReliableDictionary<string, ShoppingCart>>(ShoppingCart);
            using (var tx = mStateManager.CreateTransaction())
            {
                await myDictionary.AddOrUpdateAsync(tx, GetUserIdentity(), new ShoppingCart(item), (k, v) => v.AddItem(item));
                await tx.CommitAsync();
            }
        }

        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            var myDictionary = await mStateManager.GetOrAddAsync<IReliableDictionary<string, ShoppingCart>>(ShoppingCart);
            using (var tx = mStateManager.CreateTransaction())
            {
                var result = await myDictionary.TryGetValueAsync(tx, GetUserIdentity());
                if (result.HasValue)
                {
                    await myDictionary.SetAsync(tx, name, result.Value.RemoveItem(name));
                    await tx.CommitAsync();
                    return new OkResult();
                }
                else
                {
                    return new NotFoundResult();
                }
            }
        }

    }
}
