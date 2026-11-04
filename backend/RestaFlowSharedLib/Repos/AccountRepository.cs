using RestaurantSharedLib;
using Dapper;
using RestaurantSharedLib.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Linq;

public class AccountRepository
{
    private readonly DBContext _dbContext;
    private readonly string _connectionString = DBContext.connectionString;

    public AccountRepository(DBContext dbContext)
    {
        _dbContext = dbContext;
    }

    // yeni bir kullanıcı hesabı ekle
    public void InsertAccount(string name, string surname, string username, string gsm, string mail, string role, string password)
    {
        using (var connection = new SqlConnection(DBContext.connectionString))
        {
            string query = @"INSERT INTO Account (Name, Surname, Username, GSM, Mail, Role, Password)
                         VALUES (@Name, @Surname, @Username, @GSM, @Mail, @Role, @Password)";
            connection.Execute(query, new { Name = name, Surname = surname, Username = username, GSM = gsm, Mail = mail, Role = role, Password = password });
        }
    }

    // kullanıcı hesabını güncelle
    public void UpdateAccount(string name, string surname, string username, string gsm, string mail, string role, string password)
    {
        using (var connection = new SqlConnection(DBContext.connectionString))
        {
            string query = @"UPDATE Account SET Name = @Name, Surname = @Surname, GSM = @GSM, Mail = @Mail, Role = @Role, Password = @Password
                         WHERE Username = @Username";
            connection.Execute(query, new { Name = name, Surname = surname, Username = username, GSM = gsm, Mail = mail, Role = role, Password = password });
        }
    }


    // toplam personel sayısını yükle
    public static int LoadTotalPersonnelCount()
    {
        try
        {
            using (var dbContext = new DBContext())
            {
                string query = @"SELECT COUNT(*) FROM Account";
                // Dapper ile sorgu çalıştırılıyor
                int personnelCount = dbContext._connection.QuerySingle<int>(query);
                return personnelCount;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Toplam personel sayısı yüklenirken hata oluştu: " + ex.Message);
            return 0;
        }
    }

    // kullanıcı adını kullanarak hesabı yükle
    public Account GetAccountByUsername(string username)
    {
        try
        {
            string query = @"SELECT 
                                UID as Id, 
                                Username, 
                                Name, 
                                Surname, 
                                Password, 
                                Mail, 
                                isAuth as IsAuthorized, 
                                Role,
                                GSM
                             FROM Account 
                             WHERE Username = @Username";

            // Dapper ile sorgu
            return _dbContext._connection.QueryFirstOrDefault<Account>(query, new { Username = username });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message, ex, Account.LoggedUser?.Username ?? Account.LoggedUser.Username, "GetAccountByUsername");
            return null;
        }
    }

    public void RegisterAccount(Account account)
    {
        try
        {
            string query = @"INSERT INTO Account 
                                (Username, Password, Mail, isAuth, Role, Name, Surname) 
                             VALUES 
                                (@Username, @Password, @Mail, @IsAuthorized, @Role, @Name, @Surname)";

            // Dapper ile ekleme
            _dbContext._connection.Execute(query, new
            {
                account.Username,
                account.Password,
                account.Mail,
                IsAuthorized = account.IsAuthorized,
                account.Role,
                account.Name,
                account.Surname
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message, ex, Account.LoggedUser?.Username ?? Account.LoggedUser.Username, "RegisterAccount");
            throw;
        }
    }

    public bool DeleteAccountByUsername(string username)
    {
        try
        {
            string query = "DELETE FROM Account WHERE Username = @Username";
            int affectedRows = _dbContext._connection.Execute(query, new { Username = username });
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message, ex, Account.LoggedUser?.Username ?? Account.LoggedUser.Username, "DeleteAccountByUsername");
            return false;
        }
    }

    public IEnumerable<Account> GetAllAccounts()
    {
        try
        {
            string query = @"SELECT 
                                UID as Id, 
                                Username, 
                                Name, 
                                Surname, 
                                Password, 
                                Mail, 
                                isAuth as IsAuthorized, 
                                Role 
                             FROM Account";
            return _dbContext._connection.Query<Account>(query);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message, ex, Account.LoggedUser?.Username ?? Account.LoggedUser.Username, "GetAllAccounts");
            return new List<Account>();
        }
    }

    public IEnumerable<Account> FilterAccounts(string searchText, string role)
    {
        try
        {
            var query = @"SELECT 
                            UID as Id, 
                            Username, 
                            Name, 
                            Surname, 
                            Password, 
                            Mail, 
                            isAuth as IsAuthorized, 
                            Role 
                         FROM Account
                         WHERE (@Role IS NULL OR Role = @Role)
                           AND (@SearchText IS NULL 
                                OR Username LIKE @SearchPattern
                                OR Name LIKE @SearchPattern
                                OR Surname LIKE @SearchPattern
                                OR Mail LIKE @SearchPattern
                                OR Role LIKE @SearchPattern)";

            return _dbContext._connection.Query<Account>(query, new
            {
                Role = string.IsNullOrEmpty(role) || role == "Tümü" ? null : role,
                SearchText = string.IsNullOrEmpty(searchText) ? null : searchText,
                SearchPattern = $"%{searchText}%"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message, ex, Account.LoggedUser?.Username ?? Account.LoggedUser.Username, "FilterAccounts");
            return new List<Account>();
        }
    }

    public Account Login(string username, string password)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string query = "SELECT UID as id, Name, Surname, username, password, isAuth, role FROM account WHERE username = @username AND password = @password";
            return connection.QueryFirstOrDefault<Account>(
                query,
                new { username, password }
            );
        }
    }

    public void InsertUser(Account user)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string query = "INSERT INTO Account (Name, Surname, Username, GSM, Mail, Role, Password) VALUES (@Name, @Surname, @Username, @GSM, @Mail, @Role, @Password)";
            connection.Execute(query, user);
        }
    }

    public void UpdateUser(Account user)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string query = "UPDATE Account SET Name = @Name, Surname = @Surname, GSM = @GSM, Mail = @Mail, Role = @Role, Password = @Password WHERE Username = @Username";
            connection.Execute(query, user);
        }
    }

    // Siparişlerin sipariş verildiği günlere göre performansını hesaplamak için

    // garsonların satış performansını hesaplamak için
    public IEnumerable<dynamic> GetSalesPerformanceByWaiter()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string query = @"
                SELECT 
                    a.Name AS WaiterName, 
                    SUM(o.TotalPrice) AS TotalSales
                FROM [Order] o
                JOIN Account a ON o.WaiterId = a.UID
                GROUP BY a.Name";

            return connection.Query(query).ToList();
        }
    }
}
