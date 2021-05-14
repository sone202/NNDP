using AsyncAwaitBestPractices.MVVM;
using BubbleChartOilWells.BusinessLogic.Services;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using Microsoft.Win32;
using System;
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

namespace BubbleChartOilWells.ViewModels
{
    public class MainVM : BaseVM
    {
        private readonly string jsonFileName;
        private readonly OilWellService oilWellService;
        private readonly IrapMapService irapMapService;
        private readonly UserMapService userMapService;
        private readonly DebitGainService debitGainService;
        private readonly NeuralNetService neuralNetService;
        private readonly ExportMapValuesService exportMapValuesService;
        private readonly SaveMapService saveMapService;

        private bool isReady;
        private bool isExcelImportEnabled;
        private bool isMapImportEnabled;
        private bool isProdMapChecked;
        private bool isProdSumMapChecked;
        private bool isBubblesChecked;
        private bool isNeuralNetTrainResultsVisibile;
        private bool isNeuralNetPredictionResultsVisible;
        private bool isUsingKH;

        private MapVM selectedMap;
        // TODO: refactor
        private List<string> selectedOilWell;
        private DebitGainVM debitGainVM;
        private NeuralNetVM neuralNetVM;

        private AsyncCommand importOilWellsAsyncCommand;
        private AsyncCommand getOilWellsAsyncCommand;
        private AsyncCommand importIrapMapAsyncCommand;
        private AsyncCommand importUserMapAsyncCommand;
        private AsyncCommand importUserMapContourAsyncCommand;
        private AsyncCommand<string> drawDebitGainMapAsyncCommand;
        private AsyncCommand calculateDupuisAsyncCommand;
        private AsyncCommand importTrainDataAsyncCommand;
        private AsyncCommand importInitialDataAsyncCommand;
        private AsyncCommand trainNeuralNetAsyncCommand;
        private AsyncCommand predictNeuralNetAsyncCommand;
        private AsyncCommand exportMapValuesAsyncCommand;
        private AsyncCommand saveMapAsyncCommand;


        public IEnumerable<OilWellVM> OilWellVMs { get; set; }
        public ObservableCollection<MapVM> MapVMs { get; set; }
        public SeriesCollection OfpSeriesCollection { get; set; }
        public SeriesCollection CrossPlotTrainSeriesCollection { get; set; }
        public SeriesCollection CrossPlotTestSeriesCollection { get; set; }

        public bool IsReady
        {
            get => isReady;
            set
            {
                isReady = value;
                OnPropertyChanged(nameof(IsReady));
            }
        }

