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
using ExcelDataReader;
using System.Data;

namespace BubbleChartOilWells.ViewModels
{
    public class MainVM : BaseVM
    {
        private readonly string jsonFileName;
        private readonly OilWellService oilWellService;
        private readonly IrapMapService irapMapService;
        private readonly UserMapService userMapService;
        private readonly LiquidDebitGainService liquidDebitGainService;
        private readonly NeuralNetService neuralNetService;
        private readonly ExportMapValuesService exportMapValuesService;     

        private bool isReady;
        private bool isLoading;
        private bool isExcelImportEnabled;
        private bool isMapImportEnabled;
        private bool isProdMapEnabled;
        private bool isProdSumMapEnabled;
        private bool isBubblesEnabled;
        private bool isNeuralNetTrainResultsVisibile;
        private bool isNeuralNetPredictionResultsVisible;
        private bool isUsingKH;
        private MapVM selectedMap;
        // TODO: refactor
        private List<string> selectedOilWell;
        private LiquidDebitGainVM liquidDebitGainVM;
        private NeuralNetVM neuralNetVM;

        private AsyncCommand importOilWellsAsyncCommand;
        private AsyncCommand getOilWellsAsyncCommand;
        private AsyncCommand importIrapMapAsyncCommand;
        private AsyncCommand importUserMapAsyncCommand;
        private AsyncCommand importUserMapContourAsyncCommand;
        private AsyncCommand calculateWaterDebitGainAsyncCommand;
        private AsyncCommand calculateDupuisAsyncCommand;
        private AsyncCommand resetWaterDebitGainAsyncCommand;
        private AsyncCommand importTrainDataAsyncCommand;
        private AsyncCommand importInitialDataAsyncCommand;
        private AsyncCommand trainNeuralNetAsyncCommand;
        private AsyncCommand predictNeuralNetAsyncCommand;
        private AsyncCommand resetNeuralNetAsyncCommand;
        private AsyncCommand exportMapValuesAsyncCommand;

        public string fileNames;
        //запись первого и второго листа Excel
        List<object> dataList_1 = new List<object>();
        List<object> dataList_2 = new List<object>();

        IExcelDataReader edr;



        public IEnumerable<OilWellVM> OilWellVMs { get; set; }
        public ObservableCollection<MapVM> MapVMs { get; set; }
        public SeriesCollection SwSeriesCollection { get; set; }
        public List<string> SwLabels { get; set; }



