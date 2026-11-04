using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSharedLib.Models
{
    public class Reservation
    {
           public int UID { get; set; } // Tabloya göre UID
            public int TableId { get; set; }
            public string CustomerName { get; set; }
            public string CustomerPhone { get; set; }
            public DateTime? Date { get; set; }
            public string Note { get; set; }
            public string Status { get; set; }
        
    }
}
