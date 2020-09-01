using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Windows;
using System.ComponentModel;
using System.IO;
using System.Globalization;

namespace BubbleChartOilWells.Models
{
    #region 2 Worksheet
    // 1 col = region
    // 2 col = field
    // 3 col = area
    // 4 col = oil well ID
    // 5 col = coordinate X
    // 6 col = coordinate Y
    #endregion

    #region 1 Worksheet
    // 01 col = region
    // 02 col = field
    // 03 col = area
    // 04 col = oil well ID
    // 05 col = date
    // 06 col = objective
    // 07 col = pattern
    // 08 col = state
    // 09 col = work_period

    // 10 col = liquid_debit;
    // 11 col = oil_debit;
    // 12 col = water_debit;

    // 13 col = water_encroachment;
    // 14 col = injection_capacity;

    // 15 col = liquid_prod;
    // 16 col = oil_prod;
    // 17 col = water_prod;
    // 18 col = injection;

    // 19 col = liquid_prod_SUM;
    // 20 col = oil_prod_SUM;
    // 21 col = water_prod_SUM;
    #endregion


    static class DataImport
    {
        private static string _file_path { get; set; }
        public static List<Region> _regions { get; private set; } = new List<Region>();
        public static List<OilWell> _oil_wells { get; private set; } = new List<OilWell>();

