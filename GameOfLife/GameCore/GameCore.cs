using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using GameOfLife.Models;

namespace GameOfLife.GameCore
{
    /// <summary>
    /// Основной класс игровой логики 
    /// </summary>
    class GameCore
    {
        bool[,] Grid, OldGrid;//Grid - определяет текущее состояние сетки, OldGrid - определяет предыдущее состояние сетки
        LoggerDataContext db = new LoggerDataContext();// БД 
        List<MessageData> Messages = new List<MessageData>();//сообщение для вывода пользователю
        public int Rows { get; private set; } = 0;//количество строк в сетке
        public int Cols { get; private set; } = 0;//количество колонок в сетке
        public int GenerationNumber { get; private set; } = 0;//номер поколения
        public bool ClosedSystem { get; set; } = true;//Определяет будет ли система замкнута

        public GameCore(int resolution, int sizeRect)
        {
            Cols = resolution / sizeRect;
            Rows = resolution / sizeRect;
            Grid = new bool[Rows, Cols];
            OldGrid = new bool[Rows, Cols];


            db.Logger.Load();
            UpdateMessageListFromDB();

            //MessageLogger = new List<string>();
        }

        /// <summary>
        /// Функция обновления Сообщений из БД для формы
        /// </summary>
        public void UpdateMessageListFromDB()
        {
            foreach (LoggerData logger in db.Logger)
            {
                Messages.Add(new MessageData
                {
                    Id = logger.Id,
                    Data = $"Game #{logger.Id}: Begining Time: {logger.BeginingGameTime} End Time: {logger.EndGameTime}"
                }); ;
            }
        }
        /// <summary>
        /// Функция сохранения в базу данных
        /// </summary>
        public void SaveToDataBase()
        {
            if (!GridIsEmpty() && !CompareStateGridWhithLogger())
            {
                addToLoggingList();
                AddToMessageList();
            }
        }

        /// <summary>
        /// Функция загрузки из базы данных по индексу 
        /// </summary>
        /// <param name="index">индекс записи</param>
        public void LoadFromDataBase(int index)
        {

            foreach (LoggerData lg in db.Logger)
            {
                if (Messages[index].Id == lg.Id)
                {
                    Grid = ConvertFromString(lg.GridData);
                    break;
                }
            }
        }

        /// <summary>
        /// Функция удаления записи из БД по индексу
        /// </summary>
        /// <param name="index"></param>
        public void DeleteFromDataBase(int index)
        {

            foreach (LoggerData lg in db.Logger)
            {
                if (Messages[index].Id == lg.Id)
                {
                    db.Logger.Remove(lg);
                    Messages.RemoveAt(index);
                    break;
                }
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Функция конвертации массива данных в строку
        /// </summary>
        /// <param name="grid">массив данных</param>
        /// <returns></returns>
        private string ConvertToString(bool[,] grid)
        {

            var intArray = new int[Rows, Cols];

            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Rows; j++)
                    intArray[i, j] = grid[i, j] ? 1 : 0;

            var result = string.Join("", intArray.OfType<int>()//string.Join(",", intArray.OfType<int>()
            .Select((value, index) => new { value, index })
            .GroupBy(x => x.index / intArray.GetLength(1), x => x.value,
                (i, ints) => $":{string.Join("", ints)}"));//string.Join(",", ints)

            return result;
        }

        /// <summary>
        /// Функция конвертации строки в массив данных
        /// </summary>
        /// <param name="stringFullGrid">строка с данными</param>
        /// <returns></returns>
        private bool[,] ConvertFromString(string stringFullGrid)
        {

            bool[,] result = new bool[Rows, Cols];

            for (int _row = 0; _row < Rows; _row++)
                for (int _col = 0; _col < Cols; _col++)
                    result[_row, _col] = false;


            String[] strlistCols = stringFullGrid.Split(':', ' ');

            int row = 0, col = 0;
            foreach (String colStr in strlistCols)
            {
                if (colStr.Length != 0)
                {
                    //String[] strlistRows = colStr.Split(',', ' ');
                    foreach (char rowStr in colStr)
                    {

                        result[row, col] = (Char.GetNumericValue(rowStr) != 0) ? true : false;
                        col++;
                        if (col == Cols) col = 0;

                    }
                    row++;
                    if (row == Rows) row = 0;
                }

            }

            return result;
        }

        /// <summary>
        /// Функция поиска максимального индекса в списке
        /// </summary>
        /// <param name="list">список логируемых данных</param>
        /// <returns></returns>
        public int FindMaxIdMessage(List<MessageData> list)
        {
            if (list.Count == 0)
            {
                throw new InvalidOperationException("Empty list");
            }
            int maxId = int.MinValue;
            foreach (MessageData type in list)
            {
                if (type.Id > maxId)
                {
                    maxId = type.Id;
                }
            }
            return maxId;
        }

        /// <summary>
        /// Функция добавления в список сообщений
        /// </summary>
        public void AddToMessageList()
        {
            int maxIndex = (Messages.Count != 0) ? FindMaxIdMessage(Messages) : -1;
            if (db.Logger.Count<LoggerData>() != 0 && maxIndex == -1) maxIndex = db.Logger.OrderByDescending(u => u.Id).FirstOrDefault().Id - 1;

            Messages.Add(new MessageData
            {
                Id = db.Logger.OrderByDescending(u => u.Id).FirstOrDefault().Id,
                Data = $"Game #{maxIndex + 1}: Begining Time: {db.Logger.OrderByDescending(u => u.Id).FirstOrDefault().BeginingGameTime} End Time: {db.Logger.OrderByDescending(u => u.Id).FirstOrDefault().EndGameTime}"
            });
        }

        /// <summary>
        /// Функция возвращает список сообщений
        /// </summary>
        public List<string> GetMessageList()
        {
            List<string> msgs = new List<string>();
            foreach (MessageData str in Messages)
                msgs.Add(str.Data);

            return msgs;
        }

        /// <summary>
        /// Функция добавления данных в структуру лог и базу данных
        /// </summary>
        public void addToLoggingList()
        {
            db.Logger.Add(new LoggerData
            {
                Id = db.Logger.Count<LoggerData>(),
                BeginingGameTime = DateTime.Now,
                EndGameTime = DateTime.Now,
                GridData = ConvertToString(Grid)
            });

            db.SaveChanges();
        }

        /// <summary>
        /// Функция определения следующего поколения
        /// </summary>
        public void NextGen()
        {
            bool[,] newGrid = new bool[Rows, Cols];

            //отрисовка
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    var countBeside = CountBeside(row, col);

                    if (!Grid[row, col] && countBeside == 3)//может зародиться жизнь
                    {
                        newGrid[row, col] = true;
                    }
                    else if (Grid[row, col] && (countBeside < 2 || countBeside > 3))//клетка погибает
                    {
                        newGrid[row, col] = false;
                    }
                    else
                    {
                        newGrid[row, col] = Grid[row, col];
                    }
                }
            }
            OldGrid = Grid;
            Grid = newGrid;
            GenerationNumber++;
        }

