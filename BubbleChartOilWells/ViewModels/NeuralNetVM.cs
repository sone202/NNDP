using AsyncAwaitBestPractices.MVVM;
using BubbleChartOilWells.BusinessLogic.Services;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Windows.Media;

namespace BubbleChartOilWells.ViewModels
{
    public partial class MainVM
    {
        // services
        private readonly NeuralNetService neuralNetService;

        // element visibility conditions
        private bool isNeuralNetTrainResultsVisibile;
        private bool isNeuralNetPredictionResultsVisible;

        // view models
        private NeuralNetVM neuralNetVM;

        // commands
        private AsyncCommand importTrainDataAsyncCommand;
        private AsyncCommand importInitialDataAsyncCommand;
        private AsyncCommand trainNeuralNetAsyncCommand;
        private AsyncCommand predictNeuralNetAsyncCommand;

        
        
        
        // element visibility conditions
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
        
        // view models
        public NeuralNetVM NeuralNetVM
        {
            get => neuralNetVM;
            set
            {
                neuralNetVM = value;
                OnPropertyChanged(nameof(NeuralNetVM));
            }
        }
        
        // commands
        public AsyncCommand ImportTrainDataAsyncCommand => importTrainDataAsyncCommand ??
                                                           (importTrainDataAsyncCommand =
                                                               new AsyncCommand(ImportTrainDataAsync));

        public AsyncCommand ImportInitialDataAsyncCommand => importInitialDataAsyncCommand ??
                                                             (importInitialDataAsyncCommand =
                                                                 new AsyncCommand(ImportInitialDataAsync));

        public AsyncCommand TrainNeuralNetAsyncCommand => trainNeuralNetAsyncCommand ??
                                                          (trainNeuralNetAsyncCommand =
                                                              new AsyncCommand(TrainNeuralNetAsync));

        public AsyncCommand PredictNeuralNetAsyncCommand => predictNeuralNetAsyncCommand ??
                                                            (predictNeuralNetAsyncCommand =
                                                                new AsyncCommand(PredictWithNeuralNetAsync));

        // plots
        public SeriesCollection CrossPlotTrainSeriesCollection { get; set; }
        public SeriesCollection CrossPlotTestSeriesCollection { get; set; }


        // methods
        
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
                    InitialDirectory = "",
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
                    crossPlotTrainPoints.Add(new ObservablePoint(NeuralNetVM.TrainPredicted[i],
                        NeuralNetVM.TrainActual[i]));
                }

                CrossPlotTrainSeriesCollection.Add(
                    new ScatterSeries
                    {
                        Title = "Кросс-плот обучения",
                        Values = new ChartValues<ObservablePoint>(crossPlotTrainPoints),
                        Fill = (Brush) (new BrushConverter()).ConvertFromString("#786FA6"),
                        MaxPointShapeDiameter = 4
                    });

                CrossPlotTestSeriesCollection = new SeriesCollection();
                var crossPlotTestPoints = new List<ObservablePoint>();
                for (int i = 0; i < NeuralNetVM.TestActual.Count; i++)
                {
                    crossPlotTestPoints.Add(
                        new ObservablePoint(NeuralNetVM.TestPredicted[i], NeuralNetVM.TestActual[i]));
                }

                CrossPlotTestSeriesCollection.Add(
                    new ScatterSeries
                    {
                        Title = "Кросс-плот тестирования",
                        Values = new ChartValues<ObservablePoint>(crossPlotTestPoints),
                        Fill = (Brush) (new BrushConverter()).ConvertFromString("#E15F41"),
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

                // var mapResult = userMapService.GetPredictedMap(neuralNetVM.PredictionFullResults);
                // if (mapResult.IsSuccess)
                // {
                //     Application.Current.Dispatcher.Invoke(new Action(() =>
                //     {
                //         if (SelectedMap != null)
                //         {
                //             SelectedMap.IsSelected = false;
                //         }
                //
                //         SelectedMap = mapResult.Data;
                //
                //         MapVMs.Add(SelectedMap);
                //     }));
                // }
                // else
                // {
                //     MessageBox.Show(mapResult.ErrorMessage);
                // }
            });

            IsReady = true;
        }
    }
}