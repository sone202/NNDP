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
    class DataImport
    {
        public string file_path { get; private set; }

        public DataImport(List<Bubble> bubbles)
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
                bubbles.Add(new Bubble(oil));
        }
        public DataImport(Dictionary<OilWell, Bubble> Oil_Well)
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
                Oil_Well[oil] = new Bubble(oil); // writing data in List
        }
        public DataImport(Dictionary<Bubble, OilWell> Oil_Well)
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

                Oil_Well[new Bubble(oil)] = oil; // writing data in List
        }

        private string GetFileName()
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.InitialDirectory = "c:\\";
            OFD.Filter = "Excel all files (*.xlsx, *.xlsm, *.xltx, *.xltm)|*.xlsx; *.xlsm; *.xltx; *.xltm|All files (*.*)|*.*";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;
            if (OFD.ShowDialog() == true)
            {
                return OFD.FileName;
            }
            else
                return null;
        }
        private List<OilWell> GetExcelData(string file_path)
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
            {
                MessageBox.Show("No Data", "Warning");
                return null;
            }


            // parsing
            for (int i = 2; i <= row_count; i++)
            {
                List<double> row = new List<double>();

                for (int j = 1; j <= column_count; j++)
                    if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                        row.Add(Double.Parse(xlRange.Cells[i, j].Value2.ToString()));


                OilWell oil_well = new OilWell(row);

                double prod_sum = oil_well.Oil_Production + oil_well.Liquid_Production;
                if (prod_sum < Bubble._min_oil_value)
                    Bubble._min_oil_value = prod_sum;
                else if (prod_sum > Bubble._max_oil_value)
                    Bubble._max_oil_value = prod_sum;


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

