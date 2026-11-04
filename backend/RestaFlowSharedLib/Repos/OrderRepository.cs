using RestaurantSharedLib;
using System.Windows.Forms;
using System;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using RestaurantSharedLib.Models;
using System.Data.SqlClient;

namespace RestaFlowApi.Repos
{

    public class OrderRepository
    {
        public static int LoadDailyOrderCount()
        {
            try
            {
                using (var dbContext = new DBContext())
                {
                    string query = @"SELECT COUNT(*) FROM [Order] WHERE CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)";
                    int orderCount = dbContext._connection.QuerySingle<int>(query);
                    return orderCount;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Günlük siparişler yüklenirken hata oluştu: " + ex.Message);
                Logger.LogError("Günlük siparişler yüklenirken hata oluştu.", ex, Account.LoggedUser?.Username, null);
                return 0;
            }
        }

        // günlük ciroyu yüklemek için
        public static int LoadDailyRevenue()
        {
            try
            {
                using (var dbContext = new DBContext())
                {
                    string query = @"SELECT ISNULL(SUM(TotalPrice), 0) FROM [Order] WHERE CAST(OrderDate AS DATETIME) <= '2025-06-19 15:00' AND Status = 'Ödendi'";
                    decimal revenue = dbContext._connection.QuerySingle<decimal>(query);
                    return (int)revenue;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Günlük ciro yüklenirken hata oluştu: " + ex.Message);
                Logger.LogError("Günlük ciro yüklenirken hata oluştu.", ex, Account.LoggedUser?.Username, null);

                return 0;
            }
        }

        // tüm siparişleri yüklemek için
        public static List<Order> LoadAllOrders()
        {
            try
            {
                using (var dbContext = new DBContext())
                {
                    string query = @"SELECT * FROM [Order]";
                    var orders = dbContext._connection.Query<Order>(query).ToList();
                    return orders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Siparişler yüklenirken hata oluştu: " + ex.Message);
                Logger.LogError("Siparişler yüklenirken hata oluştu.", ex, Account.LoggedUser?.Username, null);

                return new List<Order>();
            }
        }

        // sipariş detaylarını yüklemek için
        public static List<OrderDetail> LoadOrderDetails(int orderId)
        {
            try
            {
                using (var dbContext = new DBContext())
                {
                    string query = @"
                SELECT od.*, s.Name AS ProductName
                FROM OrderDetail od
                LEFT JOIN Stock s ON od.ProductId = s.No
                WHERE od.OrderID = @OrderId";
                    var orderDetails = dbContext._connection.Query<OrderDetail>(query, new { OrderId = orderId }).ToList();
                    return orderDetails;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Sipariş detayları yüklenirken hata oluştu. OrderID: {orderId}", ex, Account.LoggedUser?.Username, null);
                return new List<OrderDetail>();
            }
        }


        public Order GetAllOrderByTableId(int tableId)
        {
            try
            {
                using (var connection = new SqlConnection(DBContext.connectionString))
                {
                    string query = @"SELECT TOP 1 * FROM [Order] WHERE TableID = @TableID ORDER BY OrderDate DESC";
                    return connection.QuerySingleOrDefault<Order>(query, new { TableID = tableId });

                }

            }
            catch (Exception ex)
            {
                Logger.LogError($"Masa ID {tableId} için siparişler sorgulanırken hata oluştu.", ex, Account.LoggedUser?.Username, null);
                return null;
            }
        }



        // siparişleri tablo ID'sine göre boş masaları almak için
        public Order GetOrderByTableId(int tableId)
        {
            try
            {
                using (var connection = new SqlConnection(DBContext.connectionString))
                {
                    string query = @"SELECT TOP 1 * FROM [Order] WHERE TableID = @TableID AND Status <> 'Ödendi' AND Status <> 'İptal Edildi' ORDER BY OrderDate DESC";
                    return connection.QuerySingleOrDefault<Order>(query, new { TableID = tableId });

                }

            }
            catch (Exception ex)
            {
                Logger.LogError($"Masa ID {tableId} için siparişler sorgulanırken hata oluştu.", ex, Account.LoggedUser?.Username, null);
                return null;
            }
        }

        // sipariş durumunu sipariş hareket tablosunda güncellemek için

        public static void InsertOrderMovement(OrderMovementClass movement)
        {
            Logger.LogInfo($"OrderMovement ekleniyor. OrderID: {movement.OrderId}, Status: {movement.OrderStatus}", Account.LoggedUser?.Username, null);
            try
            {
                using (var connection = new SqlConnection(DBContext.connectionString))
                {
                    string sql = @"INSERT INTO OrderMovement (OrderID, MovementDate, OrderStatus, EmployeeId)
                               VALUES (@OrderId, @OrderDate, @OrderStatus, @EmployeeId)";
                    connection.Execute(sql, new
                    {
                        movement.OrderId,
                        OrderDate = movement.MovementDate,
                        OrderStatus = movement.OrderStatus,
                        EmployeeId = movement.EmployeeId,
                        TargetTableId = movement.TargetTableId,
                    });
                    Logger.LogInfo($"OrderMovement başarıyla eklendi. OrderID: {movement.OrderId}", Account.LoggedUser?.Username, null);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"OrderMovement eklenirken hata oluştu. OrderID: {movement.OrderId}", ex, Account.LoggedUser?.Username, null);
                //MessageBox.Show("Sipariş hareketi eklenirken hata oluştu: " + ex.Message);
            }
        }



        // sipariş eklemek için
        public int InsertOrder(Order order)
        {
            Logger.LogInfo("_master_Sipariş ekleniyor.", Account.LoggedUser?.Username, null);
            try
            {
                using (var connection = new SqlConnection(DBContext.connectionString))
                {
                    string sql = @"
                INSERT INTO [Order] (Status, TableID, WaiterID, OrderDate, TotalPrice)
                OUTPUT INSERTED.UID
                VALUES (@Status, @TableID, @WaiterID, @OrderDate, @TotalPrice)";
                    int orderId = connection.QuerySingle<int>(sql, new
                    {
                        order.Status,
                        order.TableId,
                        order.WaiterId,
                        order.OrderDate,
                        order.TotalPrice
                    });
                    Logger.LogInfo($"_master_Sipariş başarıyla eklendi. OrderID: {orderId}", Account.LoggedUser?.Username, null);
                    return orderId;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("_master_Sipariş eklenirken hata oluştu.", ex, Account.LoggedUser?.Username, null);
                //MessageBox.Show("Sipariş eklenirken hata oluştu: " + ex.Message);
                return 0;
            }
        }

        // sipariş detaylarını eklemek için
        public static void InsertOrderDetail(int orderId, string productId, int quantity, DateTime insertDate)
        {
            Logger.LogInfo($"Sipariş detayı ekleniyor. OrderID: {orderId}, ProductID: {productId}", Account.LoggedUser?.Username, null);
            try
            {
                using (var connection = new SqlConnection(DBContext.connectionString))
                {
                    string sql = @"
                INSERT INTO [OrderDetail] (OrderID, ProductId, Quantity, InsertDate)
                VALUES (@OrderID, @ProductId, @Quantity, @InsertDate)";
                    connection.Execute(sql, new
                    {
                        OrderID = orderId,
                        ProductId = productId,
                        Quantity = quantity,
                        InsertDate = insertDate
                    });
                    Logger.LogInfo($"Sipariş detayı başarıyla eklendi. OrderID: {orderId}, ProductID: {productId}", Account.LoggedUser?.Username, null);
                    //MessageBox.Show("----" + orderId + " " + productId + " " + insertDate.ToString() );

                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Sipariş detayı eklenirken hata oluştu. OrderID: {orderId}, ProductID: {productId}", ex, Account.LoggedUser?.Username, null);
                //MessageBox.Show("Sipariş detayı eklenirken hata oluştu: " + ex.Message);
            }
        }


        // Masa durumunu çekmek için
        public string GetTableStatus(int tableId)
        {
            try
            {
                using (var connection = new SqlConnection(DBContext.connectionString))
                {
                    return connection.QueryFirstOrDefault<string>(
                        "SELECT Status FROM [Table] WHERE UID = @TableID",
                        new { TableID = tableId });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Masa durumu sorgulanırken hata oluştu. TableID: {tableId}", ex, Account.LoggedUser?.Username, null);
                //MessageBox.Show("Masa durumu sorgulanırken hata oluştu: " + ex.Message);
                return null;
            }
        }

        // waiter adını almak için
        public static string GetWaiterFullNameById(int waiterId)
        {
            using (var connection = new SqlConnection(DBContext.connectionString))
            {
                string query = "SELECT Name, Surname FROM Account WHERE UID = @WaiterId";
                var result = connection.QueryFirstOrDefault(query, new { WaiterId = waiterId });
                if (result != null)
                    return $"{result.Name} {result.Surname}";
                return "Bilinmiyor";
            }
        }

        // sipariş durumunu iptal etmek için
        public static void setOrderStatusToCanceled(int orderId)
        {
            try
            {
                using (var connection = new SqlConnection(DBContext.connectionString))
                {
                    string sql = "UPDATE [Order] SET Status = @Status WHERE UID = @OrderId";
                    connection.Execute(sql, new { Status = "İptal Edildi", OrderId = orderId });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Sipariş iptal edilirken hata oluştu. OrderID: {orderId}", ex, Account.LoggedUser?.Username, null);
                MessageBox.Show("Sipariş iptal edilirken hata oluştu: " + ex.Message);
            }
        }

        // sipariş detayını silmek için
        public void DeleteOrderDetail(int orderId, string productNo)
        {
            try
            {
                using (var connection = new SqlConnection(DBContext.connectionString))
                {
                    string sql = "DELETE FROM OrderDetail WHERE OrderID = @OrderId AND ProductId = @ProductNo";
                    connection.Execute(sql, new { OrderId = orderId, ProductNo = productNo });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Sipariş detayı silinirken hata oluştu. OrderID: {orderId}, ProductNo: {productNo}", ex, Account.LoggedUser?.Username, null);
                throw; // Hata fırlat
            }
        }


        // günlere göre sipariş sayılarını çekmek için
        public static Dictionary<DateTime, int> LoadOrderCountsByDay(int lastNDays = 7)
        {
            var result = new Dictionary<DateTime, int>();
            using (var context = new DBContext())
            {
                var endDate = DateTime.Today;
                var startDate = endDate.AddDays(-lastNDays + 1);

                // Tüm günleri sıfırla başlat
                for (var dt = startDate; dt <= endDate; dt = dt.AddDays(1))
                    result[dt] = 0;

                // SQL'de gruplama ile gün ve sipariş sayısı çek
                var data = context._connection.Query<(DateTime OrderDate, int Count)>(
                    @"SELECT CAST(OrderDate AS DATE) AS OrderDate, COUNT(*) AS Count
              FROM [Order]
              WHERE OrderDate >= @startDate AND OrderDate <= @endDate
              GROUP BY CAST(OrderDate AS DATE)",
                    new { startDate, endDate });

                foreach (var row in data)
                {
                    result[row.OrderDate] = row.Count;
                }
            }
            return result;
        }

        public List<Order> GetAllOrders()
        {
            using (var connection = new SqlConnection(DBContext.connectionString))
            {
                string sql = @"SELECT o.UID, o.TableID AS TableId, o.WaiterID AS WaiterId, 
                              a.Name + ' ' + a.Surname AS WaiterName, o.OrderDate, o.Status, o.TotalPrice as totalPrice, Note as note
                       FROM [Order] o
                       LEFT JOIN Account a ON o.WaiterID = a.UID";
                return connection.Query<Order>(sql).ToList();
            }
        }



        public List<Order> GetOrdersByWaiterId(int waiterId)
        {
            using (var connection = new SqlConnection(DBContext.connectionString))
            {
                string sql = @"SELECT o.UID AS Id, o.TableID AS TableId, o.WaiterID AS WaiterId, 
                              a.Name + ' ' + a.Surname AS WaiterName, o.OrderDate, o.Status, o.TotalPrice as totalPrice,Note as note
                       FROM [Order] o
                       LEFT JOIN Account a ON o.WaiterID = a.UID
                       WHERE o.WaiterID = @WaiterId";
                return connection.Query<Order>(sql, new { WaiterId = waiterId }).ToList();
            }
        }

   

        // sipariş hareketlerini almak için
        public List<OrderMovementClass> GetOrderMovements()
        {
            using (var connection = new SqlConnection(DBContext.connectionString))
            {
                string sql = @"
            SELECT 
                om.UID, 
                om.OrderID, 
                om.MovementDate, 
                om.OrderStatus, 
                om.EmployeeId,
                a.Name + ' ' + a.Surname AS WaiterName
            FROM OrderMovement om
            LEFT JOIN Account a ON om.EmployeeId = a.UID
            ORDER BY om.MovementDate DESC";
                return connection.Query<OrderMovementClass>(sql).ToList();
            }
        }


        // belirli bir sipariş ID'sine göre sipariş hareketlerini almak için
        public List<OrderMovementClass> GetOrderMovementsByOrderId(int orderId)
        {
            using (var connection = new SqlConnection(DBContext.connectionString))
            {
                string sql = @"SELECT UID, OrderID, MovementDate, OrderStatus, EmployeeId 
                       FROM OrderMovement 
                       WHERE OrderID = @OrderID
                       ORDER BY MovementDate DESC";
                return connection.Query<OrderMovementClass>(sql, new { OrderID = orderId }).ToList();
            }
        }



        public static List<Order> LoadOrdersForKitchen()
        {
            try
            {
                using (var dbContext = new DBContext())
                {
                    // Ödenmemiş siparişleri çekmek için sorguyu güncelle
                    string query = @"SELECT * FROM [Order] WHERE Status <> 'Ödendi' AND Status <> 'Tamamlandı' AND Status <> 'Servis Edildi' AND Status <> 'İptal Edildi'";
                    var orders = dbContext._connection.Query<Order>(query).ToList();
                    return orders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Siparişler yüklenirken hata oluştu: " + ex.Message);
                Logger.LogError("Siparişler yüklenirken hata oluştu.", ex, Account.LoggedUser?.Username, null);
                return new List<Order>();
            }
        }

        public static void UpdateOrderStatus(int orderId, string newStatus)
        {
            using (var dbContext = new DBContext())
            {
                string query = "UPDATE [Order] SET Status = @NewStatus WHERE UID = @OrderId";
                dbContext._connection.Execute(query, new { NewStatus = newStatus, OrderId = orderId });
            }
        }

        public List<OrderDetail> GetOrderDetailsByTableId(int tableId)
        {
            using (var context = new DBContext())
            {
                var orderDetails = context._connection.Query<OrderDetail>(
                    "SELECT * FROM OrderDetail WHERE OrderID = @TableId",
                    new { TableId = tableId }).ToList();

                return orderDetails;
            }
        }

        // Sipariş ID'sine göre siparişi almak için
        public Order GetOrderById(int orderId)
        {
            try
            {
                using (var connection = new SqlConnection(DBContext.connectionString))
                {
                    string sql = @"SELECT o.UID, o.TableID AS TableId, o.WaiterID AS WaiterId, 
                                  a.Name + ' ' + a.Surname AS WaiterName, o.OrderDate, o.Status, o.TotalPrice as totalPrice, o.Note as note
                           FROM [Order] o
                           LEFT JOIN Account a ON o.WaiterID = a.UID
                           WHERE o.UID = @OrderId";
                    return connection.QueryFirstOrDefault<Order>(sql, new { OrderId = orderId });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Sipariş ID {orderId} ile sorgulanırken hata oluştu.", ex, Account.LoggedUser?.Username, null);
                return null;
            }
        }

        // Sipariş detayındaki ürün miktarını güncellemek için
        public void UpdateOrderDetailQuantity(int orderId, string productNo, int newQuantity)
        {
            try
            {
                using (var connection = new SqlConnection(DBContext.connectionString))
                {
                    string sql = "UPDATE OrderDetail SET Quantity = @Quantity WHERE OrderID = @OrderId AND ProductId = @ProductNo";
                    connection.Execute(sql, new { Quantity = newQuantity, OrderId = orderId, ProductNo = productNo });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Sipariş detayı güncellenirken hata oluştu. OrderID: {orderId}, ProductNo: {productNo}", ex, Account.LoggedUser?.Username, null);
                throw;
            }
        }
        // pagination için
        public List<Order> GetPagedOrders(int start, int limit, int? waiterId = null)
        {
            using (var connection = new SqlConnection(DBContext.connectionString))
            {
                int intrnlStart = start + 1;
                string sql = @"
                   SELECT o.UID, o.TableID AS TableId, o.WaiterID AS WaiterId, 
                      a.Name + ' ' + a.Surname AS WaiterName, o.OrderDate, o.Status, o.TotalPrice as totalPrice, o.Note as note
                    FROM [Order] o
                    LEFT JOIN Account a ON o.WaiterID = a.UID
                    " + (waiterId.HasValue ? "WHERE o.WaiterID = @WaiterId" : "") + @"
                    ORDER BY o.OrderDate DESC, o.UID DESC
                    OFFSET @Start ROWS FETCH NEXT @Limit ROWS ONLY";
                Logger.LogInfo("** GetPageOrders start = " + intrnlStart.ToString() + ", Limit = " + limit.ToString() + "\r\n" + sql + "\r\n---------\r\n");
                return connection.Query<Order>(sql, new { Start = (intrnlStart * limit) - limit, Limit = limit, WaiterId = waiterId }).ToList();
            }


        }

        // her masa (TableID) için en güncel (son) siparişi getirir
        //public List<Order> GetLastOrdersForAllTables()
        //{
        //    using (var connection = new SqlConnection(DBContext.connectionString))
        //    {
        //        string sql = @"
        //    SELECT *
        //    FROM (
        //        SELECT *, 
        //               ROW_NUMBER() OVER (PARTITION BY TableID ORDER BY OrderDate DESC) AS rn
        //        FROM [Order]
        //    ) t
        //    WHERE rn = 1
        //    ORDER BY OrderDate DESC";
        //        return connection.Query<Order>(sql).ToList();
        //    }
        //}


        //public List<Order> GetLastOrdersForAllTablesPaged(int start, int limit)
        //{
        //    using (var connection = new SqlConnection(DBContext.connectionString))
        //    {
        //        int intrnlStart = start + 1;
        //        string sql = @"
        //    SELECT *
        //        FROM (
        //        SELECT *, ROW_NUMBER() OVER (PARTITION BY TableID ORDER BY OrderDate DESC, UID DESC) AS rn
        //        FROM [Order]
        //        ) t
        //        WHERE rn = 1
        //           ORDER BY OrderDate DESC, UID DESC
        //            OFFSET @Start ROWS FETCH NEXT @Limit ROWS ONLY";
        //        return connection.Query<Order>(sql, new { Start = (intrnlStart * limit) - limit, Limit = limit }).ToList();
        //    }
        //}

        public List<Order> GetLastOrdersForAllTablesPagedByWaiter(int start, int limit, int? waiterId = null)
        {
            using (var connection = new SqlConnection(DBContext.connectionString))
            {
                int intrnlStart = start + 1;
                string sql = @"
            SELECT *
            FROM (
                SELECT O.*, Tb.Status as Table_Status, ROW_NUMBER() OVER (PARTITION BY O.TableID ORDER BY O.OrderDate DESC, O.UID DESC) AS rn
                FROM [Order] O
                    LEFT JOIN [Table] as Tb on Tb.UID = O.TableID
                " + (waiterId.HasValue ? "WHERE O.WaiterID = @WaiterId" : "") + @"
            ) t
            WHERE rn = 1
            AND Upper(Table_Status)='DOLU'
            ORDER BY OrderDate DESC, UID DESC
            OFFSET @Start ROWS FETCH NEXT @Limit ROWS ONLY";
                return connection.Query<Order>(sql, new { Start = (intrnlStart * limit) - limit, Limit = limit, WaiterId = waiterId }).ToList();
            }
        }
    }


    }
