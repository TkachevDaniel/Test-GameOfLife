using System;

namespace GameOfLife.Models
{
    /// <summary>
    /// Класс для хранения данных логирования из БД
    /// </summary>
    public class LoggerData
    {
        public int Id { get; set; }
        public DateTime BeginingGameTime { get; set; }
        public DateTime EndGameTime { get; set; }
        public string GridData { get; set; }
    }
}
