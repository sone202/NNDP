﻿using BubbleChartOilWells.BusinessLogic.Utils;
using BubbleChartOilWells.Contracts;
using BubbleChartOilWells.Contracts.Models.Dto;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace BubbleChartOilWells.BusinessLogic.Services
{
    public class IrapMapService
    {
        public IrapMapService()
        {
            // TODO: logger
        }
         
        public ResultResponse<MapVM> ImportIrapMap(string fileName)
        {
            try
            {
                var irapMapDto = new IrapMapDto();

                using (var reader = new StreamReader(fileName))
                {
                    var firstRow = reader.ReadLine()?.Split(' ').Select(x => Convert.ToDouble(x, CultureInfo.InvariantCulture));
                    irapMapDto.PixelHeight = Convert.ToInt32(firstRow.ElementAt(1));
                    irapMapDto.CellWidth = firstRow.ElementAt(2);
                    irapMapDto.CellHeight = firstRow.ElementAt(3);

                    var secondRow = reader.ReadLine().Split(' ').Select(x => Convert.ToDouble(x, CultureInfo.InvariantCulture));
                    irapMapDto.MinX = secondRow.ElementAt(0);
                    irapMapDto.MaxX = secondRow.ElementAt(1);
                    irapMapDto.MinY = secondRow.ElementAt(2);
                    irapMapDto.MaxY = secondRow.ElementAt(3);

                    var thirdRow = reader.ReadLine().Split(' ').Select(x => Convert.ToDouble(x, CultureInfo.InvariantCulture));
                    irapMapDto.PixelWidth = Convert.ToInt32(thirdRow.ElementAt(0));

                    reader.ReadLine();

                    var zValues = reader.ReadToEnd().Replace(Environment.NewLine, string.Empty).Split(' ').Where(x => x != "");
                    irapMapDto.Z = zValues.Select(x => Convert.ToDouble(x, CultureInfo.InvariantCulture)).ToList();
                }

                var bitmap = ConvertToBitmap.GetMapBitmap(irapMapDto.PixelWidth, irapMapDto.PixelHeight, irapMapDto.Z, 9999900);

                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                bitmapSource.Freeze();

                var mapVM = new MapVM
                {
                    Name = Path.GetFileNameWithoutExtension(fileName),
                    Width = irapMapDto.MaxX - irapMapDto.MinX,
                    Height = irapMapDto.MaxY - irapMapDto.MinY,
                    PixelWidth = irapMapDto.PixelWidth,
                    PixelHeight = irapMapDto.PixelHeight,
                    IsSelected = true,
                    LeftBottomCoordinate = new System.Windows.Point(irapMapDto.MinX, irapMapDto.MinY),
                    BitmapSource = bitmapSource,
                    Z = irapMapDto.Z
                };

                return ResultResponse<MapVM>.GetSuccessResponse(mapVM);
            }
            catch (Exception e)
            {
                // TODO: write error to log
                return ResultResponse<MapVM>.GetErrorResponse($@"Ошибка импортирования irap карты.{Environment.NewLine}
                                                              {e.Message}{Environment.NewLine}
                                                              {e.StackTrace}");
            }
        }
    }
}
