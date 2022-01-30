using Microsoft.AspNetCore.Mvc;
using Northwnd.API.Interfaces;
using Northwnd.DAL.Models;

namespace Test.API.Controllers
{
    public class CategoryController : Controller
    {
        private ICategory _category;

        public CategoryController(ICategory category)
        {
            _category = category;
        }

        [HttpGet]
        [Route("api/GetCategories")]
        public async Task<ActionResult> GetCategoryes()
        {
            var result = await _category.GetCategories();
            return Ok(result);
        }

        [HttpGet]
        [Route("api/GetCategoryById/{categoryId}")]
        public async Task<ActionResult> GetCategoryById(int categoryId)
        {
            var result = await _category.GetCategory(categoryId);
            return Ok(result);
        }

        [HttpPost]
        [Route("api/AddCategory")]
        public async Task<ActionResult> AddCategory(CategoryRequestModel category)
        {
            var result = await _category.AddCategory(category);
            return Ok(result);
        }

        [HttpDelete]
        [Route("api/DeleteCategory")]
        public async Task DeleteCategory(int categoryId)
        {
            await _category.DeleteCategory(categoryId);
        }


        [HttpPut]
        [Route("api/UpdateCategory")]
        public async Task<ActionResult> UpdateCategory(int categoryId,CategoryRequestModel category)
        {
            var result = await _category.EditCategory(categoryId,category);
            return Ok(result);
        }

    }
}
