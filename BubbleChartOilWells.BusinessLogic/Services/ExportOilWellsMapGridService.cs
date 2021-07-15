using AutoMapper;
using BubbleChartOilWells.Contracts;
using BubbleChartOilWells.Contracts.Models.Dto;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using BubbleChartOilWells.DataAccess.Models;
using BubbleChartOilWells.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Windows;

namespace BubbleChartOilWells.BusinessLogic.Services
{
    public class ExportOilWellsMapGridService
    {
        public ResultResponse<string> ExportMapValuesToExcel(IEnumerable<MapVM> mapVMs, IEnumerable<OilWellVM> oilWellVMs, string fileName, string sheetName = "Sheet1")
        {
            try
            {
                CreateSpreadsheetDocument(fileName, sheetName);

                using (var spreadsheetDocument = SpreadsheetDocument.Open(fileName, true))
                {
                    var workbookPart = spreadsheetDocument.WorkbookPart;
                    var worksheet = workbookPart.WorksheetParts.FirstOrDefault().Worksheet;
                    var sheetData = worksheet.GetFirstChild<SheetData>();

                    // setting start cell
                    var tableRowStartIndex = 1u;
                    var tableStartChar = 'A';

                    // writing oilWells to excel
                    var oilWellExcels = oilWellVMs.Select(x => new OilWellExcel(x)).ToList();
                    var coordinates = oilWellExcels.Select(x => new Point(x.X, x.Y)).ToList();

                    var oilWellheaders = new List<string> { "Месторождение", "Площадь", "Скважина", "Объект", "Х", "Y" };
                    var columnsWidth = CalculateColumnsLength(oilWellExcels, oilWellheaders, 30, Enumerable.Repeat<int?>(5, oilWellheaders.Count).ToArray());
                    ChangeColumnWidth(worksheet, sheetData, columnsWidth, tableStartChar);
                    AppendHeader(oilWellheaders, sheetData, tableRowStartIndex, tableStartChar);
                    SerialiazeToSheet(oilWellExcels, sheetData, tableRowStartIndex + 1, tableStartChar);
                    tableStartChar = (Char)(Convert.ToUInt16(tableStartChar) + 6);

                    // TODO: refactor
                    // writing map grid values to excel
                    foreach (var map in mapVMs)
                    {
                        var mapGridValues = GetMapGridValues(map, coordinates);
                        columnsWidth = CalculateColumnsLength(mapGridValues, new List<string> { map.Name }, 30, Enumerable.Repeat<int?>(5, 1).ToArray());
                        ChangeColumnWidth(worksheet, sheetData, columnsWidth, tableStartChar);
                        AppendHeader(new List<string>() { map.Name }, sheetData, tableRowStartIndex, tableStartChar);
                        SerialiazeToSheet(mapGridValues, sheetData, tableRowStartIndex + 1, tableStartChar++);
                    }

                    worksheet.Save();
                    workbookPart.Workbook.Save();
                    spreadsheetDocument.Save();
                    spreadsheetDocument.Close();
                }

                return ResultResponse<string>.GetSuccessResponse();
            }
            catch (Exception e)
            {
                // TODO: write error to log
                return ResultResponse<string>.GetErrorResponse($@"Ошибка экспорта значенй сетки карты в excel.{Environment.NewLine}
                                                              {e.Message}{Environment.NewLine}
                                                              {e.StackTrace}");
            }
        }

        /// <summary>
        /// Добавление шапки таблицы
        /// </summary>
        /// <param name="headers">Заголовки</param>
        /// <param name="sheetData">Данные листа</param>
        /// <param name="rowIndex">Номер ряда</param>
        /// <param name="cellChar">Буква колонки</param>
        private void AppendHeader(IEnumerable<string> headers, SheetData sheetData, uint rowIndex, char cellChar)
        {
            var valueRow = GetOrAddRow(sheetData, rowIndex);
            foreach (var header in headers)
            {
                var valueCell = new Cell { CellReference = $"{cellChar++}{rowIndex}", StyleIndex = 2U };

                valueCell.DataType = new EnumValue<CellValues>(CellValues.String);
                valueCell.CellValue = new CellValue(header);
                valueRow.AppendChild(valueCell);
            }
        }

        /// <summary>
        /// Создание нового документа xlsx
        /// </summary>
        /// <param name="stream">Поток</param>
        /// <param name="sheetName">Наименование листа</param>
        /// <returns></returns>
        private void CreateSpreadsheetDocument(string fileName, string sheetName = "Sheet1")
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.
            var spreadsheetDocument = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document.
            var workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Add Sheets to the Workbook.
            var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            sheets.Append(new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = sheetName
            });

            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();
        }