        static DataImport()
        {


            #region FileDialog
            //-----------------------------------------------------------------
            // open file
            //file_path = GetFileName();    
            //if (file_path == null)
            //{
            //    //MessageBox.Show("Invalid file name");
            //    return;
            //}
            // DELETE BEFORE RELEASE
            //--------------------------------------------------------------------
            #endregion

            _file_path = "C:\\Users\\timzl\\Documents\\Обменный_файл(синтетика).xlsx";

            // excel data parsing           
            List<List<List<string>>> raw_data = GetExcelData(_file_path);


            //el - line from worksheet 2
            //System.Threading.Tasks.Parallel.ForEach(raw_data[1], el =>
            //{
            //    try
            //    {
            //        #region 2 Worksheet
            //        // 1 col = region
            //        // 2 col = field
            //        // 3 col = area
            //        // 4 col = oil well ID
            //        // 5 col = coordinate X
            //        // 6 col = coordinate Y
            //        #endregion
            //        OilWell cur_oil_well = new OilWell
            //        {
            //            region = el[0],
            //            field = el[1],
            //            area = el[2],
            //            ID = Int32.Parse(el[3]),
            //            coordinates = new Point(double.Parse(el[4]), double.Parse(el[5]))
            //        };
            //        _oil_wells.Add(cur_oil_well);
            //    }
            //    catch (Exception e)
            //    {
            //        MessageBox.Show(e.Message);
            //        return;
            //    }
            //});
            foreach (var el in raw_data[1])
            {
                try
                {
                    #region 2 Worksheet
                    // 1 col = region
                    // 2 col = field
                    // 3 col = area
                    // 4 col = oil well ID
                    // 5 col = coordinate X
                    // 6 col = coordinate Y
                    #endregion
                    OilWell cur_oil_well = new OilWell
                    {
                        region = el[0],
                        field = el[1],
                        area = el[2],
                        ID = Int32.Parse(el[3]),
                        coordinates = new Point(double.Parse(el[4]), double.Parse(el[5]))
                    };
                    _oil_wells.Add(cur_oil_well);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }
            };
            ;





            //el - line from worksheet 1
            //System.Threading.Tasks.Parallel.ForEach(raw_data[0], el =>
            //{
            //    try
            //    {
            //        #region 1 Worksheet
            //        // 01 col = region
            //        // 02 col = field
            //        // 03 col = area
            //        // 04 col = oil well ID
            //        // 05 col = date
            //        // 06 col = objective
            //        // 07 col = pattern
            //        // 08 col = state
            //        // 09 col = work_period

            //        // 10 col = liquid_debit;
            //        // 11 col = oil_debit;
            //        // 12 col = water_debit;

            //        // 13 col = water_encroachment;
            //        // 14 col = injection_capacity;

            //        // 15 col = liquid_prod;
            //        // 16 col = oil_prod;
            //        // 17 col = water_prod;
            //        // 18 col = injection;

            //        // 19 col = liquid_prod_SUM;
            //        // 20 col = oil_prod_SUM;
            //        // 21 col = water_prod_SUM;
            //        #endregion
            //        int index = 0;
            //        for (int i = 0; i < _oil_wells.Count; i++)
            //        {
            //            if (
            //            _oil_wells[i].region == el[0] &&
            //            _oil_wells[i].field == el[1] &&
            //            _oil_wells[i].area == el[2] &&
            //            _oil_wells[i].ID == Int32.Parse(el[3]))
            //                index = i;
            //        }
            //        DateTime date = Convert.ToDateTime(el[4]);
            //        Dictionary<string, DateTime> dates_obj = new Dictionary<string, DateTime>();
            //        dates_obj[el[5]] = date;

            //        _oil_wells[index].dates.Add(el[5], date);
            //        _oil_wells[index].work_period[dates_obj] = Int32.Parse(el[8]);

            //        _oil_wells[index].liquid_debit[dates_obj] = double.Parse(el[9]);
            //        _oil_wells[index].oil_debit[dates_obj] = double.Parse(el[10]);
            //        _oil_wells[index].water_debit[dates_obj] = double.Parse(el[11]);

            //        _oil_wells[index].water_encroachment[dates_obj] = double.Parse(el[12]);
            //        _oil_wells[index].injection_capacity[dates_obj] = double.Parse(el[13]);
            //        _oil_wells[index].liquid_prod[dates_obj] = double.Parse(el[14]);
            //        _oil_wells[index].oil_prod[dates_obj] = double.Parse(el[15]);
            //        _oil_wells[index].water_prod[dates_obj] = double.Parse(el[16]);

            //        _oil_wells[index].injection[dates_obj] = double.Parse(el[17]);
            //        _oil_wells[index].liquid_prod_SUM[dates_obj] = double.Parse(el[18]);
            //        _oil_wells[index].oil_prod_SUM[dates_obj] = double.Parse(el[19]);
            //        _oil_wells[index].water_prod_SUM[dates_obj] = double.Parse(el[20]);

            //    }
            //    catch (Exception e)
            //    {
            //        MessageBox.Show(e.Message);
            //        return;
            //    }

            //});
            foreach (var el in raw_data[0])
            {
                try
                {
                    #region 1 Worksheet
                    // 01 col = region
                    // 02 col = field
                    // 03 col = area
                    // 04 col = oil well ID
                    // 05 col = date
                    // 06 col = objective
                    // 07 col = pattern
                    // 08 col = state
                    // 09 col = work_period

                    // 10 col = liquid_debit;
                    // 11 col = oil_debit;
                    // 12 col = water_debit;

                    // 13 col = water_encroachment;
                    // 14 col = injection_capacity;

                    // 15 col = liquid_prod;
                    // 16 col = oil_prod;
                    // 17 col = water_prod;
                    // 18 col = injection;

                    // 19 col = liquid_prod_SUM;
                    // 20 col = oil_prod_SUM;
                    // 21 col = water_prod_SUM;
                    #endregion
                    int index = 0;
                    for (int i = 0; i < _oil_wells.Count; i++)
                    {
                        if (
                        _oil_wells[i].region == el[0] &&
                        _oil_wells[i].field == el[1] &&
                        _oil_wells[i].area == el[2] &&
                        _oil_wells[i].ID == Int32.Parse(el[3]))
                        {
                            index = i;
                            break;
                        }
                    }
                    
                    DateTime date = DateTime.FromOADate(double.Parse(el[4]));
                    (string, DateTime) dates_obj = (el[5], date);

                    _oil_wells[index].dates.Add(dates_obj);
                    _oil_wells[index].work_period[dates_obj] = double.Parse(el[8]);

                    _oil_wells[index].liquid_debit[dates_obj] = double.Parse(el[9]);
                    _oil_wells[index].oil_debit[dates_obj] = double.Parse(el[10]);
                    _oil_wells[index].water_debit[dates_obj] = double.Parse(el[11]);

                    _oil_wells[index].water_encroachment[dates_obj] = double.Parse(el[12]);
                    _oil_wells[index].injection_capacity[dates_obj] = double.Parse(el[13]);
                    _oil_wells[index].liquid_prod[dates_obj] = double.Parse(el[14]);
                    _oil_wells[index].oil_prod[dates_obj] = double.Parse(el[15]);
                    _oil_wells[index].water_prod[dates_obj] = double.Parse(el[16]);

                    _oil_wells[index].injection[dates_obj] = double.Parse(el[17]);
                    _oil_wells[index].liquid_prod_SUM[dates_obj] = double.Parse(el[18]);
                    _oil_wells[index].oil_prod_SUM[dates_obj] = double.Parse(el[19]);
                    _oil_wells[index].water_prod_SUM[dates_obj] = double.Parse(el[20]);

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }

            };
            ;
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
        private static List<List<List<string>>> GetExcelData(string file_path)
        {

            List<OilWell> oil_wells = new List<OilWell>();

            // excel data parsing           
            Excel.Application xlApp = new Excel.Application();  // OLE connecting
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(file_path);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            int row_count = xlRange.Rows.Count;
            int column_count = xlRange.Columns.Count;



            #region 2 Worksheet
            // 1 col = region
            // 2 col = field
            // 3 col = area
            // 4 col = oil well ID
            // 5 col = coordinate X
            // 6 col = coordinate Y
            #endregion

            #region 1 Worksheet
            // 01 col = region
            // 02 col = field
            // 03 col = area
            // 04 col = oil well ID
            // 05 col = date
            // 06 col = objective
            // 07 col = pattern
            // 08 col = state
            // 09 col = work_period

            // 10 col = liquid_debit;
            // 11 col = oil_debit;
            // 12 col = water_debit;

            // 13 col = water_encroachment;
            // 14 col = injection_capacity;

            // 15 col = liquid_prod;
            // 16 col = oil_prod;
            // 17 col = water_prod;
            // 18 col = injection;

            // 19 col = liquid_prod_SUM;
            // 20 col = oil_prod_SUM;
            // 21 col = water_prod_SUM;
            #endregion

            // parsing 1 Worksheet
            List<List<string>> worksheet_1 = new List<List<string>>();
            for (int i = 2; i <= row_count; i++)
            {
                List<string> row = new List<string>();
                for (int j = 1; j <= column_count; j++)
                    row.Add(xlRange.Cells[i, j]?.Value2?.ToString());
                worksheet_1.Add(row);
            }


            // parsing 2 Worksheet
            xlWorksheet = xlWorkbook.Sheets[2];
            xlRange = xlWorksheet.UsedRange;

            row_count = xlRange.Rows.Count;
            column_count = xlRange.Columns.Count;
            List<List<string>> worksheet_2 = new List<List<string>>();

            for (int i = 2; i <= row_count; i++)
            {
                List<string> row = new List<string>();
                for (int j = 1; j <= column_count; j++)
                    row.Add(xlRange.Cells[i, j]?.Value2?.ToString());
                worksheet_2.Add(row);
            }
            ;
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



            List<List<List<string>>> raw_data = new List<List<List<string>>> { worksheet_2, worksheet_1 };
            return raw_data;
        }

    }
}

