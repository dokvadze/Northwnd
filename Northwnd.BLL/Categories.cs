using Northwnd.API.Interfaces;
using Northwnd.DAL.Models;
using Test.DAL;

namespace Northwnd.BLL
{
    public class Categories : ICategory
    {

        private NorthwndDbContext _northwndDbContext;

        public Categories(NorthwndDbContext northwndDbContext)
        {
            _northwndDbContext = northwndDbContext;
        }

        public async Task<List<Category>> GetCategories() => _northwndDbContext.Categories.ToList();


        public async Task<Category> GetCategory(int categoryId)
        {
            var result = await GetCategories();
            return result.Where(x => x.CategoryID == categoryId).FirstOrDefault();

        }


        public async Task<Category> AddCategory(CategoryRequestModel category)
        {

            var newCategoryIdentifier = Guid.NewGuid();

            var Category = new Category
            {
                CategoryName = category.CategoryName,
                Description = category.Description,
                Picture = category.Picture,
                UniqueId = newCategoryIdentifier
            };

            _northwndDbContext.Categories.Add(Category);
            await _northwndDbContext.SaveChangesAsync();

            return _northwndDbContext.Categories.Where(x => x.UniqueId == newCategoryIdentifier).FirstOrDefault();

        }

        public async Task DeleteCategory(int categoryId)
        {
            var category = GetCategory(categoryId);
            if (category != null) _northwndDbContext.Categories.Remove(category.Result);
            await _northwndDbContext.SaveChangesAsync();
        }

        public async Task<Category> EditCategory(int categoryId, CategoryRequestModel category)
        {
            var Category = await GetCategory(categoryId);

            Category.CategoryName = category.CategoryName;
            Category.Description = category.Description;
            Category.Picture = category.Picture;
            Category.UniqueId = Category.UniqueId;

            _northwndDbContext.Categories.Update(Category);
            await _northwndDbContext.SaveChangesAsync();

            return Category;
        }


    }
}