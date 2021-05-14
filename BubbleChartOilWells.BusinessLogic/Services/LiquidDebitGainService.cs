using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using BubbleChartOilWells.BusinessLogic.Utils;
using BubbleChartOilWells.Contracts;
using BubbleChartOilWells.Contracts.Models.Dto;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using ExcelDataReader;

namespace BubbleChartOilWells.BusinessLogic.Services
{
    public class LiquidDebitGainService
    {
        public LiquidDebitGainService()
        {
            // TODO: Logger
        }

        //            KH(Pr-Pwf)                Ko          Kw                    
        // q = ___________________________ * (________ + ________)            
        //     18.41*(ln(re/rw)-0.75+Stot)    MUo* Bo     MUw* Bw
        public ResultResponse<MapVM> CreateMapLiquidDebitGain(LiquidDebitGainVM liqDG)
        {
            try
            {
                var zValues = new List<double>();
                var KHValues = new List<double>();
                if (liqDG.KH == null)
                {
                    
                    for (int i = 0; i < liqDG.K.ZValues.Count; i++)
                    {
                        KHValues.Add(liqDG.K.ZValues[i] * liqDG.H.ZValues[i]);
                    }
                }
                else
                {
                    KHValues = liqDG.KH.ZValues;
                }

                for (int i = 0; i < liqDG.Pr.ZValues.Count; i++)
                {
                    if (KHValues[i] != 9999900)
                    {
                        zValues.Add(KHValues[i] * (liqDG.Pr.ZValues[i] - liqDG.Pwf) /
                        (18.41 * (Math.Log(liqDG.re / liqDG.rw) - 0.75 + liqDG.Stot)) *
                        (liqDG.Ko / (liqDG.MUo * liqDG.Bo) + liqDG.Kw / (liqDG.MUw * liqDG.Bw)));
                    }
                    else
                    {
                        zValues.Add(9999900);
                    }
                }

                var liquidDebitMapBitmap = ConvertToBitmap.GetBitmap(liqDG.Pr.BitmapSource.PixelWidth, liqDG.Pr.BitmapSource.PixelHeight, zValues, 9999900);

                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(liquidDebitMapBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                var liquidDebitMapVM = new MapVM
                {
                    Name = "LDtest",
                    Width = liqDG.Pr.Width,
                    Height = liqDG.Pr.Height,
                    LeftBottomCoordinate = liqDG.Pr.LeftBottomCoordinate,
                    ZValues = zValues,
                    BitmapSource = bitmapSource
                };
                liquidDebitMapVM.BitmapSource.Freeze();
                liquidDebitMapVM.IsSelected = true;

                return ResultResponse<MapVM>.GetSuccessResponse(liquidDebitMapVM);
            }
            catch (Exception e)
            {
                // TODO: write error to log
                return ResultResponse<MapVM>.GetErrorResponse($@"Ошибка вычисления прироста дебита жидкости.{Environment.NewLine}
                                                              {e.Message}{Environment.NewLine}
                                                              {e.StackTrace}");
            }
        }
    }
}
