using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSharedLib.Models
{
    internal class StockPrice
    {
        int uid;
        int stockId;
        decimal price;
        DateTime updateDate;
        double currentPrice;

        public StockPrice()
        {
        }
        public StockPrice(int uid, int stockId, decimal price, DateTime updateDate, double currentPrice)
        {
            this.uid = uid;
            this.stockId = stockId;
            this.price = price;
            this.updateDate = updateDate;
            this.currentPrice = currentPrice;
        }
        public int UID { get { return uid; } set { uid = value; } }
        public int StockId { get { return stockId; } set { stockId = value; } }
        public decimal Price { get { return price; } set { price = value; } }
        public DateTime UpdateDate { get { return updateDate; } set { updateDate = value; } }
        public double CurrentPrice { get{ return currentPrice; } set { currentPrice = value; }
        }

    }
}
