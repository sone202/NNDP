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

            _file_path = "C:\\Users\\timzl\\Documents\\Обменный_файл(синтетика) - Copy.xlsx";

            // excel data parsing           
            List<List<List<string>>> raw_data = GetExcelData(_file_path);


            //el - line from worksheet 2
            System.Threading.Tasks.Parallel.ForEach(raw_data[1], el =>
            {
                try
                {                 // adding new region
                    if (_regions.Count != 0)
                    {
                        if (_regions[_regions.Count].name != el[0])
                            _regions.Add(new Region(el[0]));
                    }
                    else
                        _regions.Add(new Region(el[0]));



                    // adding new field to the region
                    ref List<Field> fields = ref _regions[_regions.Count - 1].fields;
                    if (fields.Count != 0)
                        foreach (Field field in fields)
                        {
                            if (!field.name.Equals(el[1]))
                            {
                                fields.Add(new Field(el[1]));
                                break;
                            }
                        }
                    else
                        fields.Add(new Field(el[1]));


                    // adding new areas
                    ref List<Area> areas = ref fields[fields.Count - 1].areas;
                    if (areas.Count != 0)
                        foreach (Area area in areas)
                        {
                            if (!area.name.Equals(el[2]))
                            {
                                areas.Add(new Area(el[2]));
                                break;
                            }
                        }
                    else
                        areas.Add(new Area(el[2]));


                    // adding new oil wells
                    ref List<OilWell> oil_wells = ref areas[areas.Count - 1].oil_wells;
                    if (oil_wells.Count != 0)
                        foreach (OilWell oil_well in oil_wells)
                        {
                            if (!oil_well.ID.Equals(el[3]))
                            {
                                oil_wells.Add(new OilWell(int.Parse(el[3])));
                                break;
                            }
                        }
                    else
                        oil_wells.Add(new OilWell(
                            int.Parse(el[3]),
                            _regions[_regions.Count - 1],
                            fields[fields.Count - 1],
                            areas[areas.Count - 1],
                            new Point(double.Parse(el[4]), double.Parse(el[5]))
                            ));

                    DataImport._oil_wells.AddRange(oil_wells);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            });







            //el - line from worksheet 1
            System.Threading.Tasks.Parallel.ForEach(raw_data[0], el =>
            {
                try
                {
                    // getting region
                    int CRI = 0; // current region index
                    for (int i = 0; i < _regions.Count; i++)
                        if (_regions[i].name == el[0])
                        {
                            CRI = i;
                            break;
                        }

                    // getting field
                    int CFI = 0; // current field index
                    for (int i = 0; i < _regions[CRI].fields.Count; i++)
                        if (_regions[CRI].fields[i].name == el[1])
                        {
                            CFI = i;
                            break;
                        }


                    // getting area
                    int CAI = 0; // current area index
                    for (int i = 0; i < _regions[CRI].fields[CFI].areas.Count; i++)
                        if (_regions[CRI].fields[CFI].areas[i].name == el[2])
                        {
                            CAI = i;
                            break;
                        }


                    // adding new objectives
                    ref List<Objective> objectives = ref _regions[CRI].fields[CFI].objectives;
                    if (objectives.Count != 0)
                        foreach (Objective objective in objectives)
                        {
                            if (!objective.name.Equals(el[5]))
                            {
                                objectives.Add(new Objective(el[5]));
                                break;
                            }
                        }
                    else
                        objectives.Add(new Objective(el[5]));


                    // adding oil well from current region and field to the objectives
                    ref List<OilWell> oil_wells = ref objectives[objectives.Count - 1].oil_wells;
                    if (oil_wells.Count != 0)
                        foreach (OilWell oil_well in oil_wells)
                        {
                            bool flag = false;
                            if (!oil_well.ID.Equals(el[3]))
                            {
                                foreach (OilWell DA_oil_well in DataImport._oil_wells)
                                    if (DA_oil_well.ID == Int32.Parse(el[3]))
                                    {
                                        oil_wells.Add(DA_oil_well);
                                        flag = true;
                                        break;
                                    }
                            }
                            if (flag)
                                break;
                        }
                    else
                        foreach (OilWell DA_oil_well in DataImport._oil_wells)
                            if (DA_oil_well.ID == Int32.Parse(el[3]))
                            {
                                oil_wells.Add(DA_oil_well);
                                break;
                            }


                    // searching current well 
                    foreach (OilWell DA_oil_well in DataImport._oil_wells)
                        if (DA_oil_well.ID == Int32.Parse(el[3]))
                        {

                            // adding objective to the well
                            if (DA_oil_well.objectives.Count != 0)
                                foreach (Objective objective in DA_oil_well.objectives)
                                    if (objective != objectives[objectives.Count - 1])
                                    {
                                        DA_oil_well.objectives.Add(objectives[objectives.Count - 1]);
                                        break;
                                    }
                                    else
                                        DA_oil_well.objectives.Add(objectives[objectives.Count - 1]);

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

                            DateTime date = Convert.ToDateTime(el[4]);
                            DA_oil_well.dates.Add(date);
                            DA_oil_well.work_period[date] = Int32.Parse(el[8]);

                            DA_oil_well.liquid_debit[date] = double.Parse(el[9]);
                            DA_oil_well.oil_debit[date] = double.Parse(el[10]);
                            DA_oil_well.water_debit[date] = double.Parse(el[11]);

                            DA_oil_well.water_encroachment[date] = double.Parse(el[12]);
                            DA_oil_well.injection_capacity[date] = double.Parse(el[13]);

                            DA_oil_well.liquid_prod[date] = double.Parse(el[14]);
                            DA_oil_well.oil_prod[date] = double.Parse(el[15]);
                            DA_oil_well.water_prod[date] = double.Parse(el[16]);

                            DA_oil_well.injection[date] = double.Parse(el[17]);
                            DA_oil_well.liquid_prod_SUM[date] = double.Parse(el[18]);
                            DA_oil_well.oil_prod_SUM[date] = double.Parse(el[19]);
                            DA_oil_well.water_prod_SUM[date] = double.Parse(el[20]);
                        }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }

            });

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

