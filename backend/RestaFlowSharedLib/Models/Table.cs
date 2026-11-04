using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace RestaurantSharedLib.Models
{
    public class Table
    {
        public int UID { get; set; }
        public string TableName { get; set; }

        public int Capacity { get; set; }
        public string Status { get; set; }


        public Table()
        {
        }

        public Table(int uid, string name, int capacity, string status)
        {
            this.UID = uid;
            this.TableName = name;
            this.Capacity = capacity;
            this.Status = status;
        }


    }




}



