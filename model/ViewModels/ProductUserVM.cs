using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSD_3354_Project_Models.ViewModels
{
    public class ProductUserVM
    {
        public ProductUserVM()
        {
            ProductList = new List<Product>();
        }

        public ApplicationUser ApplicationUser { get; set; }
        //public IEnumerable<Product> ProductList { get; set; }
        public IList<Product> ProductList { get; set; }
    }
}
