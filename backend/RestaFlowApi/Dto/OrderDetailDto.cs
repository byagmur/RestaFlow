public class OrderDetailDto
{
    public int OrderId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime InsertDate { get; set; } = DateTime.Now;
}