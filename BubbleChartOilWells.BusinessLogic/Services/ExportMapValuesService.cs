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
    public class ExportMapValuesService
    {
        public ResultResponse<string> ExportMapValuesToExcel(IEnumerable<MapVM> mapVMs, IEnumerable<OilWellVM> oilWellVMs, string fileName, string sheetName = "Лист1")
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
        private void CreateSpreadsheetDocument(string fileName, string sheetName)
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
                Name = "Лист1"
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
        /// Получения номера ряда по ячейке (например, из "A2" получим 2)
        /// </summary>
        /// <param name="cellReference">Ячейка</param>
        /// <returns></returns>
        private uint? GetRowIndexByCellReference(string cellReference)
        {
            var charArr = cellReference.ToCharArray();
            var rowRef = charArr.SkipWhile(x => char.IsLetter(x)).ToArray();
            var indexStr = new string(rowRef);

            if (uint.TryParse(indexStr, out var res))
            {
                return res;
            }

            return null;
        }

        /// <summary>
        /// Получение буквы ячейки по ячейке
        /// </summary>
        /// <param name="cellReference">Ячейка</param>
        /// <returns></returns>
        private char? GetCellCharByCellReference(string cellReference)
        {
            var charArr = cellReference.ToCharArray();
            return charArr.TakeWhile(x => char.IsLetter(x)).ToList().FirstOrDefault();
        }

        /// <summary>
        /// Добавление наименования таблицы
        /// </summary>
        /// <param name="title">Наименование</param>
        /// <param name="sheetData">Данные листа</param>
        /// <param name="rowIndex">Номер ряда</param>
        /// <param name="cellChar">Буква колонки</param>
        private void AppendTitle(string title, SheetData sheetData, uint rowIndex, char cellChar)
        {
            if (title == null)
            {
                return;
            }

            var valueRow = GetOrAddRow(sheetData, rowIndex);

            var titleCell = new Cell { CellReference = $"{cellChar}{rowIndex}", StyleIndex = 1U };
            titleCell.CellValue = new CellValue(title);
            titleCell.DataType = new EnumValue<CellValues>(CellValues.String);

            valueRow.Append(titleCell);
        }

        /// <summary>
        /// Добавление стилей к файлу
        /// </summary>
        /// <param name="workbookPart"></param>
        private void ApplyStyles(WorkbookPart workbookPart, bool outlines)
        {
            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();

            var borders1 = new Borders() { Count = 2U };

            // Пустые границы
            var border1 = new Border();
            var leftBorder1 = new LeftBorder();
            var rightBorder1 = new RightBorder();
            var topBorder1 = new TopBorder();
            var bottomBorder1 = new BottomBorder();
            var diagonalBorder1 = new DiagonalBorder();

            border1.Append(leftBorder1);
            border1.Append(rightBorder1);
            border1.Append(topBorder1);
            border1.Append(bottomBorder1);
            border1.Append(diagonalBorder1);

            // Все границы
            var borderStyle = outlines ? BorderStyleValues.Thin : BorderStyleValues.None;

            var border2 = new Border();
            var leftBorder2 = new LeftBorder() { Style = borderStyle };
            var rightBorder2 = new RightBorder() { Style = borderStyle };
            var topBorder2 = new TopBorder() { Style = borderStyle };
            var bottomBorder2 = new BottomBorder() { Style = borderStyle };
            var diagonalBorder2 = new DiagonalBorder();

            var color1 = new Color() { Indexed = 64U };

            rightBorder2.Append(color1.CloneNode(false));
            topBorder2.Append(color1.CloneNode(false));
            bottomBorder2.Append(color1.CloneNode(false));
            leftBorder2.Append(color1.CloneNode(false));

            border2.Append(leftBorder2);
            border2.Append(rightBorder2);
            border2.Append(topBorder2);
            border2.Append(bottomBorder2);
            border2.Append(diagonalBorder2);

            borders1.Append(border1);
            borders1.Append(border2);

            // Форматы ячеек
            // 0 - обычная ячейка
            // 1 - ячейка с равнением по центру (для названия таблицы)
            // 2 - ячейка со всеми границами
            // 3 - ячейка со всеми границами и форматом отображения времени dd/mm/yy hh:mm:ss (см. numberingFormat1)
            // 4 - ячейка со всеми границами и переносом текста
            var cellFormats1 = new CellFormats() { Count = 4U };
            var cellFormat0 = new CellFormat() { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 0U, FormatId = 0U };
            var cellFormat1 = new CellFormat() { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 0U, FormatId = 0U, ApplyAlignment = true };
            var cellFormat2 = new CellFormat() { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 1U, FormatId = 0U, ApplyBorder = true };
            var cellFormat3 = new CellFormat() { NumberFormatId = 165U, FontId = 0U, FillId = 0U, BorderId = 1U, FormatId = 0U, ApplyNumberFormat = true, ApplyBorder = true };
            var cellFormat4 = new CellFormat() { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 1U, FormatId = 0U, ApplyAlignment = true, ApplyBorder = true };

            var alignment1 = new Alignment() { Horizontal = HorizontalAlignmentValues.Center };
            var alignment2 = new Alignment() { WrapText = true };

            cellFormat1.Append(alignment1);
            cellFormat4.Append(alignment2);

            cellFormats1.Append(cellFormat0);
            cellFormats1.Append(cellFormat1);
            cellFormats1.Append(cellFormat2);
            cellFormats1.Append(cellFormat3);
            cellFormats1.Append(cellFormat4);

            var cellStyles1 = new CellStyles() { Count = 1U };
            var cellStyle1 = new CellStyle() { Name = "Обычный", FormatId = 0U, BuiltinId = 0U };

            cellStyles1.Append(cellStyle1);

            // Форматы отображения
            var numberingFormats1 = new NumberingFormats() { Count = 1U };
            var numberingFormat1 = new NumberingFormat() { NumberFormatId = 165U, FormatCode = "dd/mm/yy\\ h:mm;@" };
            numberingFormats1.Append(numberingFormat1);

            stylesPart.Stylesheet = new Stylesheet
            {
                NumberingFormats = numberingFormats1,
                Fonts = new Fonts(new Font()),
                Fills = new Fills(new Fill()),
                Borders = borders1,
                CellStyleFormats = new CellStyleFormats(new CellFormat()),
                CellFormats = cellFormats1,
                CellStyles = cellStyles1
            };
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