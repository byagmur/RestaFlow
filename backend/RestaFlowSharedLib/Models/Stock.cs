using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RestaurantSharedLib.Models
{
    public class Stock
    {
        public int UID { get; set; }
        public string No { get; set; }
        public string Name { get; set; }
        public double Quantity { get; set; }
        public string QuantityType { get; set; }
        public string Spec1 { get; set; }
        public string Group1 { get; set; }
        public string Group2 { get; set; }
        public int InsertUser { get; set; }
        public DateTime InsertDate { get; set; }
        public string Barcode { get; set; }
        public string Qrcode { get; set; }
        public float Price { get; set; }


        public Stock() { }

        public Stock(int uid, string no, string name, double quantity, string quantityType, string spec1, string group1, string group2, int insertUser, DateTime insertDate, string barcode, string qrcode)
        {
            UID = uid;
            No = no;
            Name = name;
            Quantity = quantity;
            QuantityType = quantityType;
            Spec1 = spec1;
            Group1 = group1;
            Group2 = group2;
            InsertUser = insertUser;
            InsertDate = insertDate;
            Barcode = barcode;
            Qrcode = qrcode;
        }

        public class ProductItem //combo boxtaki her bir ürün için item
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Quantity { get; set; } // Ürün miktarı
            public override string ToString() => Name; // ComboBox'ta isim gözüksün

            
        }

    }

    public class ProductPriceInfo
    {
        public string No { get; set; }
        public string Name { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? PurchasePrice { get; set; }
    }


}



