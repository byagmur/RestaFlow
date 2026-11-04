using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSharedLib.Models
{
    public class StockMovement
    {
        public int Uid { get; set; }
        public int StockId { get; set; }
        public string No { get; set; }
        public int MovementType { get; set; }
        public DateTime MovementDate { get; set; }
        public decimal Quantity { get; set; }
        public string QuantityType { get; set; }
        public string Name { get; set; }

        public StockMovement()
        {
        }
        public StockMovement(int uid, int stockId,string No, int movementType, DateTime movementDate, string quantityType, string name)
        {
            this.Uid = uid;
            this.StockId = stockId;
            this.No = No;
            this.MovementType = movementType;
            this.MovementDate = movementDate;
            this.QuantityType = quantityType;
            this.Name = name;
        }
      


    }
}