        public bool IsReady
        {
            get => isReady;
            set
            {
                isReady = value;
                IsLoading = !isReady;
                OnPropertyChanged(nameof(IsReady));
            }
        }
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
        public bool IsExcelImportEnabled
        {
            get => isExcelImportEnabled;
            set
            {
                isExcelImportEnabled = value;
                IsReady = value;
                ImportOilWellsAsyncCommand.RaiseCanExecuteChanged();
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
                ImportIrapMapAsyncCommand.RaiseCanExecuteChanged();
                ImportUserMapAsyncCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsMapImportEnabled));
            }
        }
        public bool IsProdMapEnabled
        {
            get => isProdMapEnabled;
            set
            {
                if (value == true && isProdMapEnabled == false)
                {
                    if (isBubblesEnabled == true)
                    {
                        DrawProdMap();
                    }
                    else
                    {
                        DrawOilWellMap();
                    }
                }
                isProdMapEnabled = value;
                OnPropertyChanged(nameof(OilWellVMs));
                OnPropertyChanged(nameof(IsProdMapEnabled));
            }
        }
        public bool IsProdSumMapEnabled
        {
            get => isProdSumMapEnabled;
            set
            {
                if (value == true && isProdSumMapEnabled == false)
                {
                    if (isBubblesEnabled == true)
                    {
                        DrawProdSumMap();
                    }
                    else
                    {
                        DrawOilWellMap();
                    }
                }
                isProdSumMapEnabled = value;
                OnPropertyChanged(nameof(OilWellVMs));
                OnPropertyChanged(nameof(IsProdSumMapEnabled));
            }
        }
        public bool IsBubblesEnabled
        {
            get => isBubblesEnabled;
            set
            {
                if (OilWellVMs != null)
                {
                    if (value == true)
                    {
                        if (isProdMapEnabled == true)
                        {
                            DrawProdMap();
                        }
                        if (isProdSumMapEnabled == true)
                        {
                            DrawProdSumMap();
                        }
                    }
                    else
                    {
                        DrawOilWellMap();
                    }
                }
                isBubblesEnabled = value;
                OnPropertyChanged(nameof(OilWellVMs));
                OnPropertyChanged(nameof(IsBubblesEnabled));
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
        public List<string> SelectedOilWell
        {
            get => selectedOilWell;
            set
            {
                selectedOilWell = value;
                OnPropertyChanged(nameof(SelectedOilWell));
            }
        }
        public LiquidDebitGainVM LiquidDebitGainVM
        {
            get => liquidDebitGainVM;
            set
            {
                liquidDebitGainVM = value;
                OnPropertyChanged(nameof(LiquidDebitGainVM));
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
        public AsyncCommand CalculateWaterDebitGainAsyncCommand => calculateWaterDebitGainAsyncCommand ?? (calculateWaterDebitGainAsyncCommand = new AsyncCommand(CalculateLiquidDebitGainAsync));
        public AsyncCommand CalculateDupuisAsyncCommand => calculateDupuisAsyncCommand ?? (calculateDupuisAsyncCommand = new AsyncCommand(CalculateDupuisAsync));
        public AsyncCommand ResetWaterDebitGainAsyncCommand => resetWaterDebitGainAsyncCommand ?? (resetWaterDebitGainAsyncCommand = new AsyncCommand(ResetLiquidDebitGainAsync));
        public AsyncCommand ImportTrainDataAsyncCommand => importTrainDataAsyncCommand ?? (importTrainDataAsyncCommand = new AsyncCommand(ImportTrainDataAsync));
        public AsyncCommand ImportInitialDataAsyncCommand => importInitialDataAsyncCommand ?? (importInitialDataAsyncCommand = new AsyncCommand(ImportInitialDataAsync));
        public AsyncCommand TrainNeuralNetAsyncCommand => trainNeuralNetAsyncCommand ?? (trainNeuralNetAsyncCommand = new AsyncCommand(TrainNeuralNetAsync));
        public AsyncCommand PredictNeuralNetAsyncCommand => predictNeuralNetAsyncCommand ?? (predictNeuralNetAsyncCommand = new AsyncCommand(CalculateNeuralNetAsync));
        public AsyncCommand ResetNeuralNetAsyncCommand => resetNeuralNetAsyncCommand ?? (resetNeuralNetAsyncCommand = new AsyncCommand(ResetNeuralNetAsync));
        public AsyncCommand ExportMapValuesAsyncCommand => exportMapValuesAsyncCommand ?? (exportMapValuesAsyncCommand = new AsyncCommand(ExportMapValuesAsync));



        public MainVM()
        {
            jsonFileName = "OilWells.json";
            oilWellService = new OilWellService(jsonFileName);
            irapMapService = new IrapMapService();
            userMapService = new UserMapService(jsonFileName);
            liquidDebitGainService = new LiquidDebitGainService();
            neuralNetService = new NeuralNetService();
            exportMapValuesService = new ExportMapValuesService();


            IsExcelImportEnabled = true;
            IsMapImportEnabled = true;
            IsProdMapEnabled = false;
            IsProdSumMapEnabled = false;
            IsBubblesEnabled = false;
            IsNeuralNetTrainResultsVisible = false;

            MapVMs = new ObservableCollection<MapVM>();

            LiquidDebitGainVM = new LiquidDebitGainVM();
            NeuralNetVM = new NeuralNetVM();
        }

        /// <summary>
        /// Import excel file, save imported data to JSON, add oil wells to the grid
        /// </summary>
        /// <returns></returns>
        private async Task ImportOilWellsAsync()
        {
            IsExcelImportEnabled = false;

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
                    Application.Current.Dispatcher.Invoke(new Action(() => { IsProdMapEnabled = true; }));
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
        /// 
        private async Task GetOilWellsAsync()
        {
            IsExcelImportEnabled = false;

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
                    Application.Current.Dispatcher.Invoke(new Action(() => { IsProdMapEnabled = true; }));
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
                    RestoreDirectory = true
                };

                string fileName = openFileForm.ShowDialog() == true ? openFileForm.FileName : string.Empty;

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }

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
                    RestoreDirectory = true
                };

                string fileName = openFileForm.ShowDialog() == true ? openFileForm.FileName : string.Empty;

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }

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
                List<ObservablePoint> Krw = new List<ObservablePoint>();
                List<ObservablePoint> Kro = new List<ObservablePoint>();
                List<double> waterSaturation = new List<double>();

                for (int i = 0; i < LiquidDebitGainVM.Ofp.Rows.Count; i++)
                {
                    waterSaturation.Add((double)LiquidDebitGainVM.Ofp.Rows[i][0]);

                    Krw.Add(new ObservablePoint((double)LiquidDebitGainVM.Ofp.Rows[i][0],
                        (double)LiquidDebitGainVM.Ofp.Rows[i][2]));

                    Kro.Add(new ObservablePoint((double)LiquidDebitGainVM.Ofp.Rows[i][0],
                       (double)LiquidDebitGainVM.Ofp.Rows[i][1]));
                }

                SwSeriesCollection = new SeriesCollection();

                SwSeriesCollection.Add(
                    new ScatterSeries
                    {
                        Title = "Krw",
                        Values = new ChartValues<ObservablePoint>(Krw),
                        Fill = (Brush)(new BrushConverter()).ConvertFromString("#786FA6")
                    });

                SwSeriesCollection.Add(
                    new ScatterSeries
                    {
                        Title = "Kro",
                        Values = new ChartValues<ObservablePoint>(Kro),
                        Fill = (Brush)(new BrushConverter()).ConvertFromString("#E15F41")
                    });

                List<ObservablePoint> KrwCurve = new List<ObservablePoint>();
                List<ObservablePoint> KroCurve = new List<ObservablePoint>();

                var Scw = Krw.Where(x => x.Y == Krw.Min(z => z.Y)).First().X;
                var Sor = 1 - Krw.Where(x => x.Y == Krw.Max(z => z.Y)).First().X;

                var KrwMult = Krw.Max(x => x.Y);
                var KroMult = Kro.Max(x => x.Y);

                var nw = GetNw(Krw, KrwMult, Scw, Sor, 0.1, 0, 0.005, true);
                var no = GetNo(Kro, KroMult, Scw, Sor, 0.1, 0, 0.005, true);

                for (double i = Scw; i <= 1 - Sor; i += 0.01)
                {
                    KrwCurve.Add(new ObservablePoint(i, KrwMult * Math.Pow((i - Scw) / (1 - Scw - Sor), nw)));
                    KroCurve.Add(new ObservablePoint(i, KroMult * Math.Pow((1 - i - Sor) / (1 - Scw - Sor), no)));
                }

                SwSeriesCollection.Add(
                   new LineSeries
                   {
                       Title = "Krw",
                       Values = new ChartValues<ObservablePoint>(KrwCurve),
                       Stroke = (Brush)(new BrushConverter()).ConvertFromString("#786FA6"),
                       Fill = Brushes.Transparent,
                       PointGeometrySize = 0
                   });

                SwSeriesCollection.Add(
                    new LineSeries
                    {
                        Title = "Kro",
                        Values = new ChartValues<ObservablePoint>(KroCurve),
                        Stroke = (Brush)(new BrushConverter()).ConvertFromString("#E15F41"),
                        Fill = Brushes.Transparent,
                        PointGeometrySize = 0
                    });

                SwLabels = waterSaturation.ConvertAll(x => x.ToString());

                OnPropertyChanged(nameof(SwLabels));
                OnPropertyChanged(nameof(SwSeriesCollection));

                // adding info to the object
                //LiquidDebitGainVM.Kw = Krw.Max(z => z.Y) * Math.Pow((LiquidDebitGainVM.Sw - Scw) / (1 - Scw - Sor), 2.298);
                //LiquidDebitGainVM.Ko = Kro.Max(z => z.Y) * Math.Pow((1 - LiquidDebitGainVM.Sw - Sor) / (1 - Scw - Sor), 2.874);
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Ошибка построения кривых Krw и Kro{Environment.NewLine}
                                {e.Message}");
            }

            IsReady = true;
        }
        private double GetNo(List<ObservablePoint> tableValues, double KroMult, double Scw, double Sor, double step, double no, double threshold, bool flag, double minErr = 99999)
        {
            if (flag == true)
            {
                for (double i = no; i < 100; i += step)
                {
                    // square error
                    double sum = 0;
                    foreach (var point in tableValues)
                    {
                        sum += Math.Pow(point.Y - KroMult * Math.Pow((1 - point.X - Sor) / (1 - Scw - Sor), i), 2);
                    }
                    var err = sum / tableValues.Count;

                    if (minErr >= err)
                    {
                        minErr = err;
                        no = i;
                    }
                    else if (minErr > threshold)
                    {
                        GetNo(tableValues, KroMult, Scw, Sor, step / 2, no, threshold, false, minErr);
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
                        sum += Math.Pow(point.Y - KroMult * Math.Pow((1 - point.X - Sor) / (1 - Scw - Sor), i), 2);
                    }
                    var err = sum / tableValues.Count;

                    if (minErr >= err)
                    {
                        minErr = err;
                        no = i;
                    }
                    else if (minErr > threshold)
                    {
                        GetNo(tableValues, KroMult, Scw, Sor, step / 2, no, threshold, true, minErr);
                    }
                    else
                    {
                        return no;
                    }
                }
                return no;
            }
        }
        private double GetNw(List<ObservablePoint> tableValues, double KrwMult, double Scw, double Sor, double step, double nw, double threshold, bool flag, double minErr = 99999)
        {
            if (flag == true)
            {
                for (double i = nw; i < 100; i += step)
                {
                    // square error
                    double sum = 0;
                    foreach (var point in tableValues)
                    {
                        sum += Math.Pow(point.Y - KrwMult * Math.Pow((point.X - Scw) / (1 - Scw - Sor), i), 2);
                    }
                    var err = sum / tableValues.Count;

                    if (minErr >= err)
                    {
                        minErr = err;
                        nw = i;
                    }
                    else if (minErr > threshold)
                    {
                        GetNw(tableValues, KrwMult, Scw, Sor, step / 2, nw, threshold, false, minErr);
                    }
                    else
                    {
                        return nw;
                    }
                    System.Diagnostics.Debug.WriteLine(nw);
                }
                return nw;
            }
            else
            {
                for (double i = nw; i > 0; i -= step)
                {
                    // square error
                    double sum = 0;
                    foreach (var point in tableValues)
                    {
                        sum += Math.Pow(point.Y - KrwMult * Math.Pow((point.X - Scw) / (1 - Scw - Sor), i), 2);
                    }
                    var err = sum / tableValues.Count;

                    if (minErr >= err)
                    {
                        minErr = err;
                        nw = i;
                    }
                    else if (minErr > threshold)
                    {
                        GetNw(tableValues, KrwMult, Scw, Sor, step / 2, nw, threshold, true, minErr);
                    }
                    else
                    {
                        return nw;
                    }
                    System.Diagnostics.Debug.WriteLine(nw);
                }
                return nw;
            }
        }

        /// <summary>
        /// Calculate water debit gain
        /// </summary>
        /// <returns></returns>
        private async Task CalculateLiquidDebitGainAsync()
        {
            IsReady = false;

            if ((LiquidDebitGainVM.K == null || LiquidDebitGainVM.H == null) &&
                LiquidDebitGainVM.KH == null || LiquidDebitGainVM.Pr == null)
            {
                MessageBox.Show("Выберите карты для рассчета");
                IsReady = true;

                return;
            }

            if (LiquidDebitGainVM.Kw == -1 || LiquidDebitGainVM.Ko == -1)
            {
                await CalculateDupuisAsync();
            }

            await Task.Run(() =>
            {
                var result = liquidDebitGainService.CreateMapLiquidDebitGain(LiquidDebitGainVM);
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
                    }));
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage);
                    return;
                }
            });

            IsReady = true;
        }

        /// <summary>
        /// Reset values for the water debit gain calculations
        /// </summary>
        /// <returns></returns>
        private async Task ResetLiquidDebitGainAsync()
        {
            IsReady = false;

            await Task.Run(LiquidDebitGainVM.Reset);

            OnPropertyChanged(nameof(LiquidDebitGainVM));

            IsReady = true;
        }

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
                    return;
                }

                NeuralNetVM.trainingDataFileName = fileName;
            });

            OnPropertyChanged(nameof(NeuralNetVM));

            IsReady = true;
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
                    return;
                }

                NeuralNetVM.initialDataFileName = fileName;
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

            if (string.IsNullOrEmpty(NeuralNetVM.trainingDataFileName))
            {
                MessageBox.Show("Выберите файл для обучения нейросети.");
                return;
            }
            await Task.Run(() =>
            {
                var result = neuralNetService.TrainNeuralNetwork(NeuralNetVM);
                if (result.IsSuccess)
                {
                    NeuralNetVM = result.Data;
                    OnPropertyChanged(nameof(NeuralNetVM));
                    IsNeuralNetTrainResultsVisible = true;

                }
                else
                {
                    MessageBox.Show(result.ErrorMessage);
                }
            });

            IsReady = true;
        }

        /// <summary>
        /// Predict values with neuralnet
        /// </summary>
        /// <returns></returns>
        private async Task CalculateNeuralNetAsync()
        {
            IsReady = false;
            IsNeuralNetPredictionResultsVisible = false;

            if (string.IsNullOrEmpty(NeuralNetVM.initialDataFileName))
            {
                MessageBox.Show("Выберите файл для прогнозирования.");
                return;
            }

            await Task.Run(() =>
            {
                var result = neuralNetService.CalculateLiquidDebitGain(NeuralNetVM);
                if (result.IsSuccess)
                {
                    NeuralNetVM = result.Data;
                    IsNeuralNetPredictionResultsVisible = true;
                    OnPropertyChanged(nameof(NeuralNetVM));
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage);
                }
            });

            IsReady = true;
        }

        /// <summary>
        /// Reset neuralnet
        /// </summary>
        /// <returns></returns>
        private async Task ResetNeuralNetAsync()
        {
            IsReady = false;
            IsNeuralNetTrainResultsVisible = false;

            await Task.Run(NeuralNetVM.Reset);
            OnPropertyChanged(nameof(NeuralNetVM));

            IsReady = true;
        }

        /// <summary>
        /// Export map values
        /// </summary>
        /// <returns></returns>
        private async Task ExportMapValuesAsync()
        {
            IsReady = false;

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\Users";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                MessageBox.Show("You selected: " + dialog.FileName);
            }
            exportMapValuesService.ExportMapValuesToExcel(MapVMs.Where(x => x.IsExporting == true), OilWellVMs, dialog.FileName + "\\mapExprot.xlsx");

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

        

            private List<object> readFile_excel_list_1(string fileNames)
            {
                var extension = fileNames.Substring(fileNames.LastIndexOf('.'));
                // Создаем поток для чтения.
                FileStream stream = File.Open(fileNames, FileMode.Open, FileAccess.Read);
                // В зависимости от расширения файла Excel, создаем тот или иной читатель.
                // Читатель для файлов с расширением *.xlsx.
                if (extension == ".xlsx")
                    edr = ExcelReaderFactory.CreateOpenXmlReader(stream);
                // Читатель для файлов с расширением *.xls.
                else if (extension == ".xls")
                    edr = ExcelReaderFactory.CreateBinaryReader(stream);

                //// reader.IsFirstRowAsColumnNames
                var conf = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                };
                // Читаем, получаем DataView и работаем с ним как обычно.
                DataSet dataSet = edr.AsDataSet(conf);
                //DataTable dt = dataSet.Tables[0];
                DataTable dataTable = dataSet.Tables[0];
                DataRowCollection row = dataTable.Rows;

                List<object> rowDataList = null;
                List<object> allRowsList = new List<object>();

                foreach (DataRow item in row)
                {
                    rowDataList = item.ItemArray.ToList();
                    allRowsList.Add(rowDataList);
                }


                //n_1_columns = dataTable.Columns.Count;
                //array_global_data_list_1[n_1_row,n_1_columns];

                // После завершения чтения освобождаем ресурсы.
                edr.Close();
                return allRowsList;
            }

            private List<object> readFile_excel_list_2(string fileNames)
            {
                var extension = fileNames.Substring(fileNames.LastIndexOf('.'));
                // Создаем поток для чтения.
                FileStream stream = File.Open(fileNames, FileMode.Open, FileAccess.Read);
                // В зависимости от расширения файла Excel, создаем тот или иной читатель.
                // Читатель для файлов с расширением *.xlsx.
                if (extension == ".xlsx")
                    edr = ExcelReaderFactory.CreateOpenXmlReader(stream);
                // Читатель для файлов с расширением *.xls.
                else if (extension == ".xls")
                    edr = ExcelReaderFactory.CreateBinaryReader(stream);

                //// reader.IsFirstRowAsColumnNames
                var conf = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                };
                // Читаем, получаем DataView и работаем с ним как обычно.
                DataSet dataSet = edr.AsDataSet(conf);
                var dataTable = dataSet.Tables[1];
                DataRowCollection row = dataTable.Rows;

                List<object> rowDataList = null;
                List<object> allRowsList = new List<object>();

                foreach (DataRow item in row)
                {
                    rowDataList = item.ItemArray.ToList();
                    allRowsList.Add(rowDataList);
                }


                //n_1_columns = dataTable.Columns.Count;
                //array_global_data_list_1[n_1_row,n_1_columns];

                // После завершения чтения освобождаем ресурсы.
                edr.Close();
                return allRowsList;
            }

        }


    }

