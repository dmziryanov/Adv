using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAL
{
    public class CategoryRepository : ICategoryRepository
    {
        private AdvContext _advContext;

        public CategoryRepository()
        {
            _advContext = new AdvContext();
        }

        public IEnumerable<Category> GetAll()
        {

            return null;// _advContext.Categories;
        
        }
    }
}
