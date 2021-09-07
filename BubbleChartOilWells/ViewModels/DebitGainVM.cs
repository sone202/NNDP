using AsyncAwaitBestPractices.MVVM;
using BubbleChartOilWells.BusinessLogic.Services;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;

namespace BubbleChartOilWells.ViewModels
{
    public partial class MainVM
    {
        // services
        private readonly DebitGainService debitGainService;

        // calculation mode
        private bool isUsingKH;

        // view models
        private DebitGainVM debitGainVM;

        // commands
        private AsyncCommand<string> drawDebitGainMapAsyncCommand;
        private AsyncCommand calculateDupuisAsyncCommand;

        public bool IsUsingKH
        {
            get => isUsingKH;
            set
            {
                isUsingKH = value;
                OnPropertyChanged(nameof(IsUsingKH));
            }
        }

        public DebitGainVM DebitGainVM
        {
            get => debitGainVM;
            set
            {
                debitGainVM = value;
                OnPropertyChanged(nameof(DebitGainVM));
            }
        }

        // commands
        public AsyncCommand<string> DrawDebitGainMapAsyncCommand => drawDebitGainMapAsyncCommand ??
                                                                    (drawDebitGainMapAsyncCommand =
                                                                        new AsyncCommand<string>(mapType =>
                                                                            DrawDebitGainMapAsync(mapType)));

        public AsyncCommand CalculateDupuisAsyncCommand => calculateDupuisAsyncCommand ??
                                                           (calculateDupuisAsyncCommand =
                                                               new AsyncCommand(CalculateDupuisAsync));

        // ofp plot
        public SeriesCollection OfpSeriesCollection { get; set; }

        // methods

