using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using GameOfLife.GameCore;

namespace TestGameOfLife
{

    public partial class MainWindow : Window
    {


        DispatcherTimer timer;//таймер игры
        SolidColorBrush colorAlive;//цвет живой клетки
        SolidColorBrush colorBackground;//цвет фона сетки

        int resolution = 600;
        int sizeRect = 20;

        GameCore gameCore;

        public MainWindow()
        {
            InitializeComponent();

            colorAlive = (SolidColorBrush)(new BrushConverter().ConvertFrom("#389649"));
            colorBackground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#222"));

            gameCore = new GameCore(resolution, sizeRect);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += TimerTick;
            CheckBoxClosedSystem.IsChecked = true;

            UpdateListGames();
        }

        void TimerTick(object sender, EventArgs e)
        {
            DrawNextGen();

            if (gameCore.CompareStateGrid())
            {
                timer.Stop();
                LabelGeneration.Content = "GAME OVER (DATA SAVED)";
                gameCore.ResetGenerationNumber();

                gameCore.SaveToDataBase();
                UpdateListGames();
            }

        }

        /// <summary>
        /// Функция делает отрисовку следующего поколения
        /// </summary>
        private void DrawNextGen()
        {
            canvas.Children.Clear();
            gameCore.NextGen();
            ShowGenerationLabel(gameCore.GenerationNumber);

            bool[,] grid = gameCore.GetCurrentStateGen();

            for (int x = 0; x < gameCore.Rows; x++)
            {
                for (int y = 0; y < gameCore.Cols; y++)
                {
                    if (grid[x, y])
                        AddDrawRect(x, y);
                }
            }
        }

        /// <summary>
        /// Функция отрисовки квадрата на поле
        /// </summary>
        /// <param name="x">координата по Х</param>
        /// <param name="y">координата по У</param>
        /// <param name="size">размер клетки</param>
        /// <param name="color">цвет клетки</param>
        private void DrawRect(int x, int y, int size, SolidColorBrush color)
        {
            Rectangle rect = new Rectangle();
            rect.Width = size - 2;
            rect.Height = size - 2;
            rect.Fill = color;

            canvas.Children.Add(rect);

            Canvas.SetLeft(rect, x * size + 2);
            Canvas.SetTop(rect, y * size + 2);
        }

        /// <summary>
        /// Функция отрисовки двумерного буффера данных
        /// </summary>
        /// <param name="buffer">буффер данных</param>
        private void DrawBufferArray(bool[,] buffer)
        {
            //очистка
            canvas.Children.Clear();

            //Random
            for (int x = 0; x < gameCore.Rows; x++)
            {
                for (int y = 0; y < gameCore.Cols; y++)
                {
                    if (buffer[x, y])
                        DrawRect(x, y, sizeRect, colorAlive);
                }
            }
        }

        /// <summary>
        /// Добавляет "живую" ячейку на сетку  
        /// </summary>
        /// <param name="x">координаты по Х</param>
        /// <param name="y">координаты по У</param>
        public void AddDrawRect(int x, int y)
        {
            DrawRect(x, y, sizeRect, colorAlive);
        }

        /// <summary>
        /// Функция удаляет "живую" ячейку с сетки  
        /// </summary>
        /// <param name="x">координаты по Х</param>
        /// <param name="y">координаты по У</param>
        public void RemoveDrawRect(int x, int y)
        {
            DrawRect(x, y, sizeRect, colorBackground);
        }

