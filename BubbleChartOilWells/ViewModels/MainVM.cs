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
    public partial class MainVM : BaseVM
    {
        private readonly string jsonOilWellsFileName;
        private readonly OilWellService oilWellService;
        private readonly IrapMapService irapMapService;
        private readonly UserMapService userMapService;
        private readonly ExportMapValuesService exportMapValuesService;

        private bool isReady;
        private bool isExcelImportEnabled;
        private bool isMapImportEnabled;
        private bool isProdMapChecked;
        private bool isProdSumMapChecked;
        private bool isBubblesChecked;

        private MapVM selectedMap;

        // TODO: refactor
        private List<string> selectedOilWellPropertyValues;

        private AsyncCommand importOilWellsAsyncCommand;
        private AsyncCommand importJsonOilWellsAsyncCommand;
        private AsyncCommand importIrapMapAsyncCommand;
        private AsyncCommand importUserMapAsyncCommand;
        private AsyncCommand importUserMapContourAsyncCommand;
        private AsyncCommand exportMapValuesAsyncCommand;
        private AsyncCommand saveMapAsyncCommand;

        public IEnumerable<OilWellVM> OilWellVMs { get; set; }
        public ObservableCollection<MapVM> MapVMs { get; set; }

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
        public List<string> SelectedOilWellPropertyValues
        {
            get => selectedOilWellPropertyValues;
            set
            {
                selectedOilWellPropertyValues = value;
                OnPropertyChanged(nameof(SelectedOilWellPropertyValues));
            }
        }

        public AsyncCommand ImportOilWellsAsyncCommand => importOilWellsAsyncCommand ??
                                                          (importOilWellsAsyncCommand =
                                                              new AsyncCommand(ImportOilWellsAsync));

        public AsyncCommand ImportJsonOilWellsAsyncCommand => importJsonOilWellsAsyncCommand ??
                                                              (importJsonOilWellsAsyncCommand =
                                                                  new AsyncCommand(ImportJsonOilWellsAsync));

        public AsyncCommand ImportIrapMapAsyncCommand => importIrapMapAsyncCommand ??
                                                         (importIrapMapAsyncCommand =
                                                             new AsyncCommand(ImportIrapMapAsync));

        public AsyncCommand ImportUserMapAsyncCommand => importUserMapAsyncCommand ??
                                                         (importUserMapAsyncCommand =
                                                             new AsyncCommand(ImportUserMapAsync));

        public AsyncCommand ImportUserMapContourAsyncCommand => importUserMapContourAsyncCommand ??
                                                                (importUserMapContourAsyncCommand =
                                                                    new AsyncCommand(ImportUserMapContourAsync));

        public AsyncCommand ExportMapValuesAsyncCommand => exportMapValuesAsyncCommand ??
                                                           (exportMapValuesAsyncCommand =
                                                               new AsyncCommand(ExportMapToExcelAsync));


        public MainVM()
        {
            jsonOilWellsFileName = Path.GetFullPath("OilWells.json");
            oilWellService = new OilWellService(jsonOilWellsFileName);
            irapMapService = new IrapMapService();
            userMapService = new UserMapService(jsonOilWellsFileName);
            debitGainService = new DebitGainService();
            neuralNetService = new NeuralNetService();
            exportMapValuesService = new ExportMapValuesService();
            saveMapService = new SaveMapService();
            sessionService = new SessionService();

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
        private async Task ImportJsonOilWellsAsync()
        {
            IsExcelImportEnabled = false;
            IsProdMapChecked = false;

            await Task.Run(() =>
            {
                var isJsonExists = File.Exists(jsonOilWellsFileName);
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
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (SelectedMap != null)
                            {
                                SelectedMap.IsSelected = false;
                            }

                            SelectedMap = result.Data;
                            MapVMs.Add(SelectedMap);
                            OnPropertyChanged(nameof(MapVMs));
                        });
                    }
                    else
                    {
                        MessageBox.Show(result.ErrorMessage);
                    }
                }
            });

            IsMapImportEnabled = true;
        }

        // TODO: delete
        private void DebugMethod(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var row = reader.ReadLine().Split(new char[] {' ', '\t'});
                    OilWellVMs.First(x => x.Name == row[0]).Select();
                }

                reader.Close();
            }
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
                var isJsonExists = File.Exists(jsonOilWellsFileName);
                if (!isJsonExists)
                {
                    MessageBox.Show("Сохраненные данные отсутствуют. Необходимо импортировать excel файл.");
                    return;
                }

                var openFileForm = new OpenFileDialog
                {
                    InitialDirectory = "\\Documents",
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
                    //OnPropertyChanged(nameof(OilWellVMs));
                    if (result.IsSuccess)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            // TODO: delete
                            DebugMethod(fileName);
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
                var isJsonExists = File.Exists(jsonOilWellsFileName);
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
                    var result = exportMapValuesService.ExportMapValuesToExcel(MapVMs.Where(x => x.IsExporting == true),
                        OilWellVMs, dialog.FileName + "\\mapExport.xlsx");
                    if (!result.IsSuccess)
                    {
                        MessageBox.Show(result.ErrorMessage);
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
            var maxLiquidProdSum =
                OilWellVMs.Max(x => x.Objectives.Sum(y => y.MonthlyObjectiveProduction.LiquidProdSum));

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

        public void DrawOilWellMap()
        {
            foreach (var oilWellVM in OilWellVMs)
            {
                oilWellVM.CreateOilWellView();
            }
        }
    }
}