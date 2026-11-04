using System;
using System.Collections.Generic;

namespace RestaFlowApi.Dto;
public class OrderDto
{

    public int? UID { get; set; }
    public required string Status { get; set; }
    public int TableID { get; set; }
    public int WaiterID { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Note { get; set; }

    public List<ItemDto>? Items { get; set; }

}



