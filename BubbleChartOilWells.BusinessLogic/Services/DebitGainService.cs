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
    public class DebitGainService
    {
        private const double MAP_NULL_VALUE = 9999900;

        public DebitGainService()
        {
            // TODO: Logger
        }

        public ResultResponse<MapVM> CreateDebitGainMap(DebitGainVM debitGainVM, string mapType)
        {
            try
            {
                var z = new List<double>();
                var KHValues = new List<double>();
                if (debitGainVM.KH == null)
                {
                    for (int i = 0; i < debitGainVM.K.Z.Count; i++)
                    {
                        if (debitGainVM.K.Z[i] == MAP_NULL_VALUE || debitGainVM.H.Z[i] == MAP_NULL_VALUE)
                        {
                            KHValues.Add(MAP_NULL_VALUE);
                            continue;
                        }

                        KHValues.Add(debitGainVM.K.Z[i] * debitGainVM.H.Z[i]);
                    }
                }
                else
                {
                    KHValues = debitGainVM.KH.Z;
                }

                switch (mapType)
                {
                    //            KH(Pr-Pwf)                Ko          Kw                    
                    // q = ___________________________ * (________ + ________)            
                    //     18.41*(ln(re/rw)-0.75+Stot)     MUo*Bo     MUw*Bw
                    case "liquid":
                    {
                        for (int i = 0; i < debitGainVM.Pr.Z.Count; i++)
                        {
                            if (KHValues[i] != MAP_NULL_VALUE)
                            {
                                // z.Add(KHValues[i] * (debitGainVM.Pr.Z[i] - debitGainVM.Pwf)
                                //       / (18.41 * (Math.Log(debitGainVM.Re / debitGainVM.Rw) - 0.75 + debitGainVM.Stot))
                                //       * (debitGainVM.KoInEachMapCell[i] / (debitGainVM.MUo * debitGainVM.Bo) +
                                //          debitGainVM.KwInEachMapCell[i] / (debitGainVM.MUw * debitGainVM.Bw)));
                                //

                                // TODO: refactor
                                // water + oil

                                z.Add(KHValues[i] * (debitGainVM.Pr.Z[i] - debitGainVM.Pwf)
                                      / (18.41 * (Math.Log(debitGainVM.Re / debitGainVM.Rw) - 0.75 + debitGainVM.Stot))
                                      * (debitGainVM.KwInEachMapCell[i] / (debitGainVM.MUw * debitGainVM.Bw))
                                      + KHValues[i] * (debitGainVM.Pr.Z[i] - debitGainVM.Pwf)
                                      / (18.41 * (Math.Log(debitGainVM.Re / debitGainVM.Rw) - 0.75 + debitGainVM.Stot))
                                      * (debitGainVM.KoInEachMapCell[i] / (debitGainVM.MUo * debitGainVM.Bo)));
                            }
                            else
                            {
                                z.Add(MAP_NULL_VALUE);
                            }
                        }

                        break;
                    }

                    //            KH(Pr-Pwf)                Ko                     
                    // q = ___________________________ * (________)            
                    //     18.41*(ln(re/rw)-0.75+Stot)     MUo*Bo 
                    case "oil":
                    {
                        for (int i = 0; i < debitGainVM.Pr.Z.Count; i++)
                        {
                            if (KHValues[i] != MAP_NULL_VALUE)
                            {
                                z.Add(KHValues[i] * (debitGainVM.Pr.Z[i] - debitGainVM.Pwf)
                                      / (18.41 * (Math.Log(debitGainVM.Re / debitGainVM.Rw) - 0.75 + debitGainVM.Stot))
                                      * (debitGainVM.KoInEachMapCell[i] / (debitGainVM.MUo * debitGainVM.Bo)));
                            }
                            else
                            {
                                z.Add(MAP_NULL_VALUE);
                            }
                        }

                        break;
                    }

                    //            KH(Pr-Pwf)                 Kw                    
                    // q = ___________________________ * (________)            
                    //     18.41*(ln(re/rw)-0.75+Stot)     MUw*Bw
                    case "water":
                    {
                        for (int i = 0; i < debitGainVM.Pr.Z.Count; i++)
                        {
                            if (KHValues[i] != MAP_NULL_VALUE)
                            {
                                z.Add(KHValues[i] * (debitGainVM.Pr.Z[i] - debitGainVM.Pwf)
                                      / (18.41 * (Math.Log(debitGainVM.Re / debitGainVM.Rw) - 0.75 + debitGainVM.Stot))
                                      * (debitGainVM.KwInEachMapCell[i] / (debitGainVM.MUw * debitGainVM.Bw)));
                            }
                            else
                            {
                                z.Add(MAP_NULL_VALUE);
                            }
                        }

                        break;
                    }
                }


                var debitMapBitmap = ConvertToBitmap.GetMapBitmap(debitGainVM.Pr.BitmapSource.PixelWidth,
                    debitGainVM.Pr.BitmapSource.PixelHeight, z, 9999900);

                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(debitMapBitmap.GetHbitmap(), IntPtr.Zero,
                    Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                var debitMapVM = new MapVM
                {
                    // Name = $@"{mapType}Map~{DateTime.Now}",
                    Name = $@"{mapType}",
                    Width = debitGainVM.Pr.Width,
                    Height = debitGainVM.Pr.Height,
                    LeftBottomCoordinate = debitGainVM.Pr.LeftBottomCoordinate,
                    Z = z,
                    BitmapSource = bitmapSource
                };
                debitMapVM.BitmapSource.Freeze();
                debitMapVM.IsSelected = true;

                return ResultResponse<MapVM>.GetSuccessResponse(debitMapVM);
            }
            catch (Exception e)
            {
                // TODO: write error to log
                return ResultResponse<MapVM>.GetErrorResponse($@"Ошибка вычисления прироста дебита.{Environment.NewLine}
                                                              {e.Message}{Environment.NewLine}
                                                              {e.StackTrace}");
            }
        }

        public ResultResponse<List<double>> GetW(DebitGainVM liquidDebitGainVM, double Scw, double Sor)
        {
            try
            {
                var W = new List<double>();
                for (double Sw = Scw; Sw <= 1 - Sor; Sw += 0.01)
                {
                    W.Add((GetKrw(liquidDebitGainVM, Sw).Data * liquidDebitGainVM.MUo * liquidDebitGainVM.Bo)
                          / (GetKro(liquidDebitGainVM, Sw).Data * liquidDebitGainVM.MUw * liquidDebitGainVM.Bw +
                             GetKrw(liquidDebitGainVM, Sw).Data * liquidDebitGainVM.MUo * liquidDebitGainVM.Bo));
                }

                return ResultResponse<List<double>>.GetSuccessResponse(W);
            }
            catch (Exception e)
            {
                // TODO: write error to log
                return ResultResponse<List<double>>.GetErrorResponse(
                    $@"Ошибка вычисления обводненности.{Environment.NewLine}
                                                                            {e.Message}{Environment.NewLine}
                                                                            {e.StackTrace}");
            }
        }

        public ResultResponse<double> GetKrw(DebitGainVM liquidDebitGainVM, double Sw)
        {
            try
            {
                var result = liquidDebitGainVM.Krwor * Math.Pow((Sw - liquidDebitGainVM.Scw)
                                                                / (1 - liquidDebitGainVM.Scw - liquidDebitGainVM.Sor),
                    liquidDebitGainVM.Nw);

                return ResultResponse<double>.GetSuccessResponse(double.IsNaN(result) ? MAP_NULL_VALUE : result);
            }
            catch (Exception e)
            {
                // TODO: write error to log
                return ResultResponse<double>.GetErrorResponse($@"Ошибка вычисления значения ОФП по воде.{Environment.NewLine}
                                                                            {e.Message}{Environment.NewLine}
                                                                            {e.StackTrace}");
            }
        }

        public ResultResponse<double> GetKro(DebitGainVM liquidDebitGainVM, double Sw)
        {
            try
            {
                var result = liquidDebitGainVM.Krocw * Math.Pow((1 - Sw - liquidDebitGainVM.Sor)
                                                                / (1 - liquidDebitGainVM.Scw - liquidDebitGainVM.Sor),
                    liquidDebitGainVM.No);

                return ResultResponse<double>.GetSuccessResponse(double.IsNaN(result) ? MAP_NULL_VALUE : result);
            }
            catch (Exception e)
            {
                // TODO: write error to log
                return ResultResponse<double>.GetErrorResponse($@"Ошибка вычисления ОФП по нефти.{Environment.NewLine}
                                                                            {e.Message}{Environment.NewLine}
                                                                            {e.StackTrace}");
            }
        }
    }
}