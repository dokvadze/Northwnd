using Northwnd.DAL.Models;

namespace Northwnd.API.Interfaces
{
    public interface ICategory
    {
        Task<List<Category>> GetCategories();
        Task<Category> GetCategory(int categoryId);
        Task<Category> AddCategory(CategoryRequestModel category);
        Task<Category> EditCategory(int categoryId, CategoryRequestModel category);
        Task DeleteCategory(int categoryId);
    }
}
