using RazorEcom.Data;
using RazorEcom.Models;

namespace RazorEcom.Services
{
    public class CategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<int> GetAllChildCategoryIds(int parentId)
        {
            var allCategories = _context.Categories.ToList();
            var parentCategory = allCategories.FirstOrDefault(c => c.Id == parentId);

            if (parentCategory == null)
                return new List<int>();

            return GetAllChildCategoryIdsRecursive(parentCategory, allCategories);
        }

        private List<int> GetAllChildCategoryIdsRecursive(Category parent, List<Category> all)
        {
            var ids = new List<int> { parent.Id };

            var children = all.Where(c => c.ParentId == parent.Id).ToList();
            foreach (var child in children)
            {
                ids.AddRange(GetAllChildCategoryIdsRecursive(child, all));
            }

            return ids;
        }
    }
}