        public bool IsExcelImportEnabled
        {
            get => isExcelImportEnabled;
            set
            {
                isExcelImportEnabled = value;
                IsReady = value;
                OnPropertyChanged(nameof(IsExcelImportEnabled));
            }
        }
        public bool IsMapImportEnabled
        {
            get => isMapImportEnabled;
            set
            {
                isMapImportEnabled = value;
                IsReady = value;
                OnPropertyChanged(nameof(IsMapImportEnabled));
            }
        }
        public bool IsProdMapChecked
        {
            get => isProdMapChecked;
            set
            {
                if (value == true && isProdMapChecked == false)
                {
                    if (isBubblesChecked == true)
                    {
                        DrawProdMap();
                    }
                    else
                    {
                        DrawOilWellMap();
                    }
                }
                isProdMapChecked = value;
                OnPropertyChanged(nameof(OilWellVMs));
                OnPropertyChanged(nameof(IsProdMapChecked));
            }
        }
        public bool IsProdSumMapChecked
        {
            get => isProdSumMapChecked;
            set
            {
                if (value == true && isProdSumMapChecked == false)
                {
                    if (isBubblesChecked == true)
                    {
                        DrawProdSumMap();
                    }
                    else
                    {
                        DrawOilWellMap();
                    }
                }
                isProdSumMapChecked = value;
                OnPropertyChanged(nameof(OilWellVMs));
                OnPropertyChanged(nameof(IsProdSumMapChecked));
            }
        }
        public bool IsBubblesChecked
        {
            get => isBubblesChecked;
            set
            {
                if (OilWellVMs != null)
                {
                    if (value == true)
                    {
                        if (isProdMapChecked == true)
                        {
                            DrawProdMap();
                        }
                        if (isProdSumMapChecked == true)
                        {
                            DrawProdSumMap();
                        }
                    }
                    else
                    {
                        DrawOilWellMap();
                    }
                }
                isBubblesChecked = value;
                OnPropertyChanged(nameof(OilWellVMs));
                OnPropertyChanged(nameof(IsBubblesChecked));
            }
        }
        public bool IsNeuralNetTrainResultsVisible
        {
            get => isNeuralNetTrainResultsVisibile;
            set
            {
                isNeuralNetTrainResultsVisibile = value;
                OnPropertyChanged(nameof(IsNeuralNetTrainResultsVisible));
            }
        }
        public bool IsNeuralNetPredictionResultsVisible
        {
            get => isNeuralNetPredictionResultsVisible;
            set
            {
                isNeuralNetPredictionResultsVisible = value;
                OnPropertyChanged(nameof(IsNeuralNetPredictionResultsVisible));
            }
        }
        public bool IsUsingKH
        {
            get => isUsingKH;
            set
            {
                isUsingKH = value;
                OnPropertyChanged(nameof(IsUsingKH));
            }
        }
        public MapVM SelectedMap
        {
            get => selectedMap;
            set
            {
                selectedMap = value;
                OnPropertyChanged(nameof(SelectedMap));
            }
        }
        // TODO: refactor
        public List<string> SelectedOilWellPropertiesValues
        {
            get => selectedOilWell;
            set
            {
                selectedOilWell = value;
                OnPropertyChanged(nameof(SelectedOilWellPropertiesValues));
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
        public NeuralNetVM NeuralNetVM
        {
            get => neuralNetVM;
            set
            {
                neuralNetVM = value;
                OnPropertyChanged(nameof(NeuralNetVM));
            }
        }


        public AsyncCommand ImportOilWellsAsyncCommand => importOilWellsAsyncCommand ?? (importOilWellsAsyncCommand = new AsyncCommand(ImportOilWellsAsync));
        public AsyncCommand GetOilWellsAsyncCommand => getOilWellsAsyncCommand ?? (getOilWellsAsyncCommand = new AsyncCommand(GetOilWellsAsync));
        public AsyncCommand ImportIrapMapAsyncCommand => importIrapMapAsyncCommand ?? (importIrapMapAsyncCommand = new AsyncCommand(ImportIrapMapAsync));
        public AsyncCommand ImportUserMapAsyncCommand => importUserMapAsyncCommand ?? (importUserMapAsyncCommand = new AsyncCommand(ImportUserMapAsync));
        public AsyncCommand ImportUserMapContourAsyncCommand => importUserMapContourAsyncCommand ?? (importUserMapContourAsyncCommand = new AsyncCommand(ImportUserMapContourAsync));
        public AsyncCommand<string> DrawDebitGainMapAsyncCommand => drawDebitGainMapAsyncCommand ?? (drawDebitGainMapAsyncCommand = new AsyncCommand<string>(mapType => DrawDebitGainMapAsync(mapType)));
        public AsyncCommand CalculateDupuisAsyncCommand => calculateDupuisAsyncCommand ?? (calculateDupuisAsyncCommand = new AsyncCommand(CalculateDupuisAsync));
        public AsyncCommand ImportTrainDataAsyncCommand => importTrainDataAsyncCommand ?? (importTrainDataAsyncCommand = new AsyncCommand(ImportTrainDataAsync));
        public AsyncCommand ImportInitialDataAsyncCommand => importInitialDataAsyncCommand ?? (importInitialDataAsyncCommand = new AsyncCommand(ImportInitialDataAsync));
        public AsyncCommand TrainNeuralNetAsyncCommand => trainNeuralNetAsyncCommand ?? (trainNeuralNetAsyncCommand = new AsyncCommand(TrainNeuralNetAsync));
        public AsyncCommand PredictNeuralNetAsyncCommand => predictNeuralNetAsyncCommand ?? (predictNeuralNetAsyncCommand = new AsyncCommand(PredictWithNeuralNetAsync));
        public AsyncCommand ExportMapValuesAsyncCommand => exportMapValuesAsyncCommand ?? (exportMapValuesAsyncCommand = new AsyncCommand(ExportMapToExcelAsync));
        public AsyncCommand SaveMapAsyncCommand => saveMapAsyncCommand ?? (saveMapAsyncCommand = new AsyncCommand(SaveMapAsync));


        public MainVM()
        {
            jsonFileName = Path.GetFullPath("OilWells.json");
            oilWellService = new OilWellService(jsonFileName);
            irapMapService = new IrapMapService();
            userMapService = new UserMapService(jsonFileName);
            debitGainService = new DebitGainService();
            neuralNetService = new NeuralNetService();
            exportMapValuesService = new ExportMapValuesService();
            saveMapService = new SaveMapService();

            isReady = true;
            IsExcelImportEnabled = true;
            IsMapImportEnabled = true;
            IsProdMapChecked = false;
            IsProdSumMapChecked = false;
            IsBubblesChecked = false;
            IsNeuralNetTrainResultsVisible = false;

            MapVMs = new ObservableCollection<MapVM>();

            DebitGainVM = new DebitGainVM();
            NeuralNetVM = new NeuralNetVM();
        }

        /// <summary>
        /// Import excel file, save imported data to JSON, add oil wells to the grid
        /// </summary>
        /// <returns></returns>
        private async Task ImportOilWellsAsync()
        {
            IsExcelImportEnabled = false;
            IsProdMapChecked = false;

            await Task.Run(() =>
            {
                var openFileForm = new OpenFileDialog
                {
                    InitialDirectory = "c:\\documents",
                    Filter = "Excel all files (*.xlsx, *.xlsm, *.xltx, *.xltm)|*.xlsx; *.xlsm; *.xltx; *.xltm",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                string fileName = openFileForm.ShowDialog() == true ? openFileForm.FileName : string.Empty;

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }

                var result = oilWellService.ImportOilWells(fileName);
                if (result.IsSuccess)
                {
                    OilWellVMs = result.Data;
                    Application.Current.Dispatcher.Invoke(new Action(() => { IsProdMapChecked = true; }));
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage);
                }
            });

            IsExcelImportEnabled = true;
        }

        /// <summary>
        /// Add oil wells to the grid from JSON file
        /// </summary>
        /// <returns></returns>
        private async Task GetOilWellsAsync()
        {
            IsExcelImportEnabled = false;
            IsProdMapChecked = false;

            await Task.Run(() =>
            {
                var isJsonExists = File.Exists(jsonFileName);
                if (!isJsonExists)
                {
                    MessageBox.Show("Сохраненные данные отсутствуют. Необходимо импортировать excel файл.");
                    return;
                }

                var result = oilWellService.GetOilWells();
                if (result.IsSuccess)
                {
                    OilWellVMs = result.Data;
                    Application.Current.Dispatcher.Invoke(new Action(() => { IsProdMapChecked = true; }));
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage);
                }
            });

            IsExcelImportEnabled = true;
        }

