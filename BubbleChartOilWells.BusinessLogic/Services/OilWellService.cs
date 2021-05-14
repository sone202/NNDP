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

        public OilWellService(string jsonFileName)
        {
            oilWellRepository = new OilWellRepository(jsonFileName);

            // TODO: logger
        }

        public ResultResponse<List<OilWellVM>> ImportOilWells(string fileName)
        {
            //Месторождение_Координаты
            var oilWellsDto = new List<OilWellExcelDto>();
            //Месторождение_Параметры
            var monthlyObjectiveProductionDto = new List<MonthlyObjectiveProductionExcelDto>();
            
            // TODO: remove in release version
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
                            if (reader.GetString(0) == null)
                                continue;
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
                            debugObject = reader.GetValue(4);
                            debugString = Convert.ToString(reader.GetValue(4));

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

                var groupsByOilWell = monthlyObjectiveProductionDto.GroupBy(x => new { x.Region, x.Field, x.Area, x.OilWellName });

                foreach (var groupByOilWell in groupsByOilWell)
                {
                    var oilWell = oilWells.Find(x => x.Region == groupByOilWell.Key.Region &&
                                                     x.Field == groupByOilWell.Key.Field &&
                                                     x.Area == groupByOilWell.Key.Area &&
                                                     x.Name == groupByOilWell.Key.OilWellName);

                    if (oilWell != null)
                    {
                        oilWell.Objectives = new List<Objective>();
                        var groupsByObjective = groupByOilWell.GroupBy(x => x.ObjectiveName);

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

                return ResultResponse<List<OilWellVM>>.GetSuccessResponse(oilWellViewModels);
            }
            catch (Exception e)
            {
                // TODO: записывать ошибку в лог
                return ResultResponse<List<OilWellVM>>.GetErrorResponse("Ошибка чтения данных. Пожалуйста, импортируйте excel файл заново.");
            }
        }

        /*
         public ResultResponse<List<OilWellVM>> GetPump(string fileName)
        {
            //Месторождение_Координаты
            var oilWellsDto = new List<OilWellExcelDto>();
            //Месторождение_Параметры
            var monthlyObjectiveProductionDto = new List<MonthlyObjectiveProductionExcelDto>();

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
                            //debugObject = reader.GetValue(4);
                            //debugString = Convert.ToString(reader.GetValue(4));

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
                var groupsByOilWell = monthlyObjectiveProductionDto.GroupBy(x => new { x.Region, x.Field, x.Area, x.OilWellName });

                foreach (var groupByOilWell in groupsByOilWell)
                {
                    var oilWell = oilWells.Find(x => x.Region == groupByOilWell.Key.Region &&
                                                     x.Field == groupByOilWell.Key.Field &&
                                                     x.Area == groupByOilWell.Key.Area &&
                                                     x.Name == groupByOilWell.Key.OilWellName);

                    if (oilWell != null)
                    {
                        oilWell.Objectives = new List<Objective>();
                        var groupsByObjective = groupByOilWell.GroupBy(x => x.ObjectiveName);

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

            }
            catch (Exception)
            {

                throw;
            }

            return ResultResponse<List<OilWellVM>>.GetErrorResponse("Ошибка чтения данных. Пожалуйста, импортируйте excel файл заново.");
        }
        */
        
    }

}
