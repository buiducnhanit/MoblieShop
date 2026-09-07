using MoblieShop.Models;

namespace MoblieShop.ViewModels
{
    public class WishListViewModel
    {
        public IEnumerable<Product> Products { get; set; }

        public IEnumerable<WishListItem> WishListItems { get; set; }
    }
}