        /// <summary>
        /// Import irap map
        /// </summary>
        /// <returns></returns>
        private async Task ImportIrapMapAsync()
        {
            IsMapImportEnabled = false;

            await Task.Run(() =>
            {
                var openFileForm = new OpenFileDialog
                {
                    InitialDirectory = "c:\\documents",
                    Filter = "Irap (*.irap)|*.irap",
                    FilterIndex = 1,
                    Multiselect = true,
                    RestoreDirectory = true
                };


                var fileNames = openFileForm.ShowDialog() == true ? openFileForm.FileNames : null;


                foreach (var fileName in fileNames)
                {
                    var result = irapMapService.ImportIrapMap(fileName);
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
                }
            });

            IsMapImportEnabled = true;
        }

        /// <summary>
        /// Import user map
        /// </summary>
        /// <returns></returns>
        private async Task ImportUserMapAsync()
        {
            IsMapImportEnabled = false;

            await Task.Run(() =>
            {
                var isJsonExists = File.Exists(jsonFileName);
                if (!isJsonExists)
                {
                    MessageBox.Show("Сохраненные данные отсутствуют. Необходимо импортировать excel файл.");
                    return;
                }

                var openFileForm = new OpenFileDialog
                {
                    InitialDirectory = "c:\\documents",
                    Filter = "Карта (*.txt)|*.txt",
                    FilterIndex = 1,
                    Multiselect = true,
                    RestoreDirectory = true
                };

                var fileNames = openFileForm.ShowDialog() == true ? openFileForm.FileNames : null;
                if (fileNames == null)
                {
                    return;
                }

                foreach (var fileName in fileNames)
                {
                    var result = userMapService.ImportUserMap(fileName);
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
                }
            });

            IsMapImportEnabled = true;
        }

