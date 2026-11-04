using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using RestaurantSharedLib.Models;

namespace RestaurantSharedLib.Repos
{
    public class StockRepository
    {
        private readonly string _connectionString;

        public StockRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Stok kartlarını listelemek için
        public IEnumerable<dynamic> GetAllStocks(string filter = "")
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT UID, No, Name, Quantity, QuantityType, Spec1, Group1, Group2, StokQStatus FROM V_Stock";
                if (!string.IsNullOrEmpty(filter))
                {
                    query += " WHERE " + filter;
                }
                return connection.Query(query);
            }
        }


        // Stok kartını oluşturmak için
        public dynamic GetStockById(int uid)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT UID, No, Name, Quantity, QuantityType, Spec1, Group1, Group2 FROM Stock WHERE UID = @UID";
                return connection.QueryFirstOrDefault(query, new { UID = uid });
            }
        }

        public void DeleteStock(int uid)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM Stock WHERE UID = @UID";
                connection.Execute(sql, new { UID = uid });
            }
        }


        // Spec türlerini çekmek için
        public IEnumerable<dynamic> GetSpecTypes(string specDescriptor, string parentSpec = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT SpecName FROM SpecsTypes WHERE SpecDescriptor = @SpecDescriptor";
                if (!string.IsNullOrEmpty(parentSpec))
                {
                    query += " AND ParentSpec = @ParentSpec";
                    return connection.Query(query, new { SpecDescriptor = specDescriptor, ParentSpec = parentSpec });
                }
                return connection.Query(query, new { SpecDescriptor = specDescriptor });
            }
        }

        // Stok güncellemek için
        public void UpdateStock(int stockId, string no, string name, string quantityType, string spec1, string group1, string group2)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE Stock
                    SET No = @No, Name = @Name,
                        QuantityType = @QuantityType, Spec1 = @Spec1, 
                        Group1 = @Group1, Group2 = @Group2
                    WHERE UID = @StockId";

                connection.Execute(query, new
                {
                    StockId = stockId,
                    No = no,
                    Name = name,
                    QuantityType = quantityType,
                    Spec1 = spec1,
                    Group1 = group1,
                    Group2 = group2
                });
            }
        }

        // Stok eklemek için
        public void InsertStock(string no, string name, string quantityType, string spec1, string group1, string group2)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    INSERT INTO Stock (No, Name, QuantityType, Spec1, Group1, Group2)
                    VALUES (@No, @Name, @QuantityType, @Spec1, @Group1, @Group2)";

                connection.Execute(query, new
                {
                    No = no,
                    Name = name,
                    QuantityType = quantityType,
                    Spec1 = spec1,
                    Group1 = group1,
                    Group2 = group2
                });
            }
        }

        // Stok hareketlerini eklemek için
        public void AddStockMovement(int stockId, decimal quantity, decimal? salePrice = null, decimal? purchasePrice = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string movementQuery = @"
                    INSERT INTO StockMovement (StockID, MovType, MovDate, MovQuantity)
                    VALUES (@StockID, @MovType, @MovDate, @MovQuantity)";

                        connection.Execute(movementQuery, new
                        {
                            StockID = stockId,
                            MovType = 1,
                            MovDate = DateTime.Now,
                            MovQuantity = quantity
                        }, transaction);

                        if (quantity > 0 && (salePrice.HasValue || purchasePrice.HasValue))
                        {
                            string priceQuery = @"
                        INSERT INTO StockPrice (StockID, UpdateDate, SalePrice, PurchasePrice)
                        VALUES (@StockID, @UpdateDate, @SalePrice, @PurchasePrice)";

                            connection.Execute(priceQuery, new
                            {
                                StockID = stockId,
                                UpdateDate = DateTime.Now,
                                SalePrice = salePrice,
                                PurchasePrice = purchasePrice
                            }, transaction);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Ürünlerin fiyatlarını çekmek için
        public List<Stock> GetProductsByCategory(string category)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"SELECT p.UID, p.No, p.Name, p.Quantity, p.QuantityType, p.Spec1, p.Group1, p.Group2, sp.SalePrice as Price
                            FROM Stock p
                            LEFT JOIN StockPrice sp ON p.UID = sp.StockID
                                AND sp.UpdateDate = (
                                    SELECT MAX(UpdateDate) 
                                    FROM StockPrice 
                                    WHERE StockID = p.UID
                                )
                            WHERE (p.Spec1 = @Category)
                            ORDER BY p.Name
";

                var products = connection.Query<Stock>(query, new { Category = category }).ToList();

                // Eğer Price property'si float? ise null kontrolü yapmaya gerek yok.
                // Eğer float ise ve null gelebilirse:
                foreach (var product in products)
                {
                    if (product.Price == null)
                        product.Price = 0;
                }

                return products;
            }
        }

        // Stok hareketlerini çekmek için
        public IEnumerable<StockMovement> GetStockMovements()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT 
                sm.UID as Uid, 
                sm.StockID as StockId, 
                sm.MovType as MovementType, 
                sm.MovDate as MovementDate, 
                sm.MovQuantity as Quantity, 
                s.QuantityType, 
                s.No,
                s.Name as Name
            FROM StockMovement sm
              JOIN Stock s ON sm.StockID = s.UID
            ORDER BY MovementType DESC";

                return connection.Query<StockMovement>(query).ToList();
            }
        }

        // Spec veya Group1 değerlerini çekmek için
        public List<string> GetSpecs(string specDescriptor)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT SpecName FROM SpecsTypes WHERE SpecDescriptor = @SpecDescriptor";
                return connection.Query<string>(query, new { SpecDescriptor = specDescriptor }).ToList();
            }
        }

        // Seçili Spec'e göre Group1 değerlerini çekmek için
        public List<string> GetGroupsBySpec(string parentSpec)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT SpecName FROM SpecsTypes WHERE SpecDescriptor = 'Group1' AND ParentSpec = @ParentSpec";
                return connection.Query<string>(query, new { ParentSpec = parentSpec }).ToList();
            }
        }

        // Seçili Group1'e göre ürünleri çekmek için
        public List<Stock> GetProductsByGroup(string group1)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT UID, Name, Quantity FROM Stock WHERE Group1 = @Group1";
                return connection.Query<Stock>(query, new { Group1 = group1 }).ToList();
            }
        }
        // grup2 combosunu doldurmak için
        public List<string> GetGroups2ByGroup1(string parentGroup1)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT SpecName FROM SpecsTypes WHERE SpecDescriptor = 'Group2' AND ParentSpec = @ParentSpec";
                return conn.Query<string>(sql, new { ParentSpec = parentGroup1 }).AsList();
            }
        }


        // Ürün id'sine göre ürün no'yu çekmek için
        public string GetProductNoById(int productId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT No FROM Stock WHERE UID = @ProductId";
                return connection.QueryFirstOrDefault<string>(query, new { ProductId = productId });
            }
        }

        // Ürünlerin fiyatlarını çekmek için
        public IEnumerable<ProductPriceInfo> GetProductPrices()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"SELECT s.No, s.Name, sp.SalePrice, sp.PurchasePrice 
                         FROM Stock s
                         LEFT JOIN StockPrice sp ON s.UID = sp.StockID
                         WHERE sp.UpdateDate = (
                             SELECT MAX(UpdateDate) 
                             FROM StockPrice 
                             WHERE StockID = s.UID
                         )";
                return connection.Query<ProductPriceInfo>(query).ToList();
            }
        }

        public void DecreaseStock(int stockUid, int quantity)
        {
            using (var connection = new SqlConnection(DBContext.connectionString))
            {
                string sql = @"INSERT INTO StockMovement (StockID, MovType, MovDate, MovQuantity)
           VALUES (@StockID, @MovType, @MovDate, @MovQuantity)";

                connection.Execute(sql, new
                {
                    StockID = stockUid,         // int, ilgili ürünün UID'si
                    MovType = 0,                // int, çıkış için 0 (giriş için 1)
                    MovDate = DateTime.Now,     // DateTime, hareket tarihi
                    MovQuantity = quantity      // int, çıkış yapılan miktar
                });
            }
        }

        // Ürün no'suna göre stok UID'sini çekmek için
        public int GetStockUidByNo(string productNo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT UID FROM Stock WHERE No = @No";
                return connection.QueryFirstOrDefault<int>(query, new { No = productNo });
            }
        }


    }
}