        /// <summary>
        /// Функция очищения значений массива сетки
        /// </summary>
        public void ClearValues()
        {
            ResetGenerationNumber();

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    Grid[row, col] = false;
                }
            }
        }

        /// <summary>
        /// Функция сбрасывает номер поколения
        /// </summary>
        public void ResetGenerationNumber()
        {
            GenerationNumber = 0;
        }

        /// <summary>
        /// Функция сравнения текущего состояния сетки и предыдущего 
        /// </summary>
        public bool CompareStateGrid()
        {
            bool compare = true;
            for (int x = 0; x < Rows; x++)
            {
                for (int y = 0; y < Cols; y++)
                {
                    if (Grid[x, y] != OldGrid[x, y])
                    {
                        compare = false;
                        break;
                    }
                }
            }

            return compare;
        }

        /// <summary>
        /// Функция сравнения текущего состояния сетки и той, что в логгере 
        /// </summary>
        private bool CompareStateGridWhithLogger()
        {
            bool compare = false;
            string currentGrid = ConvertToString(Grid);
            foreach (LoggerData log in db.Logger)
            {
                if (log.GridData == currentGrid)
                {
                    compare = true;
                    break;
                }
            }

            return compare;
        }

        /// <summary>
        /// Функция проверки на пустоту массива сетки
        /// </summary>
        public bool GridIsEmpty()
        {
            bool isEmpty = true;

            for (int x = 0; x < Rows; x++)
            {
                for (int y = 0; y < Cols; y++)
                {
                    if (Grid[x, y] == true)
                    {
                        isEmpty = false;
                        break;
                    }
                }
            }

            return isEmpty;
        }

        /// <summary>
        /// Функция возвращает текущее состояние сетки
        /// </summary>
        public bool[,] GetCurrentStateGen()
        {
            bool[,] grid = new bool[Rows, Cols];
            grid = Grid;

            return grid;
        }

        /// <summary>
        /// Функция читает ячейки по соседству
        /// </summary>
        /// <param name="x"> позиция по Х</param>
        /// <param name="y">позиция по У</param>
        /// <returns></returns>
        private int CountBeside(int x, int y)
        {
            int counter = 0;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    int row, col;
                    if (ClosedSystem)
                    {
                        //чтобы заглянуть за край карты (замкнуть систему)
                        row = (y + j + Rows) % Rows;
                        col = (x + i + Cols) % Cols;

                    }
                    else
                    {
                        if ((y + j) >= Rows || (y + j) < 0) continue; else row = y + j;
                        if ((x + i) >= Cols || (x + i) < 0) continue; else col = x + i;
                    }



                    bool isSelfChecken = col == x && row == y;
                    bool isAlive = Grid[col, row];

                    if (isAlive && !isSelfChecken)//если есть жизнь и это не самопроверка
                    {
                        counter++;
                    }

                }
            }
            return counter;
        }

        /// <summary>
        /// Функция случайной генерации массива
        /// </summary>
        /// <param name="density">диапазон значений</param>
        /// <returns></returns>
        public bool[,] RandomGrid(int density)
        {
            bool[,] grid = new bool[Rows, Cols];
            //Random
            Random random = new Random();
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    Grid[row, col] = random.Next(density) == 0;
                }
            }

            grid = Grid;

            return grid;
        }

        /// <summary>
        /// Функция обновления состояния клетки
        /// </summary>
        /// <param name="x">позиция по Х</param>
        /// <param name="y">позиция по У</param>
        /// <param name="state">состояние</param>
        private void UpdateStateCell(int x, int y, bool state)
        {
            if (x >= 0 && x < Rows && y >= 0 && y < Cols)
                Grid[x, y] = state;
        }

        /// <summary>
        /// Функция добавления ячейки (переключение состояния на true)
        /// </summary>
        /// <param name="x">позиция по Х</param>
        /// <param name="y">позиция по У</param>
        public void AddCell(int x, int y)
        {
            UpdateStateCell(x, y, state: true);
        }

        /// <summary>
        /// Функция удаления ячейки (переключение состояния на false)
        /// </summary>
        /// <param name="x">позиция по Х</param>
        /// <param name="y">позиция по У</param>
        public void RemoveCell(int x, int y)
        {
            UpdateStateCell(x, y, state: false);
        }


    }
}
