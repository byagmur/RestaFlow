using Microsoft.AspNetCore.Mvc;
using RestaFlowApi.Dto;
using RestaFlowApi.Repos;
using RestaurantSharedLib;
using RestaurantSharedLib.Models;
using RestaurantSharedLib.Repos;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderRepository _orderRepo;

    public OrdersController(OrderRepository orderRepo)
    {
        _orderRepo = orderRepo;
    }

    [HttpGet]
    public IActionResult GetOrders([FromQuery] int? waiterID, [FromQuery] int start = 0, [FromQuery] int limit = 50)
    {
        var orders = _orderRepo.GetPagedOrders(start, limit, waiterID);
        int totalCount = waiterID.HasValue
            ? _orderRepo.GetOrdersByWaiterId(waiterID.Value).Count
            : _orderRepo.GetAllOrders().Count;

        var pagedDtos = orders.Select(order => new OrderDto
        {
            UID = order.UID,
            Status = order.Status,
            TableID = order.TableId,
            WaiterID = order.WaiterId,
            OrderDate = order.OrderDate,
            TotalPrice = (decimal)order.TotalPrice,
            Note = order.Note
        }).ToList();

        var result = new PagedOrderResult<OrderDto>
        {
            TotalCount = totalCount,
            Data = pagedDtos
        };

        return Ok(result);
    }


    [HttpGet("{id}")]
    public IActionResult GetOrder(int id)
    {
        var order = _orderRepo.GetOrderById(id);
        if (order == null)
            return NotFound();

        // Sipariş detaylarını çek
        var details = OrderRepository.LoadOrderDetails(order.UID);

        // Stok fiyatlarını çek (örnek: StockRepository ile)
        var stockRepo = new StockRepository(DBContext.connectionString);
        var prices = stockRepo.GetProductPrices().ToList();

        // Her detay için ItemDto oluştur, fiyatı stoktan bul
        var items = details.Select(d =>
        {
            var priceInfo = prices.FirstOrDefault(p => p.No == d.ProductId);
            return new ItemDto
            {
                Id = d.ProductId,
                Name = d.ProductName,
                Quantity = d.Quantity,
                Price = priceInfo?.SalePrice ?? 0
            };
        }).ToList();

        var dto = new OrderDto
        {
            UID = order.UID,
            Status = order.Status,
            TableID = order.TableId,
            WaiterID = order.WaiterId,
            OrderDate = order.OrderDate,
            TotalPrice = (decimal)order.TotalPrice,
            Note = order.Note,
            Items = items
        };

        return Ok(dto);
    }

    [HttpPut("status")]
    public IActionResult UpdateOrderStatus([FromBody] UpdateOrderStatusDto dto)
    {
        if (dto == null || dto.OrderId <= 0 || string.IsNullOrWhiteSpace(dto.Status))
            return BadRequest("Geçersiz parametre.");

        try
        {
            OrderRepository.UpdateOrderStatus(dto.OrderId, dto.Status);
        }
        catch (Exception ex)
        {
            Logger.Log(Logger.LogLevel.Error, "ORDER:exUpdateStatus: ", ex);
            return NotFound();
        }
        return Ok(new { orderId = dto.OrderId, status = dto.Status });
    }

    [HttpPost]
    public IActionResult CreateOrder([FromBody] OrderDto orderDto)
    {
        if (orderDto == null)
            return BadRequest("Sipariş verisi zorunludur.");

        // Sipariş ürünleri kontrolü (Items property’si DTO’da yoksa bu kontrolü kaldırabilirsiniz)
        // if (orderDto.Items == null || !orderDto.Items.Any())
        //     return BadRequest("Sipariş ve ürünler zorunludur.");

        // DTO'dan entity'ye dönüşüm
        var order = new Order
        {
            TableId = orderDto.TableID,
            WaiterId = orderDto.WaiterID,
            OrderDate = orderDto.OrderDate,
            TotalPrice = (double)orderDto.TotalPrice,
            Status = orderDto.Status,
            Note = orderDto.Note,
           
        };

        var vOrderId = _orderRepo.InsertOrder(order);
        // return CreatedAtAction(nameof(GetOrder), new { uid = orderId }, orderDto);
        return Ok(new { orderId = vOrderId });
    }

    [HttpPost("detail")]
    public IActionResult AddOrderDetail([FromBody] OrderDetailDto detail)
    {
        Logger.Log(Logger.LogLevel.Info, "DETAIL:productId=" + detail.ProductId);   
        if (detail == null || string.IsNullOrWhiteSpace(detail.ProductId) || detail.Quantity <= 0)
            return BadRequest("Geçersiz sipariş detayı.");

        Logger.Log(Logger.LogLevel.Info, "DETAIL:Before Insert");
        try
        {
            OrderRepository.InsertOrderDetail(detail.OrderId, detail.ProductId, detail.Quantity, detail.InsertDate);
        }
        catch (Exception exAdd001)
        {
            Logger.Log(Logger.LogLevel.Error, "DETAIL:exAdd001: ", exAdd001);
            return NotFound();
        }
        return Ok(new { orderId = detail.OrderId });
        Logger.Log(Logger.LogLevel.Info, "DETAIL:Successed");
    }

    [HttpPost("movement")]
    public IActionResult AddOrderMovement([FromBody] OrderMovementDto movement)
    {
        if (movement == null || movement.OrderId <= 0 || string.IsNullOrWhiteSpace(movement.OrderStatus))
            return BadRequest("Geçersiz sipariş hareketi.");

        var movementEntity = new OrderMovementClass
        {
            OrderId = movement.OrderId,
            OrderStatus = movement.OrderStatus,
            EmployeeId = movement.EmployeeId,
            MovementDate = movement.MovementDate,
            TargetTableId = movement.TargetTableId,
            
        };

        OrderRepository.InsertOrderMovement(movementEntity);
        return Ok(new { message = "Sipariş hareketi başarıyla eklendi." });
    }


    [HttpGet("last-orders-per-table")]
    public IActionResult GetLastOrdersForAllTables([FromQuery] int? waiterID, [FromQuery] int start = 0, [FromQuery] int limit = 50)
    {
        // Toplam masa sayısı (waiterID'ye göre filtrelenmiş)
        int totalCount = waiterID.HasValue
            ? _orderRepo.GetAllOrders().Where(o => o.WaiterId == waiterID.Value).GroupBy(o => o.TableId).Count()
            : _orderRepo.GetAllOrders().GroupBy(o => o.TableId).Count();

        var orders = _orderRepo.GetLastOrdersForAllTablesPagedByWaiter(start, limit, waiterID);

        var dtos = orders.Select(order => new OrderDto
        {
            UID = order.UID,
            Status = order.Status,
            TableID = order.TableId,
            WaiterID = order.WaiterId,
            OrderDate = order.OrderDate,
            TotalPrice = (decimal)order.TotalPrice,
            Note = order.Note
        }).ToList();

        var result = new PagedOrderResult<OrderDto>
        {
            TotalCount = totalCount,
            Data = dtos
        };

        return Ok(result);
    }



    // Sipariş detayından ürün sil
    [HttpDelete("detail")]
    public IActionResult DeleteOrderDetail([FromQuery] int orderId, [FromQuery] string productId)
    {
        if (orderId <= 0 || string.IsNullOrWhiteSpace(productId))
            return BadRequest("Geçersiz parametre.");

        try
        {
            _orderRepo.DeleteOrderDetail(orderId, productId);
        }
        catch (Exception ex)
        {
            Logger.Log(Logger.LogLevel.Error, "DETAIL:exDelete001: ", ex);
            return NotFound();
        }
        return Ok(new { orderId });
    }

    // Sipariş detayındaki ürün miktarını güncelle
    [HttpPut("detail")]
    public IActionResult UpdateOrderDetail([FromBody] OrderDetailDto detail)
    {
        if (detail == null || detail.OrderId <= 0 || string.IsNullOrWhiteSpace(detail.ProductId) || detail.Quantity <= 0)
            return BadRequest("Geçersiz sipariş detayı.");

        try
        {
            _orderRepo.UpdateOrderDetailQuantity(detail.OrderId, detail.ProductId, detail.Quantity);
        }
        catch (Exception ex)
        {
            Logger.Log(Logger.LogLevel.Error, "DETAIL:exUpdate001: ", ex);
            return NotFound();
        }
        return Ok(new { orderId = detail.OrderId, productId = detail.ProductId, quantity = detail.Quantity });
    }


}
