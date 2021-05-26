using AutoMapper;
using BubbleChartOilWells.Contracts;
using BubbleChartOilWells.Contracts.Models.Dto;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using BubbleChartOilWells.DataAccess.Models;
using BubbleChartOilWells.DataAccess.Repositories;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BubbleChartOilWells.BusinessLogic.Services
{
    public class OilWellService
    {
        private readonly OilWellRepository oilWellRepository;
        public List<OilWell> oilWellToExcel;




        public OilWellService(string jsonFileName)
        {
            oilWellRepository = new OilWellRepository(jsonFileName);

            // TODO: logger
        }

        public ResultResponse<List<OilWellVM>> ImportOilWells(string fileName)
        {
            var oilWellsDto = new List<OilWellExcelDto>();
            var monthlyObjectiveProductionDto = new List<MonthlyObjectiveProductionExcelDto>();
            object debugObject = -1;
            var debugString = "";
            try
            {
                using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        reader.Read();
                        while (reader.Read())
                        {
                            var oilWell = new OilWellExcelDto
                            {
                                Region = reader.GetString(0),
                                Field = reader.GetString(1),
                                Area = reader.GetString(2),
                                Name = Convert.ToString(reader.GetValue(3)),
                                X = reader.GetDouble(4),
                                Y = reader.GetDouble(5)
                            };
                            oilWellsDto.Add(oilWell);
                        }

                        reader.NextResult();

                        reader.Read();
                        while (reader.Read() && (reader.GetValue(3) != null)) // workaround
                        {
                            var rawDate = Convert.ToString(reader.GetValue(4)).Substring(0, 10);
                            var formattedDate = DateTime.ParseExact(rawDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                            var monthlyObjectiveProduction = new MonthlyObjectiveProductionExcelDto
                            {
                                Region = reader.GetString(0),
                                Field = reader.GetString(1),
                                Area = reader.GetString(2),
                                OilWellName = Convert.ToString(reader.GetValue(3)),
                                Date = formattedDate,
                                ObjectiveName = reader.GetString(5),
                                Pattern = reader.GetString(6),
                                State = reader.GetString(7),
                                WorkPeriod = GetExcelDouble(reader, 8),
                                LiquidDebit = GetExcelDouble(reader, 9),
                                OilDebit = GetExcelDouble(reader, 10),
                                WaterDebit = GetExcelDouble(reader, 11),
                                WaterEncroachment = GetExcelDouble(reader, 12),
                                InjectionCapacity = GetExcelDouble(reader, 13),
                                LiquidProd = GetExcelDouble(reader, 14),
                                OilProd = GetExcelDouble(reader, 15),
                                WaterProd = GetExcelDouble(reader, 16),
                                Injection = GetExcelDouble(reader, 17),
                                LiquidProdSum = GetExcelDouble(reader, 18),
                                OilProdSum = GetExcelDouble(reader, 19),
                                WaterProdSum = GetExcelDouble(reader, 20)
                            };
                            monthlyObjectiveProductionDto.Add(monthlyObjectiveProduction);
                        }
                    }
                }                

                var oilWells = Mapper.Map<List<OilWellExcelDto>, List<OilWell>>(oilWellsDto);

                /*var groupsByOilWell = monthlyObjectiveProductionDto.GroupBy(x => new { x.Region, x.Field, x.Area, x.OilWellName, x.OilDebit, x.Date });

                foreach (var oilWelll in oilWells)
                {
                    oilWelll.Objectives = new List<Objective>();

                    foreach (var groupByOilWell in groupsByOilWell)
                    {
                        var oilWell = oilWells.Find(x => x.Region == groupByOilWell.Key.Region &&
                                                         x.Field == groupByOilWell.Key.Field &&
                                                         x.Area == groupByOilWell.Key.Area &&
                                                         x.Name == groupByOilWell.Key.OilWellName);

                        if (oilWell != null)
                        {
                            //oilWell.Objectives = new List<Objective>();
                            var groupsByObjective = groupByOilWell.GroupBy(x => x.ObjectiveName);
                            //var groupsByObjective = monthlyObjectiveProductionDto.GroupBy(x => x.ObjectiveName);

                            foreach (var groupByObjective in groupsByObjective)
                            {
                                var monthlyObjectiveProduction = Mapper.Map<List<MonthlyObjectiveProductionExcelDto>, List<MonthlyObjectiveProduction>>(groupByObjective.ToList());

                                var objective = new Objective
                                {
                                    Name = groupByObjective.Key,
                                    OilWell = oilWell,
                                    MonthlyObjectiveProduction = monthlyObjectiveProduction
                                };

                                objective.MonthlyObjectiveProduction.ForEach(x => x.Objective = objective);

                                oilWell.Objectives.Add(objective);
                            }
                        }
                    }
                }*/


                /*foreach (var oilW in oilWells)
                {
                    oilW.Objectives = new List<Objective>();
                    var groupsByObjective = monthlyObjectiveProductionDto.GroupBy(x => x.ObjectiveName);
                    List<Objective> currentObjective = new List<Objective>();

                    foreach (var obj in monthlyObjectiveProductionDto)
                    {
                        //var monthlyObjectiveProduction = Mapper.Map<List<MonthlyObjectiveProductionExcelDto>, List<MonthlyObjectiveProduction>>(obj.ToList());

                        if (oilW.Region == obj.Region &&
                            (oilW.Field == obj.Field) &&
                            (oilW.Area == obj.Area) &&
                            (oilW.Name == obj.OilWellName))
                        {
                            var objective = new Objective
                            {
                                Name = obj.ObjectiveName,
                                date = obj.Date,
                                OilWell = oilW,
                                OilDebit = obj.OilDebit                               
                                //MonthlyObjectiveProduction
                            };

                            currentObjective.Add(objective);
                            //oilW.Objectives.Add(objective);
                        }
                    }
                    oilW.Objectives = currentObjective;                   
                }*/

                var groupsByOilWell = monthlyObjectiveProductionDto.GroupBy(x => new { x.Region, x.Field, x.Area, x.OilWellName, x.OilDebit, x.Date });


                foreach (var groupByOilWell in groupsByOilWell)
                {
                    var oilWell = oilWells.Find(x => x.Region == groupByOilWell.Key.Region &&
                                                     x.Field == groupByOilWell.Key.Field &&
                                                     x.Area == groupByOilWell.Key.Area &&
                                                     x.Name == groupByOilWell.Key.OilWellName);

                    if (oilWell != null)
                    {
                        //oilWell.Objectives = new List<Objective>();
                        var groupsByObjective = groupByOilWell.GroupBy(x => x.ObjectiveName);
                        //var groupsByObjective = monthlyObjectiveProductionDto.GroupBy(x => x.ObjectiveName);

                        foreach (var groupByObjective in groupsByObjective)
                        {
                            var monthlyObjectiveProduction = Mapper.Map<List<MonthlyObjectiveProductionExcelDto>, List<MonthlyObjectiveProduction>>(groupByObjective.ToList());

                            var objective = new Objective
                            {
                                Name = groupByObjective.Key,
                                OilWell = oilWell,
                                MonthlyObjectiveProduction = monthlyObjectiveProduction
                            };

                            objective.MonthlyObjectiveProduction.ForEach(x => x.Objective = objective);                          
                            oilWell.Objectives.Add(objective);                            
                        }
                    }
                }



                // сохранение в .json 
                oilWellRepository.BulkAdd(oilWells);
             

                // оставить данные по ежемесячной добыче только на последнюю дату
                foreach (var oilWell in oilWells)
                {                   
                   // TODO: добавить информацию по всем датам
                   foreach (var objective in oilWell.Objectives)
                   {
                        
                        //objective.MonthlyObjectiveProduction.
                        //var list = objective.MonthlyObjectiveProduction.Select(t => new { Date = t.Date});
                        //objective.MonthlyObjectiveProduction = objective.MonthlyObjectiveProduction.Where(x => x.Date == list).ToList();
                        /*var results = objective.MonthlyObjectiveProduction.GroupBy(
                            p => p.Date,
                            p=> p.Date,
                            )*/
                        //var ProductionDate = objective.MonthlyObjectiveProduction.GroupBy(x=> x.Date);
                        //objective.MonthlyObjectiveProduction = objective.MonthlyObjectiveProduction.Where(x => x.Date == ProductionDate).ToList();
                        //objective.MonthlyObjectiveProduction = objective.MonthlyObjectiveProduction.GroupBy(x => x.Date)
                            //.Select(g => new MonthlyObjectiveProduction { Date = g.Key, OilDebit = g.Select(x => x) });
                        //temp = objective.MonthlyObjectiveProduction.GroupBy(x => new {x.OilDebit, x.Date });
                        //objective.MonthlyObjectiveProduction = objective.MonthlyObjectiveProduction.Where(x => x.Date )
                        //objective.MonthlyObjectiveProduction = (List<MonthlyObjectiveProduction>)objective.MonthlyObjectiveProduction.GroupBy(x => new {x.Date, x.OilDebit });  
                   }
                }
                // оставить данные по ежемесячной добыче только на последнюю дату
                foreach (var oilWell in oilWells)
                {
                    foreach (var objective in oilWell.Objectives)
                    {
                        var maxProductionDate = objective.MonthlyObjectiveProduction.Max(x => x.Date);
                        objective.MonthlyObjectiveProduction = objective.MonthlyObjectiveProduction.Where(x => x.Date == maxProductionDate).ToList();                        
                    }
                }

                var oilWellViewModels = Mapper.Map<List<OilWell>, List<OilWellVM>>(oilWells);

                return ResultResponse<List<OilWellVM>>.GetSuccessResponse(oilWellViewModels);
            }
            catch (Exception e)
            {

                // TODO: write error to log
                return ResultResponse<List<OilWellVM>>.GetErrorResponse($@"Ошибка импортирования excel файла. Пожалуйста, проверьте формат файла.{Environment.NewLine}
                                                                        debugString:{debugString}{Environment.NewLine}
                                                                        debugObject:{debugObject}{Environment.NewLine}
                                                                        строка:{monthlyObjectiveProductionDto.Count}{Environment.NewLine}
                                                                        {e.Message}{e.StackTrace}");
            }
        }

        private double GetExcelDouble(IExcelDataReader reader, int column)
        {
            var valueExcel = reader.GetValue(column);
            var value = valueExcel == null ? 0.0 : Convert.ToDouble(valueExcel);

            return value;
        }
        public ResultResponse<List<OilWellVM>> GetOilWells()
        {
            try
            {
                var oilWells = oilWellRepository.GetAll();

                // оставить данные по ежемесячной добыче только на последнюю дату
                foreach (var oilWell in oilWells)
                {
                    foreach (var objective in oilWell.Objectives)
                    {
                        var maxProductionDate = objective.MonthlyObjectiveProduction.Max(x => x.Date);
                        objective.MonthlyObjectiveProduction = objective.MonthlyObjectiveProduction.Where(x => x.Date == maxProductionDate).ToList();
                    }
                }

                var oilWellViewModels = Mapper.Map<List<OilWell>, List<OilWellVM>>(oilWells);
                oilWellToExcel = oilWells;

                return ResultResponse<List<OilWellVM>>.GetSuccessResponse(oilWellViewModels);
            }
            catch (Exception e)
            {
                // TODO: записывать ошибку в лог
                return ResultResponse<List<OilWellVM>>.GetErrorResponse("Ошибка чтения данных. Пожалуйста, импортируйте excel файл заново.");
            }
        }
        public void ReportOfHeighbourHoles(double radius)
        {
            //(IEnumerable<OilWellVM>)OilWellVMs.GroupBy(x => new { x.Region, x.Field, x.Area, x.X, x.Y, x.Objectives })
            oilWellToExcel = (List<OilWell>)oilWellToExcel.GroupBy(x => new {x.Region, x.Field, x.Area, x.X, x.Y, x.Objectives });

            int maxNeigHole = 0;

            List<List<object>> tableOfHeighbourHoles = new List<List<object>>();
            List<object> row = new List<object>();

            foreach (var oilWellCurrent in oilWellToExcel)
            {
                row.Add(oilWellCurrent.Region);
                row.Add(oilWellCurrent.Field);
                row.Add(oilWellCurrent.Name);
                row.Add(oilWellCurrent.Objectives);                

                foreach (var oilWellNext in oilWellToExcel)
                {
                    row = new List<object>();

                   
                    if (oilWellCurrent != oilWellNext)
                    {
                        if (oilWellCurrent.Region == oilWellNext.Region && oilWellCurrent.Field == oilWellNext.Field)
                        {
                            double dx = oilWellCurrent.X - oilWellNext.X;
                            double dy = oilWellCurrent.Y - oilWellNext.Y;

                            if (Math.Pow(dx, 2) + Math.Pow(dy, 2) <= Math.Pow(radius, 2))
                            {
                                row.Add(oilWellCurrent);
                            }
                        }
                    }
                }
                if (maxNeigHole < row.Count)
                    maxNeigHole = row.Count;
                tableOfHeighbourHoles.Add(row);
            }

            try
            {

            }
            catch (Exception)
            {

                throw;
            }
        }

    }

   
}
