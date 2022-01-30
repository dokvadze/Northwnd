using Microsoft.AspNetCore.Mvc;
using Northwnd.API.Interfaces;
using Northwnd.DAL.Models;

namespace Test.API.Controllers
{
    public class RegionController : Controller
    {
        private IRegion _region;

        public RegionController(IRegion region)
        {
            _region = region;
        }

        //[HttpGet]
        //[Route("api/GetProducts")]
        //public async Task<ActionResult> GetProducts()
        //{
        //    var result = await _product.GetProducts();
        //    return Ok(result);
        //}

        //[HttpGet]
        //[Route("api/GetProductById/{productId}")]
        //public async Task<ActionResult> GetProductById(int productId)
        //{
        //    var result = await _product.GetProduct(productId);
        //    return Ok(result);
        //}

        //[HttpPost]
        //[Route("api/AddProduct")]
        //public async Task<ActionResult> AddProduct(ProductRequestModel product)
        //{
        //    var result = await _product.AddProduct(product);
        //    return Ok(result);
        //}


        //[HttpPut]
        //[Route("api/UpdateProduct")]
        //public async Task<ActionResult> UpdateProduct(int productId,ProductRequestModel product)
        //{
        //    var result = await _product.EditProduct(productId,product);
        //    return Ok(result);
        //}

        //[HttpDelete]
        //[Route("api/DeleteProduct")]
        //public async Task DeleteProduct(int productId)
        //{
        //    await _product.DeleteProduct(productId);
        //}

    }
}
