using BubbleChartOilWells.BusinessLogic.Utils;
using BubbleChartOilWells.Contracts;
using BubbleChartOilWells.Contracts.Models.Dto;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using BubbleChartOilWells.DataAccess.Repositories;
using RDotNet;
using System;
using System.Collections.Generic;
using System.Data;
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
    public class UserMapService
    {
        private readonly OilWellRepository oilWellRepository;
        private REngine engine;

        public UserMapService(string jsonFileName)
        {
            oilWellRepository = new OilWellRepository(jsonFileName);

            REngine.SetEnvironmentVariables();
            engine = REngine.GetInstance();
            // TODO: logger
        }

        public ResultResponse<MapVM> DrawPredictedMap(DataTable predictedDataFrame)
        {
            try
            {
                var oilWellUserMapValueDtos = new List<OilWellUserMapValueDto>();

                foreach (DataRow row in predictedDataFrame.Rows)
                {
                    var oilWellUserMapValueDto = new OilWellUserMapValueDto
                    {
                        OilWellName = row[4].ToString(),
                        Value = double.Parse(row[row.ItemArray.Length - 1].ToString())
                    };

                    oilWellUserMapValueDtos.Add(oilWellUserMapValueDto);
                }
                var oilWells = oilWellRepository.GetAll();

                var xList = new List<double>();
                var yList = new List<double>();
                var zList = new List<double>();

                foreach (var oilWellUserMapValueDto in oilWellUserMapValueDtos)
                {
                    var oilWell = oilWells.First(x => x.Name == oilWellUserMapValueDto.OilWellName);

                    xList.Add(oilWell.X);
                    yList.Add(oilWell.Y);
                    zList.Add(oilWellUserMapValueDto.Value);
                }

                var predictedMapVM = GetKrigedMap(xList, yList, zList);
                predictedMapVM.Name = $@"PredictedMap~{DateTime.Now}";
                predictedMapVM.IsSelected = true;
                predictedMapVM.BitmapSource.Freeze();


                return ResultResponse<MapVM>.GetSuccessResponse(predictedMapVM);
            }
            catch (Exception e)
            {
                // TODO: write error to log
                return ResultResponse<MapVM>.GetErrorResponse(@$"Ошибка импортирования пользовательской карты.{Environment.NewLine}
                                                                {e.Message}{Environment.NewLine}
                                                                {e.StackTrace}");
            }
        }
        public ResultResponse<MapVM> ImportUserMap(string fileName)
        {
            try
            {
                var oilWellUserMapValueDtos = new List<OilWellUserMapValueDto>();

                using (var reader = new StreamReader(fileName))
                {
                    reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        var row = reader.ReadLine().Split(new char[] { ' ', '\t' });

                        var dotSeparatorValue = Convert.ToDouble(row[1], NumberFormatInfo.InvariantInfo);
                        var commaSeparatorValue = Convert.ToDouble(row[1], new NumberFormatInfo { NumberDecimalSeparator = "," });
                        var oilWellUserMapValueDto = new OilWellUserMapValueDto
                        {
                            OilWellName = row[0],
                            Value = (dotSeparatorValue > commaSeparatorValue) ? commaSeparatorValue : dotSeparatorValue
                        };
                        oilWellUserMapValueDtos.Add(oilWellUserMapValueDto);
                    }
                    reader.Close();
                }

                var oilWells = oilWellRepository.GetAll();

                var xList = new List<double>();
                var yList = new List<double>();
                var zList = new List<double>();

                foreach (var oilWellUserMapValueDto in oilWellUserMapValueDtos)
                {
                    var oilWell = oilWells.First(x => x.Name == oilWellUserMapValueDto.OilWellName);

                    xList.Add(oilWell.X);
                    yList.Add(oilWell.Y);
                    zList.Add(oilWellUserMapValueDto.Value);
                }

                var userMapVM = GetKrigedMap(xList, yList, zList);
                userMapVM.Name = Path.GetFileNameWithoutExtension(fileName);
                userMapVM.IsSelected = true;
                userMapVM.BitmapSource.Freeze();


                return ResultResponse<MapVM>.GetSuccessResponse(userMapVM);
            }
            catch (Exception e)
            {
                // TODO: write error to log
                return ResultResponse<MapVM>.GetErrorResponse(@$"Ошибка импортирования пользовательской карты.{Environment.NewLine}
                                                                {e.Message}{Environment.NewLine}
                                                                {e.StackTrace}");
            }
        }

        public ResultResponse<List<ContourFragmentVM>> ImportUserMapContour(string fileName)
        {
            try
            {
                var points3D = new List<Point3D>();

                using (var reader = new StreamReader(fileName))
                {
                    while (!reader.EndOfStream)
                    {
                        var row = reader.ReadLine().Split(' ');
                        var point3D = new Point3D
                        {
                            X = Convert.ToDouble(row[0], CultureInfo.InvariantCulture),
                            Y = Convert.ToDouble(row[1], CultureInfo.InvariantCulture),
                            Z = Convert.ToDouble(row[2], CultureInfo.InvariantCulture)
                        };
                        points3D.Add(point3D);
                    }
                }

                var separatorPoint = new Point3D(999, 999, 999);

                var contourFragmentVMs = new List<ContourFragmentVM>();
                contourFragmentVMs.Add(new ContourFragmentVM
                {
                    Points = new List<System.Windows.Point>()
                });

                foreach (var point3D in points3D)
                {
                    if (point3D == separatorPoint)
                    {
                        contourFragmentVMs.Add(new ContourFragmentVM
                        {
                            Points = new List<System.Windows.Point>()
                        });
                        continue;
                    }

                    var point = new System.Windows.Point(point3D.X, point3D.Y);
                    contourFragmentVMs.Last().Points.Add(point);
                }

                return ResultResponse<List<ContourFragmentVM>>.GetSuccessResponse(contourFragmentVMs);
            }
            catch (Exception e)
            {
                // TODO: write error to log
                return ResultResponse<List<ContourFragmentVM>>.GetErrorResponse("Ошибка импортирования контура пользовательской карты.");
            }
        }

        private MapVM GetKrigedMap(List<double> X, List<double> Y, List<double> Z)
        {
            var lags = 10;
            var pixels = 400;
            var model = "spherical";
            
            if(!engine.IsRunning)
            {
                engine = REngine.GetInstance();
            }

            engine.Evaluate(@"if (!require(""kriging"")) install.packages(""neuralnet"", dependencies = TRUE)");
            engine.Evaluate(@$"library(kriging)");

            engine.SetSymbol("x", engine.CreateNumericVector(X));
            engine.SetSymbol("y", engine.CreateNumericVector(Y));
            engine.SetSymbol("z", engine.CreateNumericVector(Z));
            engine.SetSymbol("lags", engine.CreateNumeric(lags));
            engine.SetSymbol("pixels", engine.CreateNumeric(pixels));
            engine.SetSymbol("model", engine.CreateCharacter(model));

            engine.Evaluate("kriged <- kriging(x, y, z, model, lags, pixels)");
            //engine.Evaluate("image(kriged, xlim = extendrange(x), ylim = extendrange(y))");
            engine.Evaluate("resultX <- kriged$map[[1]]");
            engine.Evaluate("resultY <- kriged$map[[2]]");
            engine.Evaluate("resultZ <- kriged$map[[3]]");

            var krigedZ = engine.GetSymbol("resultZ").AsNumeric().ToList();
            var krigedX = engine.GetSymbol("resultX").AsNumeric().ToList();
            var krigedY = engine.GetSymbol("resultY").AsNumeric().ToList();

            var pixelWidth = krigedY.Count(y => y == krigedY.First());
            var pixelHeight = krigedX.Count(x => x == krigedX.First());

            var xInversed = new List<double>();
            for (int i = 0; i < pixelHeight; i++)
            {
                for (int j = i; j < krigedZ.Count; j += pixelWidth)
                {
                    xInversed.Add(krigedX[j]);
                }
            }

            var yInversed = new List<double>();
            var zInversed = new List<double>();
            for (int i = 0; i < pixelWidth; i++)
            {
                for (int j = i; j < krigedZ.Count; j += pixelHeight)
                {
                    yInversed.Add(krigedY[j]);
                    zInversed.Add(krigedZ[j]);
                }
            }

            var mapWidth = xInversed.Max() - xInversed.Min();
            var mapHeight = yInversed.Max() - yInversed.Min();

            var userMapBitmap = ConvertToBitmap.GetMapBitmap(pixelWidth, pixelHeight, zInversed);

            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(userMapBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            var userMapVM = new MapVM
            {
                CellWidth = mapWidth / pixelWidth,
                CellHeight = mapHeight / pixelHeight,
                Width = mapWidth,
                Height = mapHeight,
                LeftBottomCoordinate = new System.Windows.Point(xInversed.Min(), yInversed.Min()),
                Z = zInversed,
                BitmapSource = bitmapSource
            };

            return userMapVM;
        }
    }
}