        /// <summary>
        /// Функция обработки нажатия мыши по сетке (canvas)
        /// </summary>
        private void CanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Canvas))
                return;

            int x = (int)e.GetPosition((Canvas)sender).X / sizeRect;
            int y = (int)e.GetPosition((Canvas)sender).Y / sizeRect;


            bool[,] grid = gameCore.GetCurrentStateGen();

            if (grid[x, y] == true)
            {
                gameCore.RemoveCell(x, y);
                RemoveDrawRect(x, y);
            }
            else
            {
                gameCore.AddCell(x, y);
                AddDrawRect(x, y);
            }

        }

        /// <summary>
        /// Функция обновление содержимого списка на форме
        /// </summary>
        private void UpdateListGames()
        {
            ListBoxGames.Items.Clear();

            foreach (string str in gameCore.GetMessageList())
                ListBoxGames.Items.Add(str);
        }

        /// <summary>
        /// Функция-обработчик Кнопки "Start" -- начало игры
        /// </summary>
        private void ButtonStartCommand(object sender, RoutedEventArgs e)
        {
            if (!gameCore.GridIsEmpty())
                timer.Start();
        }

        /// <summary>
        /// Функция Кнопки "Stop" - останновка игры
        /// </summary>
        private void ButtonStopCommand(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            LabelGeneration.Content = "GAME STOPED";
        }

        /// <summary>
        /// Функция-обработчик Кнопки "Random" - случайная генерация
        /// </summary>
        private void ButtonRandomCommand(object sender, RoutedEventArgs e)
        {
            gameCore.ClearValues();
            DrawBufferArray(gameCore.RandomGrid(5));
            LabelGeneration.Content = "";

        }

        /// <summary>
        /// Функция-обработчик Кнопки "Clear" - очистить поле
        /// </summary>
        private void ButtonClearCommand(object sender, RoutedEventArgs e)
        {
            //очистка
            timer.Stop();
            canvas.Children.Clear();
            gameCore.ClearValues();
            LabelGeneration.Content = "";
        }

        /// <summary>
        /// Функция-обработчик Кнопки "SaveToDataBase" - сохранения в БД
        /// </summary>
        private void ButtonSaveToDataBaseCommand(object sender, RoutedEventArgs e)
        {
            gameCore.SaveToDataBase();
            UpdateListGames();
        }

        /// <summary>
        /// Функция-обработчик Кнопки "LoadFromDataBase" - загрузка из БД
        /// </summary>
        private void ButtonLoadFromDataBaseCommand(object sender, RoutedEventArgs e)
        {
            if (ListBoxGames.SelectedIndex != -1)
            {
                gameCore.LoadFromDataBase(ListBoxGames.SelectedIndex);
                DrawBufferArray(gameCore.GetCurrentStateGen());
            }
        }


        /// <summary>
        /// Функция-обработчик Кнопки "DeleteFromDataBase" - удаления из БД
        /// </summary>
        private void ButtonDeleteFromDataBaseCommand(object sender, RoutedEventArgs e)
        {
            if (ListBoxGames.SelectedIndex != -1)
            {
                gameCore.DeleteFromDataBase(ListBoxGames.SelectedIndex);
                UpdateListGames();
            }
        }

        /// <summary>
        /// Функция-обработчик Кнопки "RandomFromList" - выбирает случайную конфигурацию из списка
        /// </summary>
        private void ButtonRandomFromListCommand(object sender, RoutedEventArgs e)
        {
            if (ListBoxGames.Items.Count != 0)
            {
                Random random = new Random();
                int idx = random.Next(ListBoxGames.Items.Count);
                ListBoxGames.SelectedItem = ListBoxGames.Items[idx];

                if (ListBoxGames.SelectedIndex != -1)
                {
                    gameCore.LoadFromDataBase(ListBoxGames.SelectedIndex);
                    DrawBufferArray(gameCore.GetCurrentStateGen());
                }
            }
        }

        /// <summary>
        /// Функция для показа текущего покаления
        /// </summary>
        private void ShowGenerationLabel(int gen)
        {
            LabelGeneration.Content = $"GENERATION: {gen}";
        }

        /// <summary>
        /// Функция-обработчик для переключения между замкнутой (в пределе бесконечной) и ограниченной сеткой
        /// </summary>
        private void CheckBoxClosedSystemCommand(object sender, RoutedEventArgs e)
        {
            gameCore.ClosedSystem = CheckBoxClosedSystem.IsChecked.Value;
        }
    }
}
