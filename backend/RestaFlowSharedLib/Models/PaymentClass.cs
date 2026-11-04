using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSharedLib.Models
{
    internal class PaymentClass
    {
        int uid;
        int orderId;
        decimal amount;
        string paymentMethod;
        string paymentStatus;
        DateTime createdAt;

        public PaymentClass() { }

        public PaymentClass(int uid, int orderId, decimal amount, string paymentMethod, string paymentStatus, DateTime createdAt)
        {
            this.uid = uid;
            this.orderId = orderId;
            this.amount = amount;
            this.paymentMethod = paymentMethod;
            this.paymentStatus = paymentStatus;
            this.createdAt = createdAt;
        }

        public int UID { get; set; }
        public int OrderID { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionID { get; set; }
        public DateTime CreatedAt { get; set; }

        
    }
}