        /// <summary>
        /// Получение ряда по индексу
        /// </summary>
        /// <param name="sheetData">Лист с ячейками</param>
        /// <param name="rowIndex">Индекс ряда</param>
        /// <returns></returns>
        private Row GetRow(SheetData sheetData, uint rowIndex)
        {
            return sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == rowIndex);
        }

        /// <summary>
        /// Получение или добавления ряда по индексу
        /// </summary>
        /// <param name="sheetData">Лист с ячейками</param>
        /// <param name="rowIndex">Индекс ряда</param>
        /// <returns></returns>
        private Row GetOrAddRow(SheetData sheetData, uint rowIndex)
        {
            var existingRow = GetRow(sheetData, rowIndex);

            if (existingRow == null)
            {
                existingRow = new Row { RowIndex = rowIndex };
                sheetData.Append(existingRow);
            }

            return existingRow;
        }

        /// <summary>
        /// Сериализация данных
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">Данные</param>
        /// <param name="sheetData">Данные листа</param>
        /// <param name="currentRowIndex">Номер ряда, с которого начнется сериализация</param>
        /// <param name="startCellChar">Буква колонки, с которой начнется сериализация</param>
        private void SerialiazeToSheet<T>(IEnumerable<T> items, SheetData sheetData, uint currentRowIndex, char startCellChar)
        {
            var properties = typeof(T).GetProperties();
            foreach (var item in items)
            {
                var valueRow = GetOrAddRow(sheetData, currentRowIndex);
                var currentCellChar = startCellChar;

                foreach (var property in properties)
                {
                    var value = item.GetType().GetProperty(property.Name)?.GetValue(item, null);
                    var valueCell = new Cell
                    {
                        CellReference = $"{currentCellChar++}{currentRowIndex}",
                        StyleIndex = 2U
                    };

                    // Если свойство является массивов, то пропускаем
                    if (property.PropertyType.IsArray)
                    {
                        continue;
                    }

                    if (property.PropertyType == typeof(DateTime?) || property.PropertyType == typeof(DateTime))
                    {
                        value = ((DateTime?)value)?.ToOADate().ToString(CultureInfo.InvariantCulture);
                        valueCell.StyleIndex = 3U;
                    }
                    else if (property.PropertyType.IsPrimitive)
                    {
                        valueCell.DataType = new EnumValue<CellValues>(CellValues.Number);
                        valueCell.CellValue = new CellValue(double.Parse(value.ToString()).ToString(new NumberFormatInfo { NumberDecimalSeparator = "," }));
                        valueRow.Append(valueCell);
                        continue;
                    }

                    if (property.PropertyType == typeof(string))
                    {
                        valueCell.DataType = new EnumValue<CellValues>(CellValues.String);
                    }

                    if (value != null && !string.IsNullOrEmpty(value.ToString()))
                    {
                        // Если длинная строка, то переносим текст
                        if (value.ToString().Length > 70)
                        {
                            valueCell.StyleIndex = 4U;
                        }

                        valueCell.CellValue = new CellValue(value.ToString());
                    }
                    else
                    {
                        valueCell.CellValue = new CellValue(string.Empty);
                    }
                    valueRow.Append(valueCell);

                }
                currentRowIndex++;
            }
        }

        
        /// <summary>
        /// Рассчёт ширины столбцов
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">Данные для сериализации</param>
        /// <param name="headers">Заголовки для колонок</param>
        /// <param name="customColumnMinLength">Кастомная миниальная ширина у колонок</param>
        /// <returns></returns>
        private uint[] CalculateColumnsLength<T>(IEnumerable<T> items, IEnumerable<string> headers, int maxColumnLength, int?[]? customColumnMinLength)
        {
            var properties = typeof(T).GetProperties();
            var columnsLength = new uint[properties.Length];

            var index = 0;
            foreach (var item in headers)
            {
                if (item.Length > columnsLength[index])
                    columnsLength[index] = (uint)item.Length + 2;
                index++;
            }

            foreach (var item in items)
            {
                index = 0;
                foreach (var property in properties)
                {
                    var value = item.GetType().GetProperty(property.Name)?.GetValue(item, null);

                    // Если свойство является массивов, то пропускаем
                    if (property.PropertyType.IsArray)
                    {
                        index++;
                        continue;
                    }

                    if (property.PropertyType == typeof(DateTime?) || property.PropertyType == typeof(DateTime))
                    {
                        value = ((DateTime?)value)?.ToString("yyyy-MM-ddTHH:mm:ss").Length;
                    }
                    else
                    {
                        value = value?.ToString().Length;
                    }

                    if (value != null && (uint)(int)value > columnsLength[index])
                    {
                        columnsLength[index] = (uint)(int)value + 2;
                    }
                    index++;
                }
            }

            // Ограничение по ширине
            for (var i = 0; i < columnsLength.Length; i++)
            {
                if (columnsLength[i] > maxColumnLength)
                {
                    columnsLength[i] = (uint)maxColumnLength;
                }
            }

            // Применяем минимальную ширину
            for (var i = 0; i < columnsLength.Length; i++)
            {
                if (customColumnMinLength != null &&
                    customColumnMinLength.Length > i &&
                    customColumnMinLength[i] != null &&
                    customColumnMinLength[i] > columnsLength[i])
                {
                    columnsLength[i] = (uint)customColumnMinLength[i].Value;
                }
            }

            return columnsLength;
        }

        /// <summary>
        /// Изменение ширины колонок
        /// </summary>
        /// <param name="worksheet">Рабочий лист</param>
        /// <param name="sheetData">Данные листа</param>
        /// <param name="columnsLength">Длины колонок</param>
        private void ChangeColumnWidth(Worksheet worksheet, SheetData sheetData, uint[] columnsLength, char tableStart)
        {
            var columns = new Columns();

            for (var i = 0; i < columnsLength.Length; i++)
            {
                columns.Append(new Column()
                {
                    Min = (uint)((tableStart % 64) + i),
                    Max = (uint)((tableStart % 64) + i),
                    Width = columnsLength[i],
                    CustomWidth = true
                });
            }
            worksheet.InsertBefore(columns, sheetData);
        }

        /// <summary>
        /// Получение списка значений сетки карты по скважинам
        /// </summary>
        /// <param name="mapVM"></param>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        private List<MapGridValue> GetMapGridValues(MapVM mapVM, IEnumerable<Point> coordinates)
        {
            var mapGridValues = new List<MapGridValue>();

            var startCoordinate = mapVM.LeftBottomCoordinate;
            var cellWidth = mapVM.Width / mapVM.BitmapSource.PixelWidth;
            var cellHeight = mapVM.Height / mapVM.BitmapSource.PixelHeight;

            // searching map values with oil well coordinate
            foreach (var point in coordinates)
            {
                var xCounter = 0;
                var yCounter = 0;

                var currentValue = -1d;
                for (double i = startCoordinate.X + cellWidth; i <= startCoordinate.X + mapVM.Width; i += cellWidth)
                {
                    if (point.X > startCoordinate.X + mapVM.Width || (point.Y > startCoordinate.Y + mapVM.Height))
                    {
                        break;
                    }

                    if (point.X < i)
                    {
                        for (double j = startCoordinate.Y + cellHeight; j <= startCoordinate.Y + mapVM.Height; j += cellHeight)
                        {
                            if (point.Y < j)
                            {
                                try
                                {
                                    currentValue = mapVM.Z[mapVM.BitmapSource.PixelWidth * yCounter + xCounter];
                                }
                                catch
                                {
                                    currentValue = -1;
                                }
                                break;
                            }
                            yCounter++;
                        }
                        break;
                    }
                    xCounter++;
                }
                mapGridValues.Add(new MapGridValue(currentValue));
            }

            return mapGridValues;
        }

    }

    // TODO: refactor
    public class OilWellExcel
    {
        public string Field { get; set; }
        public string Area { get; set; }
        public string Name { get; set; }
        public string Objectives { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public OilWellExcel(OilWellVM oilWellVM)
        {
            Field = oilWellVM.Field;
            Area = oilWellVM.Area;
            Name = oilWellVM.Name;
            Objectives = oilWellVM.Objectives.Count == 0 ? "-1" : oilWellVM.Objectives[0].Name;
            X = oilWellVM.X;
            Y = oilWellVM.Y;
        }
    }

    public class MapGridValue
    {
        public double Z { get; set; }
        public MapGridValue(double z)
        {
            Z = z;
        }
    }
}