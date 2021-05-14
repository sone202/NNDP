using BubbleChartOilWells.BusinessLogic.Utils;
using BubbleChartOilWells.Contracts;
using BubbleChartOilWells.Contracts.Models.Dto;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using BubbleChartOilWells.DataAccess.Repositories;
using RDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace BubbleChartOilWells.BusinessLogic.Services
{
    public class SaveMapService
    {
        public ResultResponse<string> SaveAsIrapService(IEnumerable<MapVM> mapVMs, string folderPath)
        {
            try
            {
                mapVMs.ToList().ForEach(x => SaveAsIrap(x, folderPath + "\\" + x.Name + ".irap"));
                return ResultResponse<string>.GetSuccessResponse();
            }
            catch (Exception e)
            {
                return ResultResponse<string>.GetErrorResponse(@$"Ошибка сохранения карты в виде irap{Environment.NewLine}
                                                    {e.Message}{Environment.NewLine}
                                                    {e.StackTrace}");
            }
        }

        private void SaveAsIrap(MapVM mapVM, string filepath)
        {
            var numberFormatInfo = new NumberFormatInfo { NumberDecimalSeparator = "." };

            using (var writer = new StreamWriter(filepath))
            {
                // header
                writer.WriteLine($@"-996 {mapVM.BitmapSource.PixelHeight.ToString(numberFormatInfo)} {mapVM.cellWidth.ToString(numberFormatInfo)} {mapVM.cellHeight.ToString(numberFormatInfo)}");
                writer.Write($@"{mapVM.LeftBottomCoordinate.X.ToString(numberFormatInfo)} {(mapVM.LeftBottomCoordinate.X + mapVM.Width).ToString(numberFormatInfo)} ");
                writer.WriteLine($@"{mapVM.LeftBottomCoordinate.Y.ToString(numberFormatInfo)} {(mapVM.LeftBottomCoordinate.Y + mapVM.Height).ToString(numberFormatInfo)}");
                writer.WriteLine($@"{mapVM.BitmapSource.PixelWidth.ToString(numberFormatInfo)} 0.0 {(mapVM.LeftBottomCoordinate.X).ToString(numberFormatInfo)} {mapVM.LeftBottomCoordinate.Y.ToString(numberFormatInfo)}");
                writer.WriteLine($@"0 0 0 0 0 0 0");

                // body
                var counter = 0d;
                foreach (var z in mapVM.Z)
                {
                    counter++;
                    if (counter != 6)
                    {
                        writer.Write($@"{z.ToString(numberFormatInfo)} ");
                    }
                    else
                    {
                        writer.WriteLine($@"{z.ToString(numberFormatInfo)} ");
                        counter = 0;
                    }
                }
                writer.Close();
            }
        }
    }
}
