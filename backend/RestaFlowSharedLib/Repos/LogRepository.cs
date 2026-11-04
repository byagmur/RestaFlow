using Dapper;
using RestaurantSharedLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSharedLib.Repos
{
    public class LogRepository
    {
        private readonly DBContext _dbContext;

        public LogRepository(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<LogEntry> GetAllLogs()
        {
            string query = "SELECT Id, LogLevel, Message, StackTrace, Source, Timestamp, UserName FROM LogEntries";
            return _dbContext._connection.Query<LogEntry>(query).ToList();
        }

        public List<LogEntry> FilterLogs(string logLevel, DateTime? startDate, DateTime? endDate)
        {
            string query = @"
            SELECT Id, LogLevel, Message, StackTrace, Source, Timestamp, UserName
            FROM LogEntries
            WHERE (@logLevel IS NULL OR LogLevel = @logLevel)
              AND (@startDate IS NULL OR Timestamp >= @startDate)
              AND (@endDate IS NULL OR Timestamp <= @endDate)";

            return _dbContext._connection.Query<LogEntry>(query, new
            {
                logLevel = string.IsNullOrEmpty(logLevel) ? null : logLevel,
                startDate = startDate == DateTime.MinValue ? null : startDate,
                endDate = endDate == DateTime.MinValue ? null : endDate
            }).ToList();
        }
    }
}
