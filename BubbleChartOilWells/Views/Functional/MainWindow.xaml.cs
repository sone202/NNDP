using BubbleChartOilWells.BusinessLogic.Mappers;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using BubbleChartOilWells.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using LiveCharts;
using BubbleChartOilWells.Views.Functional;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Runtime.InteropServices;

namespace BubbleChartOilWells
{
    public partial class MainWindow : Window
    {
        static double radius = 300;
        static int n_rows;
        static int n_columns;
        static int n = 2;
        static string[,] data = new string[n_rows, n_columns];
        static double[,] dataframe = new double[n_rows, n];


        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainVM();

            AutoMapperConfigurator.Initialize();
        }

        public void OilWellMap_Checked(object sender, RoutedEventArgs e)
        {
            ProdMapRadioButton.IsEnabled = true;
            ProdSumMapRadioButton.IsEnabled = true;
            BubbleEnableCheckBox.IsEnabled = true;

            DrawingAreaUserControl.OilWellMap_Checked(sender, e);
        }

        private void MapVMsItemsControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawingAreaUserControl.MapVMsItemsControl_SizeChanged(sender, e);
        }

        public void Map_Checked(object sender, RoutedEventArgs e)
        {
            DrawingAreaUserControl.Map_Checked(sender, e);
        }
        public void MoveTo(object sender, MouseButtonEventArgs e)
        {
            DrawingAreaUserControl.MoveTo(sender, e);
        }

        private void ExportOilWellMapValuesButtonClick(object sender, RoutedEventArgs e)
        {
            ExportOilMapValuesWindow ExportOilMapValuesWindow = new ExportOilMapValuesWindow(DataContext);
            ExportOilMapValuesWindow.Show();
            MainWindowCloseButton.IsEnabled = false;
            ExportOilWellMapValuesButton.IsEnabled = false;
        }

        #region mininmize / maximize / close buttons
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e) => SystemCommands.MinimizeWindow(this);
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                SystemCommands.MaximizeWindow(this);
            }
            else
            {
                SystemCommands.RestoreWindow(this);
            }
        }
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e) => SystemCommands.CloseWindow(this);
        #endregion       
        
        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);
        }

        public class Hole
        {
            protected string region;
            public string Region
            {
                get { return region; }

            }
            protected string place;
            public string Place
            {
                get { return place; }
            }
            protected string square;
            public string Square
            {
                get { return square; }
            }
            protected string holes;
            public string Holes
            {
                get { return holes; }
            }
            protected double x;
            public double X
            {
                get { return x; }
            }
            protected double y;
            public double Y
            {
                get { return y; }
            }

            public Hole(string region, string place, string square, string holes, double x, double y)
            {
                this.region = region;
                this.place = place;
                this.square = square;
                this.holes = holes;
                this.x = x;
                this.y = y;
            }

        }
        private static string[,] Load_csv(string filename)
        {
            // Получить текст файла.
            string whole_file = System.IO.File.ReadAllText(filename).Replace(".", ",");

            // Разделение на строки.
            whole_file = whole_file.Replace('\n', '\r');
            string[] lines = whole_file.Split(new char[] { '\r' },
                StringSplitOptions.RemoveEmptyEntries);

            // Посмотрим, сколько строк и столбцов есть.
            int num_rows = lines.Length;
            int num_cols = lines[0].Split('	').Length;
            n_rows = num_rows;
            n_columns = num_cols;

            // Выделите массив данных.
            string[,] values = new string[num_rows, num_cols];

            // Загрузите массив.
            for (int r = 0; r < num_rows; r++)
            {
                string[] line_r = lines[r].Split('	');
                for (int c = 0; c < num_cols; c++)
                {
                    values[r, c] = line_r[c];
                    //Console.WriteLine(values[r,c]);
                }
            }

            // Возвращаем значения.
            return values;
        }
        static public double[,] Columns_read_csv(string[,] data)
        {
            double[,] dataframe = new double[n_rows, n];
            for (int i = 1; i < n_rows; i++)
            {
                double.TryParse(data[i, 4], out dataframe[i, 0]);
                double.TryParse(data[i, 5], out dataframe[i, 1]);
            }
            return dataframe;
        }
        static public bool isPointInCircle(double centerX, double centerY, double R, double x, double y)
        {
            if (isInRectangle(centerX, centerY, R, x, y))
            {
                double dx = centerX - x;
                double dy = centerY - y;
                dx *= dx;
                dy *= dy;
                double distanceSquared = dx + dy;
                double radiusSquared = R * R;
                return distanceSquared <= radiusSquared;
            }
            return false;
        }
        static public bool isInRectangle(double centerX, double centerY, double R, double x, double y)
        {
            return (x >= centerX - R && x <= centerX + R && y >= centerY - R && y <= centerY + R);
        }

        public void SearchNearHole_new(object sender, RoutedEventArgs e)
        {
            data = Load_csv("DataFrame_without_index.csv");
            dataframe = Columns_read_csv(data);
           
            Hole[] hol = new Hole[n_rows];
            for (int i = 0; i < n_rows; i++)
            {
                hol[i] = new Hole(data[i, 0], data[i, 1], data[i, 2], data[i, 3], dataframe[i, 0], dataframe[i, 1]);
            }         

            listexcel(hol);
        }

        static public void listexcel(Hole[] hol)
        {
            List<List<object>> Mas = new List<List<object>>();    //динамический двумерный массив
            List<object> nn = new List<object>();
            List<object> row = new List<object>();

            /*Excel.Application excelApp = new Excel.Application();
            // Создаём экземпляр рабочей книги Excel
            Excel.Workbook workBook;
            // Создаём экземпляр листа Excel
            Excel.Worksheet workSheet;
            workBook = excelApp.Workbooks.Add();
            workSheet = (Excel.Worksheet)workBook.Worksheets.get_Item(1);*/

            Excel.Application application = null;
            Excel.Workbooks workbooks = null;
            Excel.Workbook workbook = null;
            Excel.Sheets worksheets = null;
            Excel.Worksheet worksheet = null;

            Excel.Range cell = null;

            int maxNeigHole = 0;
            string obje = "Объект1";
            string obje2 = "Объект2";



            for (int i = 1; i < n_rows; i++)
            {
                row = new List<object>();

                row.Add(hol[i].Region);
                row.Add(hol[i].Place);
                row.Add(hol[i].Square);
                row.Add(hol[i].Holes);
                row.Add(obje);
                for (int k = 1; k < n_rows; k++)
                {
                    if (i != k)
                    {
                        if ((hol[i].Region == hol[k].Region) && (hol[i].Place == hol[k].Place) && (hol[i].Square == hol[k].Square))
                        {
                            if (isPointInCircle(hol[i].X, hol[i].Y, radius, hol[k].X, hol[k].Y))
                            {
                                row.Add(hol[k].Holes);
                                
                            }
                        }
                    }
                }
                if (maxNeigHole < row.Count)
                    maxNeigHole = row.Count;
                Mas.Add(row);
            }



            for (int i = 0; i < Mas.Count; i++)
            {
                var temp = Mas[i];
                if (temp.Count != 5)
                    nn.Add(temp);
            }


            try
            {
                application = new Excel.Application();
                
                    
                
                workbooks = application.Workbooks;
                workbook = workbooks.Add();
                worksheets = workbook.Worksheets; //получаем доступ к коллекции рабочих листов
                worksheet = worksheets.Item[1];//получаем доступ к первому листу



                cell = worksheet.Cells[1, 1];//получаем доступ к ячейке
                cell.Value = "Регион";//пишем строку в ячейку A1

                cell = worksheet.Cells[1, 2];
                cell.Value = "Месторождение";

                cell = worksheet.Cells[1, 3];
                cell.Value = "Площадь";

                cell = worksheet.Cells[1, 4];
                cell.Value = "Скважина";

                cell = worksheet.Cells[1, 5];
                cell.Value = "Объект";

                for (int k = 1; k <= maxNeigHole - 5; k++)
                {
                    string str = "Соседняя скв. " + k.ToString();
                    cell = worksheet.Cells[1, 5 + k];
                    cell.Value = str;

                }



                

                for (int i = 0; i < nn.Count; i++)
                {
                    List<object> temp = new List<object>();
                    temp = (List<object>)nn[i];


                    for (int j = 0; j < temp.Count; j++)
                    {
                        //string str = "Соседняя скв. " + j.ToString();

                        //cell = worksheet.Cells[1, 4 + j];
                        //cell.Value = str;

                        var ttmp = temp[j];
                        //Console.Write(temp[j] + " " );
                        cell = worksheet.Cells[i + 2, j + 1];
                        cell.Value = temp[j];


                    }
                    //Console.WriteLine();

                }
                application.Visible = true;
                application.Quit();
            }
            finally
            {

                Marshal.ReleaseComObject(cell);
                Marshal.ReleaseComObject(worksheet);
                Marshal.ReleaseComObject(worksheets);
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(workbooks);
                Marshal.ReleaseComObject(application);
            }



            /*foreach (object item in nn)
            {
                Console.WriteLine(item.ToString() + " " );
            }*/
            //excelApp.Visible = true;



        }

        static public void excelOutput(Hole[] hol)
        {
            Excel.Application application = null;
            Excel.Workbooks workbooks = null;
            Excel.Workbook workbook = null;
            Excel.Sheets worksheets = null;
            Excel.Worksheet worksheet = null;
            //переменная для хранения диапазона ячеек
            //в нашем случае - это будет одна ячейка
            Excel.Range cell = null;
            Excel.Range cell_r = null;
            Excel.Range cell_p_1 = null;
            Excel.Range cell_p_2 = null;
            Excel.Range cell_s = null;
            Excel.Range cell_x_1 = null;
            Excel.Range cell_y_1 = null;
            Excel.Range cell_x_2 = null;
            Excel.Range cell_y_2 = null;

            try
            {
                application = new Excel.Application
                {
                    Visible = true
                };
                workbooks = application.Workbooks;
                workbook = workbooks.Add();
                worksheets = workbook.Worksheets; //получаем доступ к коллекции рабочих листов
                worksheet = worksheets.Item[1];//получаем доступ к первому листу
                cell = worksheet.Cells[1, 1];//получаем доступ к ячейке
                cell.Value = "Регион";//пишем строку в ячейку A1

                cell = worksheet.Cells[1, 2];
                cell.Value = "Месторождение";

                cell = worksheet.Cells[1, 3];
                cell.Value = "Площадь";

                cell = worksheet.Cells[1, 4];
                cell.Value = "Координаты X";

                cell = worksheet.Cells[1, 5];
                cell.Value = "Координаты Y";

                cell = worksheet.Cells[1, 6];
                cell.Value = "Месторождение_сосед";

                cell = worksheet.Cells[1, 7];
                cell.Value = "Координаты X";

                cell = worksheet.Cells[1, 8];
                cell.Value = "Координаты Y";

                cell = worksheet.Cells[1, 9];
                cell.Value = "Радиус";

                cell = worksheet.Cells[2, 9];
                cell.Value = radius;

                int row = 2;

                for (int i = 1; i < n_rows - 1; i++)
                {
                    for (int k = 1; k < n_rows; k++)
                    {
                        if (i != k)
                        {
                            if ((hol[i].Region == hol[k].Region) && (hol[i].Place == hol[k].Place) && (hol[i].Square == hol[k].Square))
                            {
                                if (isPointInCircle(hol[i].X, hol[i].Y, radius, hol[k].X, hol[k].Y))
                                {
                                    cell_r = worksheet.Cells[row, 1];
                                    cell_r.Value = hol[i].Region;

                                    cell_p_1 = worksheet.Cells[row, 2];
                                    cell_p_1.Value = hol[i].Holes;

                                    cell_s = worksheet.Cells[row, 3];
                                    cell_s.Value = hol[i].Square;

                                    cell_x_1 = worksheet.Cells[row, 4];
                                    cell_x_1.Value = hol[i].X;

                                    cell_y_1 = worksheet.Cells[row, 5];
                                    cell_y_1.Value = hol[i].Y;

                                    cell_p_2 = worksheet.Cells[row, 6];
                                    cell_p_2.Value = hol[k].Holes;

                                    cell_x_2 = worksheet.Cells[row, 7];
                                    cell_x_2.Value = hol[k].X;

                                    cell_y_2 = worksheet.Cells[row, 8];
                                    cell_y_2.Value = hol[k].Y;

                                    row++;

                                }
                            }

                        }
                    }
                }
                application.Visible = true;

                application.Quit();
            }
            finally
            {
                //освобождаем память, занятую объектами
                Marshal.ReleaseComObject(cell);
                Marshal.ReleaseComObject(cell_r);
                Marshal.ReleaseComObject(cell_p_1);
                Marshal.ReleaseComObject(cell_p_2);
                Marshal.ReleaseComObject(cell_s);
                Marshal.ReleaseComObject(cell_x_1);
                Marshal.ReleaseComObject(cell_y_1);
                Marshal.ReleaseComObject(cell_x_2);
                Marshal.ReleaseComObject(cell_y_2);
                Marshal.ReleaseComObject(worksheet);
                Marshal.ReleaseComObject(worksheets);
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(workbooks);
                Marshal.ReleaseComObject(application);
            }
        }
    }
}