        /// <summary>
        /// Calculates Krw and Kro with Dupui formula
        /// </summary>
        /// <returns></returns>
        private async Task CalculateDupuisAsync()
        {
            IsReady = false;

            try
            {
                if (DebitGainVM.Sw == null)
                {
                    MessageBox.Show("Выберите карту Sw");
                    IsReady = true;

                    return;
                }

                // getting data from datagrid 
                var krw = new List<ObservablePoint>(); // офп воды
                var kro = new List<ObservablePoint>(); // офп нефти
                var waterSaturation = new List<double>(); // водонасыщенность

                for (int i = 0; i < DebitGainVM.Ofp.Rows.Count; i++)
                {
                    waterSaturation.Add((double) DebitGainVM.Ofp.Rows[i][0]);

                    krw.Add(new ObservablePoint((double) DebitGainVM.Ofp.Rows[i][0],
                        (double) DebitGainVM.Ofp.Rows[i][2]));

                    kro.Add(new ObservablePoint((double) DebitGainVM.Ofp.Rows[i][0],
                        (double) DebitGainVM.Ofp.Rows[i][1]));
                }

                OfpSeriesCollection = new SeriesCollection();

                OfpSeriesCollection.Add(
                    new ScatterSeries
                    {
                        Title = "Krw",
                        Values = new ChartValues<ObservablePoint>(krw),
                        Fill = Brushes.Blue
                    });

                OfpSeriesCollection.Add(
                    new ScatterSeries
                    {
                        Title = "Kro",
                        Values = new ChartValues<ObservablePoint>(kro),
                        Fill = Brushes.Brown
                    });

                // calculating and plotting ofp curves
                var KrwCurve = new List<ObservablePoint>();
                var KroCurve = new List<ObservablePoint>();

                debitGainVM.Scw = krw.Where(x => x.Y == krw.Min(z => z.Y)).Last().X;
                debitGainVM.Sor = 1 - kro.Where(x => x.Y == kro.Min(z => z.Y)).First().X;

                debitGainVM.Krwor = krw.Where(x => Convert.ToDecimal(x.X) == Convert.ToDecimal(1 - debitGainVM.Sor)).Last().Y;
                debitGainVM.Krocw = kro.Where(x => Convert.ToDecimal(x.X) == Convert.ToDecimal(debitGainVM.Scw)).First().Y;

                debitGainVM.Nw = GetNw(krw, debitGainVM);
                debitGainVM.No = GetNo(kro, debitGainVM);

                for (double i = debitGainVM.Scw; i <= 1 - debitGainVM.Sor; i += 0.01)
                {
                    KrwCurve.Add(new ObservablePoint(i, debitGainService.GetKrw(debitGainVM, i).Data));
                    KroCurve.Add(new ObservablePoint(i, debitGainService.GetKro(debitGainVM, i).Data));
                }

                OfpSeriesCollection.Add(
                    new LineSeries
                    {
                        Title = "Krw",
                        Values = new ChartValues<ObservablePoint>(KrwCurve),
                        Stroke = Brushes.Blue,
                        Fill = Brushes.Transparent,
                        PointGeometrySize = 0
                    });

                OfpSeriesCollection.Add(
                    new LineSeries
                    {
                        Title = "Kro",
                        //Stroke = (Brush)(new BrushConverter()).ConvertFromString("#E15F41"),
                        Stroke = Brushes.Brown,
                        Fill = Brushes.Transparent,
                        PointGeometrySize = 0,
                        Values = new ChartValues<ObservablePoint>(KroCurve)
                    });

                // plotting W (обводненность)
                var wCurve = new List<ObservablePoint>();
                await Task.Run(() =>
                {
                    var result = debitGainService.GetW(DebitGainVM, debitGainVM.Scw, debitGainVM.Sor);
                    if (result.IsSuccess)
                    {
                        var w = result.Data;
                        var index = 0;
                        for (double i = debitGainVM.Scw; i <= 1 - debitGainVM.Sor; i += 0.01)
                        {
                            wCurve.Add(new ObservablePoint(i, w[index++]));
                        }
                    }
                    else
                    {
                        MessageBox.Show(result.ErrorMessage);
                        return;
                    }
                });

                OfpSeriesCollection.Add(
                    new LineSeries
                    {
                        Title = "W",
                        Stroke = (Brush) (new BrushConverter()).ConvertFromString("#786FA6"),
                        Fill = Brushes.Transparent,
                        PointGeometrySize = 1,
                        Values = new ChartValues<ObservablePoint>(wCurve)
                    });

                OnPropertyChanged(nameof(OfpSeriesCollection));

                // adding info to the object
                for (int i = 0; i < DebitGainVM.Sw.Z.Count; i++)
                {
                    var krwResult = debitGainService.GetKrw(debitGainVM, DebitGainVM.Sw.Z[i]);
                    if (krwResult.IsSuccess)
                    {
                        DebitGainVM.KwInEachMapCell.Add(krwResult.Data);
                    }
                    else
                    {
                        MessageBox.Show(krwResult.ErrorMessage);
                        return;
                    }

                    var kroResult = debitGainService.GetKro(debitGainVM, DebitGainVM.Sw.Z[i]);
                    if (kroResult.IsSuccess)
                    {
                        DebitGainVM.KoInEachMapCell.Add(kroResult.Data);
                    }
                    else
                    {
                        MessageBox.Show(kroResult.ErrorMessage);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Ошибка построения кривых Krw и Kro{Environment.NewLine}
                                {e.Message}");
            }

            IsReady = true;
        }

        // TODO: refactor
        private double GetNw(List<ObservablePoint> tableValues, DebitGainVM debitGainVM, double nw = 0,
            bool flag = true, double minErr = 99999, double step = 0.1)
        {
            var Krwor = debitGainVM.Krwor;
            var Scw = debitGainVM.Scw;
            var Sor = debitGainVM.Sor;
            var threshold = 0.01;
            var tableValuesCount = tableValues.Count;

            if (flag == true)
            {
                for (double i = nw; i < 100; i += step)
                {
                    // square error
                    double sum = 0;
                    foreach (var point in tableValues)
                    {
                        sum += Math.Pow(point.Y - Krwor * Math.Pow((point.X - Scw) / (1 - Scw - Sor), i), 2);
                    }

                    var err = sum / tableValuesCount;

                    if (minErr >= err)
                    {
                        minErr = err;
                        nw = i;
                    }
                    else if (minErr > threshold)
                    {
                        GetNw(tableValues, debitGainVM, nw, false, minErr, step / 2);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                for (double i = nw; i > 0; i -= step)
                {
                    // square error
                    double sum = 0;
                    foreach (var point in tableValues)
                    {
                        sum += Math.Pow(point.Y - Krwor * Math.Pow((point.X - Scw) / (1 - Scw - Sor), i), 2);
                    }

                    var err = sum / tableValuesCount;

                    if (minErr >= err)
                    {
                        minErr = err;
                        nw = i;
                    }
                    else if (minErr > threshold)
                    {
                        GetNw(tableValues, debitGainVM, nw, true, minErr, step / 2);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return nw;
        }

        private double GetNo(List<ObservablePoint> tableValues, DebitGainVM debitGainVM, double no = 0,
            bool flag = true, double minErr = 99999, double step = 0.1)
        {
            var Krocw = debitGainVM.Krocw;
            var Scw = debitGainVM.Scw;
            var Sor = debitGainVM.Sor;
            var threshold = 0.01;
            var tableValuesCount = tableValues.Count;

            if (flag == true)
            {
                for (double i = no; i < 100; i += step)
                {
                    // square error
                    double sum = 0;
                    foreach (var point in tableValues)
                    {
                        sum += Math.Pow(point.Y - Krocw * Math.Pow((1 - point.X - Sor) / (1 - Scw - Sor), i), 2);
                    }

                    var err = sum / tableValuesCount;

                    if (minErr >= err)
                    {
                        minErr = err;
                        no = i;
                    }
                    else if (minErr > threshold)
                    {
                        GetNo(tableValues, debitGainVM, no, false, minErr, step / 2);
                    }
                    else
                    {
                        return no;
                    }
                }

                return no;
            }
            else
            {
                for (double i = no; i > 0; i -= step)
                {
                    // square error
                    double sum = 0;
                    foreach (var point in tableValues)
                    {
                        sum += Math.Pow(point.Y - Krocw * Math.Pow((1 - point.X - Sor) / (1 - Scw - Sor), i), 2);
                    }

                    var err = sum / tableValuesCount;

                    if (minErr >= err)
                    {
                        minErr = err;
                        no = i;
                    }
                    else if (minErr > threshold)
                    {
                        GetNo(tableValues, debitGainVM, no, true, minErr, step / 2);
                    }
                    else
                    {
                        return no;
                    }
                }

                return no;
            }
        }

        /// <summary>
        /// Draw liquid debit gain map
        /// </summary>
        /// <returns></returns>
        private async Task DrawDebitGainMapAsync(string mapType)
        {
            IsReady = false;

            if ((DebitGainVM.K == null || DebitGainVM.H == null)
                && (DebitGainVM.KH == null || DebitGainVM.Pr == null)
                || DebitGainVM.Sw == null)
            {
                MessageBox.Show("Выберите карты для рассчета");
                IsReady = true;

                return;
            }

            if (DebitGainVM.Nw == 0 || DebitGainVM.No == 0)
            {
                await CalculateDupuisAsync();
            }

            await Task.Run(() =>
            {
                var result = debitGainService.CreateDebitGainMap(DebitGainVM, mapType);
                if (result.IsSuccess)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (SelectedMap != null)
                        {
                            SelectedMap.IsSelected = false;
                        }

                        SelectedMap = result.Data;

                        MapVMs.Add(SelectedMap);
                        OnPropertyChanged(nameof(MapVMs));
                    }));
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage);
                }
            });

            IsReady = true;
        }
    }
}