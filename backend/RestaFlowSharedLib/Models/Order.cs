using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace RestaurantSharedLib.Models
{
    public class Order
    {
        //private int id;
       private int uid;
        private string status;
        private int tableId;
        private int waiterId;
        private DateTime orderDate;
        private double totalPrice;
        private string waiterName;
        public List<string> Items { get; set; }

        private string note;

        public int UID
        {
            get { return uid; }
            set { uid = value; }
        }
        public string Status
        {
            get { return status; }
            set { status = value; }
        }

        public int TableId
        {
            get { return tableId; }
            set { tableId = value; }
        }

        public int WaiterId
        {
            get { return waiterId; }
            set { waiterId = value; }
        }

        public DateTime OrderDate
        {
            get { return orderDate; }
            set { orderDate = value; }
        }

        public double TotalPrice
        {
            get { return totalPrice; }
            set { totalPrice = value; }
        }

        public string Note
        {
            get { return note; }
            set { note = value; }
        }

        public string WaiterName
        {
            get { return waiterName; }
            set { waiterName = value; }
        }

        public Order()
        {
        }

        public Order(int uid, string status, int tableId, int waiterId, DateTime orderDate, double totalPrice)
        {
            this.uid = uid;
            this.Status = status;
            this.TableId = tableId;
            this.WaiterId = waiterId;
            this.OrderDate = orderDate;
            this.TotalPrice = totalPrice;
            this.Items = new List<string>();
            this.Note = note;
        }

        public static List<Order> OrderList = new List<Order>();
    }

    public class OrderDateOnly
    {
        public DateTime OrderDate { get; set; }
    }
}
