using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife.Models
{
    /// <summary>
    /// Класс для хранения данных сообщений, с целью их дальнейшего отображения на форме
    /// </summary>
    public class MessageData
    {
        public int Id { get; set; }
        public string Data { get; set; }
    }
}
