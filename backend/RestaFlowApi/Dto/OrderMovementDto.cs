namespace RestaFlowApi.Dto
{
    public class OrderMovementDto
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; }
        public int EmployeeId { get; set; }
        public DateTime MovementDate { get; set; } = DateTime.Now;
        public int? TargetTableId {get; set;}
        //masa no eklenecek
    }
}