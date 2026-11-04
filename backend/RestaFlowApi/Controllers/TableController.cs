using Dapper;
using Microsoft.AspNetCore.Mvc;
using RestaFlowApi.Dto;
using RestaFlowApi.Repos;
using RestaurantSharedLib;
using RestaurantSharedLib.Models;
using RestaurantSharedLib.Repos;

namespace RestaFlowApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private readonly TableRepository _tableRepo = new TableRepository();

        [HttpGet]
        public IActionResult GetAllTableStatus()
        {
            var tables = _tableRepo.GetAllTableStatus()
                .Select(t => new
                {
                    t.UID,
                    t.TableName,
                    t.Status
                })
                .ToList();

            return Ok(tables);
        }



        [HttpPost("move-order")]
        public IActionResult MoveOrderToTable([FromBody] MoveOrderDto dto)
        {
            
            if (dto == null || dto.OrderId <= 0 || dto.TargetTableId <= 0)
                return BadRequest("Geçersiz parametre.");
            Console.WriteLine("move-order : POST\r\n", Request.HttpContext,"\r\n");
            var orderRepo = new OrderRepository();
            var tableRepo = new TableRepository();

            var targetTableStatus = orderRepo.GetTableStatus(dto.TargetTableId);
            if (!string.IsNullOrEmpty(targetTableStatus) && targetTableStatus != "İptal Edildi" && targetTableStatus != "Ödendi")
                return BadRequest("Hedef masa dolu!");

            var order = orderRepo.GetOrderById(dto.OrderId);
            if (order == null)
                return NotFound("Sipariş bulunamadı.");

            // Sadece TableID güncelleniyor, Status alanına dokunulmuyor
            if (tableRepo.MoveOrderToTable(dto.OrderId, dto.TargetTableId, dto.EmployeeId, out string error))
                return Ok(new { message = "Sipariş başarıyla taşındı." });
            else
                return StatusCode(500, "Taşıma işlemi sırasında hata oluştu: " + error);
        }

        [HttpGet("empty")]
        public IActionResult GetEmptyTables()
        {
            var emptyTables = _tableRepo.GetEmptyTables()
                .Select(t => new
                {
                    t.UID,
                    t.TableName,
                    t.Status
                })
                .ToList();

            return Ok(emptyTables);
        }
    }
}