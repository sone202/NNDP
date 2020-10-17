using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Windows;
using System.ComponentModel;
using System.IO;

namespace BubbleChartOilWells.Models
{
    static class DataImport
    {
        public static string file_path { get; private set; }
        public static List<Bubble> data_list { get; private set; } = new List<Bubble>();
        public static Dictionary<OilWell, Bubble> data_dict_OB { get; private set; } = new Dictionary<OilWell, Bubble>();
        public static Dictionary<Bubble, OilWell> data_dict_BO { get; private set; } = new Dictionary<Bubble, OilWell>();

        static DataImport()
        {
            //-----------------------------------------------------------------
            // open file
            //file_path = GetFileName();    

            // DELETE BEFORE RELEASE
            file_path = "C:\\Users\\timzl\\Documents\\test.xlsx";
            //--------------------------------------------------------------------


            // excel data parsing           
            List<OilWell> oil_wells = GetExcelData(file_path);

            foreach (var oil in oil_wells)
            {
                data_list.Add(new Bubble(oil));
                data_dict_OB[oil] = new Bubble(oil); // writing data in List
                data_dict_BO[new Bubble(oil)] = oil; // writing data in List
            }
        }
        public static List<Bubble> GetDataList()
        {
            //-----------------------------------------------------------------
            // open file
            file_path = GetFileName();
            if (file_path == null)
            {
                //MessageBox.Show("Invalid file name");
                return null;
            }
            // DELETE BEFORE RELEASE
            //file_path = "C:\\Users\\timzl\\Documents\\test.xlsx";
            //--------------------------------------------------------------------


            // excel data parsing           
            List<OilWell> oil_wells = GetExcelData(file_path);
            if (oil_wells == null)
            {
                MessageBox.Show("No data in file");
                return null;
            }

            foreach (var oil in oil_wells)
                data_list.Add(new Bubble(oil));

            return data_list;
        }
        private static string GetFileName()
        {
            OpenFileDialog OFD = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                Filter = "Excel all files (*.xlsx, *.xlsm, *.xltx, *.xltm)|*.xlsx; *.xlsm; *.xltx; *.xltm",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            return OFD.ShowDialog() == true ? OFD.FileName : null;

        }
        private static List<OilWell> GetExcelData(string file_path)
        {

            List<OilWell> oil_wells = new List<OilWell>();



            // excel data parsing           
            Excel.Application xlApp = new Excel.Application();  // OLE connecting
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(file_path);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            int row_count = xlRange.Rows.Count;
            int column_count = xlRange.Columns.Count;

            if (row_count < 2)            
                return null;
            


            // parsing
            for (int i = 2; i <= row_count; i++)
            {
                List<double> row = new List<double>();

                for (int j = 1; j <= column_count; j++)
                    row.Add(Double.Parse(xlRange.Cells[i, j]?.Value2?.ToString()));


                OilWell oil_well = new OilWell(row);

                double prod_sum = oil_well.oil_prod + oil_well.liquid_prod;
                if (prod_sum < Bubble.MINOilValue)
                    Bubble.MINOilValue = prod_sum;
                else if (prod_sum > Bubble.MAXOilValue)
                    Bubble.MAXOilValue = prod_sum;


                oil_wells.Add(oil_well); // writing data in List
            }

            // closing all files
            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);





            return oil_wells;
        }

    }
}

