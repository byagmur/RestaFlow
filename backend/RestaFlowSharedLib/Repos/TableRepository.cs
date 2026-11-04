using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using RestaurantSharedLib.Models;
using System.Windows.Forms;
using System.Linq.Expressions;

namespace RestaurantSharedLib.Repos
{
    public class TableRepository
    {
        // TableRepository.cs
        public List<Table> GetEmptyTables()
        {
            try
            {
                using (var connection = new SqlConnection(DBContext.connectionString))
                {
                    //string query = "SELECT UID, TableName, Status FROM [Table] WHERE Status <> 'Dolu' and Status <> 'Rezerve'";
                    string query = "SELECT UID, TableName, Status FROM [Table] WHERE Status = ''";
                    var tables = connection.Query<Table>(query).ToList();

                    //if (tables == null || !tables.Any())
                    //{
                    //    MessageBox.Show("Tüm masalar dolu.");
                    //}

                    return tables;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Boş masalar yüklenirken hata oluştu: " + ex.Message);
                Logger.LogError("Masalar yüklenirken hata oluştu.", ex, Account.LoggedUser.Username, null);
                return new List<Table>();
            }
        }

        public static int LoadActiveTablesCount()
        {
            Logger.LogInfo("Aktif masa sayısı sorgulanıyor.", null, null);
            try
            {
                using (var dbContext = new DBContext())
                {
                    string query = @"SELECT COUNT(DISTINCT UID) FROM [Table] WHERE Status = 'Dolu'";
                    int count = dbContext._connection.QuerySingle<int>(query);
                    Logger.LogInfo($"Aktif masa sayısı: {count}", null, null);
                    return count;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Aktif masalar yüklenirken hata oluştu.", ex, null, null);
                MessageBox.Show("Aktif masalar yüklenirken hata oluştu: " + ex.Message);
                return 0;
            }
        }

        public IEnumerable<(int UID, string Status, string TableName)> GetAllTableStatus()
        {
            Logger.LogInfo("Tüm masa durumları sorgulanıyor.", Account.LoggedUser?.Username, null);
            try
            {
                using (var connection = new SqlConnection(DBContext.connectionString))
                {
                    string query = "SELECT UID, Status, TableName FROM [Table]";
                    var result = connection.Query<(int UID, string Status, string TableName)>(query);
                    Logger.LogInfo("Tüm masa durumları başarıyla alındı.", Account.LoggedUser?.Username, null);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Tüm masa durumları alınırken hata oluştu.", ex, Account.LoggedUser?.Username, null);
                throw; 
            }
        }

        // masa taşıma işlemi
        public bool MoveOrderToTable(int orderId, int targetTableId, int employeeId, out string errorMessage)
        {
            errorMessage = null;
            using (var connection = new SqlConnection(DBContext.connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Sadece siparişin TableID'sini güncelle
                        string updateOrderSql = "UPDATE [Order] SET TableID = @TargetTableId WHERE UID = @OrderId";
                        connection.Execute(updateOrderSql, new { TargetTableId = targetTableId, OrderId = orderId }, transaction);

                        // Sipariş hareketi ekle
                        var movement = new OrderMovementClass
                        {
                            OrderId = orderId,
                            OrderStatus = "Masa Taşındı",
                            EmployeeId = employeeId,
                            MovementDate = DateTime.Now,
                            TargetTableId = targetTableId
                        };
                        string insertMovementSql = @"INSERT INTO OrderMovement (OrderID, MovementDate, OrderStatus, EmployeeId, TargetTable)
                                             VALUES (@OrderId, @MovementDate, @OrderStatus, @EmployeeId, @TargetTableId)";
                        connection.Execute(insertMovementSql, movement, transaction);

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        errorMessage = ex.Message;
                        return false;
                    }
                }
            }
        }



    }
}
