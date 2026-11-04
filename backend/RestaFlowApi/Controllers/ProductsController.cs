using Microsoft.AspNetCore.Mvc;
using RestaurantSharedLib;
using RestaurantSharedLib.Models;
using RestaurantSharedLib.Repos;

namespace RestaFlowApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly StockRepository _stockRepo = new StockRepository(DBContext.connectionString);

        [HttpGet]
        public IActionResult GetProducts([FromQuery] int limit = 50)
        {
            // Ürünleri ve fiyatlarını birleştir
            var products = _stockRepo.GetAllStocks().Take(limit).ToList();
            var prices = _stockRepo.GetProductPrices().Take(limit).ToList();

            var result = products.Select(p =>
            {
                var priceInfo = prices.FirstOrDefault(x => x.No == p.No);
                return new
                {
                    p.UID,
                    p.No,
                    p.Name,
                    p.Quantity,
                    p.QuantityType,
                    p.Spec1,
                    p.Group1,
                    p.Group2,
                    p.StokQStatus,
                    SalePrice = priceInfo?.SalePrice ?? 0
                };
            }).Take(limit).ToList();

            return Ok(result);
        }
    }
}