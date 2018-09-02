using System.Collections.Generic;
using System.Linq;

namespace SimpleStoreService
{
    public class ShoppingCart
    {
        private List<ShoppingCartItem> mItems;
        public double Total
        {
            get
            {
                return mItems.Sum(i => i.LineTotal);
            }
        }

        public ShoppingCart()
        {
            mItems = new List<ShoppingCartItem>();
        }

        public ShoppingCart(ShoppingCartItem item) : this()
        {
            this.AddItem(item);
        }

        public ShoppingCart AddItem(ShoppingCartItem newItem)
        {
            var existingItem = mItems.FirstOrDefault(
                i => i.ProductName == newItem.ProductName);
            if (existingItem != null)
                existingItem.Quantity += newItem.Quantity;
            else
                mItems.Add(newItem);

            return this;
        }

        public ShoppingCart RemoveItem(string productName)
        {
            var existingItem = mItems.FirstOrDefault(
                            i => i.ProductName == productName);
            if (existingItem != null)
                mItems.Remove(existingItem);

            return this;
        }

        public IEnumerable<ShoppingCartItem> GetItems()
        {
            return mItems.AsEnumerable();
        }
    }
}