        /// <summary>
        /// Import user map contour
        /// </summary>
        /// <returns></returns>
        private async Task ImportUserMapContourAsync()
        {
            IsMapImportEnabled = false;

            await Task.Run(() =>
            {
                var isJsonExists = File.Exists(jsonFileName);
                if (!isJsonExists)
                {
                    MessageBox.Show("Сохраненные данные отсутствуют. Необходимо импортировать excel файл.");
                    return;
                }

                var openFileForm = new OpenFileDialog
                {
                    InitialDirectory = "c:\\documents",
                    Filter = "Контур карты (Irap) (*.irap)|*.irap",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                string fileName = openFileForm.ShowDialog() == true ? openFileForm.FileName : string.Empty;

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }

                var result = userMapService.ImportUserMapContour(fileName);
                if (result.IsSuccess)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        // TODO:
                    }));
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage);
                }
            });

            IsMapImportEnabled = true;
        }

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
                var Krw = new List<ObservablePoint>(); // офп воды
                var Kro = new List<ObservablePoint>(); // офп нефти
                var waterSaturation = new List<double>(); // водонасыщенность

                for (int i = 0; i < DebitGainVM.Ofp.Rows.Count; i++)
                {
                    waterSaturation.Add((double)DebitGainVM.Ofp.Rows[i][0]);

                    Krw.Add(new ObservablePoint((double)DebitGainVM.Ofp.Rows[i][0],
                        (double)DebitGainVM.Ofp.Rows[i][2]));

                    Kro.Add(new ObservablePoint((double)DebitGainVM.Ofp.Rows[i][0],
                       (double)DebitGainVM.Ofp.Rows[i][1]));
                }

                OfpSeriesCollection = new SeriesCollection();

                OfpSeriesCollection.Add(
                    new ScatterSeries
                    {
                        Title = "Krw",
                        Values = new ChartValues<ObservablePoint>(Krw),
                        Fill = Brushes.Blue
                    });

                OfpSeriesCollection.Add(
                    new ScatterSeries
                    {
                        Title = "Kro",
                        Values = new ChartValues<ObservablePoint>(Kro),
                        Fill = Brushes.Brown
                    });

                // calculating and plotting ofp curves
                var KrwCurve = new List<ObservablePoint>();
                var KroCurve = new List<ObservablePoint>();

                debitGainVM.Scw = Krw.Where(x => x.Y == Krw.Min(z => z.Y)).Last().X;
                debitGainVM.Sor = 1 - Kro.Where(x => x.Y == Kro.Min(z => z.Y)).First().X;

                debitGainVM.Krwor = Krw.Where(x => x.X == (1 - debitGainVM.Sor)).Last().Y;
                debitGainVM.Krocw = Kro.Where(x => x.X == debitGainVM.Scw).First().Y;

                debitGainVM.Nw = GetNw(Krw, debitGainVM);
                debitGainVM.No = GetNo(Kro, debitGainVM);

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
                        Stroke = (Brush)(new BrushConverter()).ConvertFromString("#786FA6"),
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
        private double GetNw(List<ObservablePoint> tableValues, DebitGainVM debitGainVM, double nw = 0, bool flag = true, double minErr = 99999, double step = 0.1)
        {
            var Krwor = debitGainVM.Krwor;
            var Scw = debitGainVM.Scw;
            var Sor = debitGainVM.Sor;
            var threshold = 0.005;
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
        private double GetNo(List<ObservablePoint> tableValues, DebitGainVM debitGainVM, double no = 0, bool flag = true, double minErr = 99999, double step = 0.1)
        {
            var Krocw = debitGainVM.Krocw;
            var Scw = debitGainVM.Scw;
            var Sor = debitGainVM.Sor;
            var threshold = 0.005;
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

        #region NeuralNet
        /// <summary>
        /// Import file for neuralnet training
        /// </summary>
        /// <returns></returns>
        private async Task ImportTrainDataAsync()
        {
            IsReady = false;
            await Task.Run(() =>
            {
                var openFileForm = new OpenFileDialog
                {
                    InitialDirectory = "c:\\documents",
                    Filter = "csv (*.csv)|*.csv",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                string fileName = openFileForm.ShowDialog() == true ? openFileForm.FileName : string.Empty;

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    IsReady = true;
                    return;
                }

                NeuralNetVM.TrainingDataFileName = fileName;
            });
            IsReady = true;

            OnPropertyChanged(nameof(NeuralNetVM));
        }

        /// <summary>
        /// Import file for prediction
        /// </summary>
        /// <returns></returns>
        private async Task ImportInitialDataAsync()
        {
            IsReady = false;
            await Task.Run(() =>
            {
                var openFileForm = new OpenFileDialog
                {
                    InitialDirectory = "c:\\documents",
                    Filter = "csv (*.csv)|*.csv",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                string fileName = openFileForm.ShowDialog() == true ? openFileForm.FileName : string.Empty;

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    IsReady = true;
                    return;
                }

                NeuralNetVM.PredictDataFileName = fileName;
            });

            OnPropertyChanged(nameof(NeuralNetVM));
            IsReady = true;
        }

        /// <summary>
        /// Train Neuralnet
        /// </summary>
        /// <returns></returns>
        private async Task TrainNeuralNetAsync()
        {
            IsReady = false;
            IsNeuralNetTrainResultsVisible = false;

            if (string.IsNullOrEmpty(NeuralNetVM.TrainingDataFileName))
            {
                MessageBox.Show("Выберите файл для обучения нейросети.");
                IsReady = true;
                return;
            }
            await Task.Run(() =>
            {
                var result = neuralNetService.TrainNeuralNetwork(NeuralNetVM);
                if (result.IsSuccess)
                {
                    NeuralNetVM = result.Data;
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage);
                    IsReady = true;
                    return;
                }
            });


            try
            {
                #region cross-plots
                CrossPlotTrainSeriesCollection = new SeriesCollection();
                var crossPlotTrainPoints = new List<ObservablePoint>();
                for (int i = 0; i < NeuralNetVM.TrainActual.Count; i++)
                {
                    crossPlotTrainPoints.Add(new ObservablePoint(NeuralNetVM.TrainPredicted[i], NeuralNetVM.TrainActual[i]));
                }
                CrossPlotTrainSeriesCollection.Add(
                       new ScatterSeries
                       {
                           Title = "Кросс-плот обучения",
                           Values = new ChartValues<ObservablePoint>(crossPlotTrainPoints),
                           Fill = (Brush)(new BrushConverter()).ConvertFromString("#786FA6"),
                           MaxPointShapeDiameter = 4
                       });

                CrossPlotTestSeriesCollection = new SeriesCollection();
                var crossPlotTestPoints = new List<ObservablePoint>();
                for (int i = 0; i < NeuralNetVM.TestActual.Count; i++)
                {
                    crossPlotTestPoints.Add(new ObservablePoint(NeuralNetVM.TestPredicted[i], NeuralNetVM.TestActual[i]));
                }
                CrossPlotTestSeriesCollection.Add(
                       new ScatterSeries
                       {
                           Title = "Кросс-плот тестирования",
                           Values = new ChartValues<ObservablePoint>(crossPlotTestPoints),
                           Fill = (Brush)(new BrushConverter()).ConvertFromString("#E15F41"),
                           MaxPointShapeDiameter = 4
                       });

                OnPropertyChanged(nameof(CrossPlotTrainSeriesCollection));
                OnPropertyChanged(nameof(CrossPlotTestSeriesCollection));
                OnPropertyChanged(nameof(NeuralNetVM));
                IsNeuralNetTrainResultsVisible = true;
                #endregion
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Ошибка при построении кросс-плотов{Environment.NewLine}
                                {e.Message}");
            }

            IsReady = true;
        }

        /// <summary>
        /// Predict values with neuralnet
        /// </summary>
        /// <returns></returns>
        private async Task PredictWithNeuralNetAsync()
        {
            IsReady = false;
            IsNeuralNetPredictionResultsVisible = false;

            if (string.IsNullOrEmpty(NeuralNetVM.PredictDataFileName))
            {
                MessageBox.Show("Выберите файл для прогнозирования.");
                IsReady = true;
                return;
            }

            await Task.Run(() =>
            {
                var neuralNetResult = neuralNetService.PredictLiquidDebitGain(NeuralNetVM);
                if (neuralNetResult.IsSuccess)
                {
                    NeuralNetVM = neuralNetResult.Data;
                    IsNeuralNetPredictionResultsVisible = true;
                    OnPropertyChanged(nameof(NeuralNetVM));
                }
                else
                {
                    MessageBox.Show(neuralNetResult.ErrorMessage);
                    IsReady = true;
                    return;
                }
                var mapResult = userMapService.DrawPredictedMap(neuralNetVM.PredictionFullResults);
                if (mapResult.IsSuccess)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (SelectedMap != null)
                        {
                            SelectedMap.IsSelected = false;
                        }
                        SelectedMap = mapResult.Data;

                        MapVMs.Add(SelectedMap);
                    }));
                }
                else
                {
                    MessageBox.Show(mapResult.ErrorMessage);
                }

            });

            IsReady = true;
        }
        #endregion

        /// <summary>
        /// Export map values
        /// </summary>
        /// <returns></returns>
        private async Task ExportMapToExcelAsync()
        {
            IsReady = false;
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\Users";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                await Task.Run(() =>
                {
                    var result = exportMapValuesService.ExportMapValuesToExcel(MapVMs.Where(x => x.IsExporting == true), OilWellVMs, dialog.FileName + "\\mapExport.xlsx");
                    if (!result.IsSuccess)
                    {
                        MessageBox.Show(result.ErrorMessage);
                        IsReady = true;
                        return;
                    }
                });
            }

            MapVMs.Select(x => x.IsExporting = false);
            IsReady = true;
        }

        /// <summary>
        /// Saving map as irap
        /// </summary>
        /// <returns></returns>
        private async Task SaveMapAsync()
        {
            IsReady = false;
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "Documents";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                await Task.Run(() =>
                {
                    var result = saveMapService.SaveAsIrapService(MapVMs.Where(x => x.IsExporting == true), dialog.FileName);
                    if (!result.IsSuccess)
                    {
                        MessageBox.Show(result.ErrorMessage);
                        IsReady = true;
                        return;
                    }
                });
            }

            MapVMs.Select(x => x.IsExporting = false);
            IsReady = true;
        }

        private void DrawProdMap()
        {
            var maxLiquidDebit = OilWellVMs.Max(x => x.Objectives.Sum(y => y.MonthlyObjectiveProduction.LiquidDebit));

            var multiplierCoefficient = 1;
            if (maxLiquidDebit > 1000)
                multiplierCoefficient = 10;
            if (maxLiquidDebit > 10000)
                multiplierCoefficient = 100;

            foreach (var oilWellVM in OilWellVMs)
            {
                oilWellVM.CreateOilWellProdView(multiplierCoefficient);
            }
        }

        private void DrawProdSumMap()
        {
            var maxLiquidProdSum = OilWellVMs.Max(x => x.Objectives.Sum(y => y.MonthlyObjectiveProduction.LiquidProdSum));

            var multiplierCoefficient = 1;
            if (maxLiquidProdSum > 1000)
                multiplierCoefficient = 10;
            if (maxLiquidProdSum > 10000)
                multiplierCoefficient = 100;

            foreach (var oilWellVM in OilWellVMs)
            {
                oilWellVM.CreateOilWellProdSumView(multiplierCoefficient);
            }
        }

        private void DrawOilWellMap()
        {
            foreach (var oilWellVM in OilWellVMs)
            {
                oilWellVM.CreateOilWellView();
            }
        }
    }
}
