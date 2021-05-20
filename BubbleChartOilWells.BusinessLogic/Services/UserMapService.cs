using BubbleChartOilWells.BusinessLogic.Utils;
using BubbleChartOilWells.Contracts;
using BubbleChartOilWells.Contracts.Models.Dto;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using BubbleChartOilWells.DataAccess.Repositories;
using RDotNet;
using System;
using System.Collections.Generic;
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

        public UserMapService(string jsonFileName)
        {
            oilWellRepository = new OilWellRepository(jsonFileName);

            // TODO: logger
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
                        var oilWellUserMapValueDto = new OilWellUserMapValueDto
                        {
                            OilWellName = row[0],
                            Value = Convert.ToDouble(row[1].Replace(',', '.'), CultureInfo.InvariantCulture)
                        };
                        oilWellUserMapValueDtos.Add(oilWellUserMapValueDto);
                    }
                }

                var oilWells = oilWellRepository.GetAll();

                var X = new List<double>();
                var Y = new List<double>();
                var Z = new List<double>();

                foreach (var oilWellUserMapValueDto in oilWellUserMapValueDtos)
                {
                    var oilWell = oilWells.First(x => x.Name == oilWellUserMapValueDto.OilWellName);

                    X.Add(oilWell.X);
                    Y.Add(oilWell.Y);
                    Z.Add(oilWellUserMapValueDto.Value);
                }

                var userMapVM = GetKrigedMap(X, Y, Z);
                userMapVM.Name = fileName.Replace("\\", "/").Split('/').Last().Split('.').First();
                userMapVM.IsSelected = true;
                userMapVM.BitmapSource.Freeze();


                return ResultResponse<MapVM>.GetSuccessResponse(userMapVM);
            }
            catch (Exception e)
            {
                // TODO: write error to log
                return ResultResponse<MapVM>.GetErrorResponse("Ошибка импортирования пользовательской карты.");
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

            REngine.SetEnvironmentVariables();
            var engine = REngine.GetInstance();

            engine.Evaluate("if(\"kriging\" %in% rownames(installed.packages()) == FALSE) {install.packages(\"kriging\")}");
            engine.Evaluate("library(kriging)");

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

            var xKriged = engine.GetSymbol("resultX").AsNumeric().ToList();
            var yKriged = engine.GetSymbol("resultY").AsNumeric().ToList();
            var zKriged = engine.GetSymbol("resultZ").AsNumeric().ToList();

            // TODO: delete debug stuff
            var debugValue = zKriged.IndexOf(0.7);

            var xDebugMax = xKriged.Max();
            var yDebugMax = yKriged.Max();
            
            var xDebugMin = xKriged.Min();
            var yDebugMin = yKriged.Min();

            engine.Dispose();

            var pictureHeight = xKriged.Count(x => x == xKriged.First());
            var pictureWidth = yKriged.Count(y => y == yKriged.First());

            var xInversed = new List<double>();
            for (int i = 0; i < pictureHeight; i++)
            {
                for (int j = i; j < zKriged.Count; j += pictureWidth)
                {
                    xInversed.Add(xKriged[j]);
                }
            }

            var yInversed = new List<double>();
            var zInversed = new List<double>();
            for (int i = 0; i < pictureWidth; i++)
            {
                for (int j = i; j < zKriged.Count; j += pictureHeight)
                {
                    yInversed.Add(yKriged[j]);
                    zInversed.Add(zKriged[j]);
                }
            }

            var userMapBitmap = ConvertToBitmap.GetBitmap(pictureWidth, pictureHeight, zInversed);
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(userMapBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bitmapSource.Freeze();

            var userMapVM = new MapVM
            {
                Width = xInversed.Max() - xInversed.Min(),
                Height = yInversed.Max() - yInversed.Min(),
                LeftBottomCoordinate = new Point(xInversed.Min(), yInversed.Min()),
                ZValues = zInversed,
                BitmapSource = bitmapSource
            };

            return userMapVM;
        }
    }
}
