using MoblieShop.Models;

namespace MoblieShop.ViewModels
{
    public class MyModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}
