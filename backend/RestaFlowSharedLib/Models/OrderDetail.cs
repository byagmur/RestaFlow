using System;

namespace RestaurantSharedLib.Models
{
    public class OrderDetail
    {
        int uid;
        int orderID;
        string productID;
        int quantity;
        DateTime orderDate;
        string productName;


        public OrderDetail()
        {
        }

        public OrderDetail(int uid, int orderID, string productID, int quantity, DateTime orderDate,string productName)
        {
            this.uid = uid;
            this.orderID = orderID;
            this.productID = productID;
            this.quantity = quantity;
            this.orderDate = orderDate;
            this.productName = productName;

        }

        public int Uid { get { return uid; } set { uid = value; } }
        public int OrderId { get { return orderID; } set { orderID = value; } }
        public string ProductId { get { return productID; } set { productID = value; } }
        public int Quantity { get { return quantity; } set { quantity = value; } }
        public DateTime OrderDate { get { return orderDate; } set { orderDate = value; } }
        public string ProductName { get { return productName; } set { productName = value; } }


    }
}
