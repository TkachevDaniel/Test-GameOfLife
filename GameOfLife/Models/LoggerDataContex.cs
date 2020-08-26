using System.Data.Entity;

namespace GameOfLife.Models
{
    /// <summary>
    /// Класс контекста Базы Данных
    /// </summary>
    public class LoggerDataContext : DbContext
    {
        public LoggerDataContext() : base("DefaultConnection")
        {

        }
        public DbSet<LoggerData> Logger { get; set; }
    }
}
