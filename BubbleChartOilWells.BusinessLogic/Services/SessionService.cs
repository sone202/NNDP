using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Xml;
using AutoMapper;
using BubbleChartOilWells.BusinessLogic.Utils;
using BubbleChartOilWells.Contracts;
using BubbleChartOilWells.Contracts.Models.Dto;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using BubbleChartOilWells.DataAccess.Models;
using BubbleChartOilWells.DataAccess.Repositories;

namespace BubbleChartOilWells.BusinessLogic.Services
{
    public class SessionService
    {
        public ResultResponse<string> SaveSession(string sessionFilename,string jsonOilWellsFilename, IEnumerable<MapVM> mapVMs, DebitGainVM debitGainVm)
        {
            try
            {
                var session = new Session();
                session.OilWells = new OilWellRepository(jsonOilWellsFilename).GetAll();
                session.Maps = Mapper.Map<IEnumerable<MapVM>, IEnumerable<Map>>(mapVMs);
                session.DebitGain = Mapper.Map<DebitGainVM, DebitGain>(debitGainVm);
                
                var sessionRepository = new SessionRepository(sessionFilename.Replace("boul", "json"));
                sessionRepository.BulkAdd(session);
                
                return ResultResponse<string>.GetSuccessResponse();
            }
            catch (Exception e)
            {
                return ResultResponse<string>.GetErrorResponse(@$"Ошибка сохранения сессии{Environment.NewLine}
                                                    {e.Message}{Environment.NewLine}
                                                    {e.StackTrace}");
            }
        }
        
        // TODO: workaround return value
        public ResultResponse<IEnumerable<object>> OpenSession(string sessionFilename)
        {
            try
            {
                // var sessionRepository = new SessionRepository(sessionFilename.Replace("boul", "json"));
                var sessionRepository = new SessionRepository(sessionFilename);
                var session = sessionRepository.GetAll();
                
                var oilWellsVMs = Mapper.Map<IEnumerable<OilWell>, IEnumerable<OilWellVM>>(session.OilWells);
                var mapVMs = Mapper.Map<IEnumerable<Map>, IEnumerable<MapVM>>(session.Maps);

                foreach (var mapVM in mapVMs)
                {
                    var bitmap = ConvertToBitmap.GetMapBitmap(mapVM.PixelWidth, mapVM.PixelHeight, mapVM.Z, 9999900);
                    var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    bitmapSource.Freeze();
                    mapVM.BitmapSource = bitmapSource;
                }
               
                var debitGainVm = Mapper.Map<DebitGain, DebitGainVM>(session.DebitGain);
                
                return ResultResponse<IEnumerable<object>>.GetSuccessResponse(new List<object>{oilWellsVMs, mapVMs, debitGainVm});
            }
            catch (Exception e)
            {
                return ResultResponse<IEnumerable<object>>.GetErrorResponse(@$"Ошибка открытия сессии{Environment.NewLine}
                                                    {e.Message}{Environment.NewLine}
                                                    {e.StackTrace}");
            }
        }
    }
}