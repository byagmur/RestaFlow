using System;

namespace RestaurantSharedLib.Models
{
    public class OrderMovementClass
    {
        int uid;
        int orderId;
        string orderStatus;
        DateTime movementDate;
        int employeeId;
        string waiterName;
        int? targetTableId;

        public OrderMovementClass()
        {
        }
        public OrderMovementClass(int uid, int orderId, int productId, int quantity, DateTime movementDate, 
                                  string orderStatus,int employeeId, int? targetTableId=null)
        {
            this.uid = uid;
            this.orderId = orderId;
            this.orderStatus = orderStatus;
            this.movementDate = movementDate;
            this.employeeId = employeeId;
            this.targetTableId = targetTableId;
        }
        public int Uid { get { return uid; } set { uid = value; } }
        public int OrderId { get { return orderId; } set { orderId = value; } }
        public DateTime MovementDate { get { return movementDate; } set { movementDate = value; } }

        public string OrderStatus { get { return orderStatus; } set { orderStatus = value; } }

        public int EmployeeId { get { return employeeId; } set { employeeId = value; } }

        public string WaiterName { get { return waiterName;  } set { waiterName = value; } }

        public int? TargetTableId { get { return targetTableId; } set { targetTableId = value; } }



    }

}
