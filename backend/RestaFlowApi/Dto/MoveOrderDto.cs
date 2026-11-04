namespace RestaFlowApi.Dto
{
    public class MoveOrderDto
    {
        public int OrderId { get; set; }
        public int TargetTableId { get; set; }
        public int EmployeeId { get; set; } //işlemi gerçekleştiren personel
    }
}
