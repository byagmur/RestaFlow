using RestaurantSharedLib.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper; // Dapper'ı eklemeyi unutmayın
              // ... diğer using'ler

namespace RestaurantSharedLib.Repos
{
   

    public class ReservationRepository
    {
        private readonly string _connectionString;
        public ReservationRepository(string connectionString) => _connectionString = connectionString;

        public void AddReservation(Reservation reservation)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO Reservation (TableId, CustomerName, CustomerPhone, Date, Note, Status)
                       VALUES (@TableId, @CustomerName, @CustomerPhone, @Date, @Note, @Status)";
                conn.Execute(sql, reservation);
            }
        }

        public IEnumerable<Reservation> GetReservations(DateTime? date = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM Reservation";
                if (date.HasValue)
                    sql += " WHERE Date = @Date";
                return conn.Query<Reservation>(sql, new { Date = date?.Date });
            }
        }

        public Reservation GetReservationById(int uid)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM Reservation WHERE UID = @UID";
                return conn.QueryFirstOrDefault<Reservation>(sql, new { UID = uid });
            }
        }

        public void UpdateReservation(Reservation reservation)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE Reservation SET
                        TableId = @TableId,
                        CustomerName = @CustomerName,
                        CustomerPhone = @CustomerPhone,
                        Date = @Date,
                        Note = @Note,
                        Status = @Status
                    WHERE UID = @UID";
                conn.Execute(sql, reservation);
            }
        }

        public void DeleteReservation(int uid)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM Reservation WHERE UID = @UID";
                conn.Execute(sql, new { UID = uid });
            }
        }
    }
}
