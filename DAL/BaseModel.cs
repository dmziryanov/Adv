using System.Collections.Generic;

namespace DAL
{
    public class BaseModel
    {
        public IEnumerable<Location> _locations;
        public IEnumerable<Category> _subCategories;
        public IEnumerable<Category> _categories;
    }
}