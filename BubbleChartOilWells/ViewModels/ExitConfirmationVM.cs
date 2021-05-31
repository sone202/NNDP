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
        private readonly SessionService sessionService;

        private AsyncCommand saveSessionAsyncCommand;
        private AsyncCommand openSessionAsyncCommand;

        public AsyncCommand SaveSessionAsyncCommand =>
            saveSessionAsyncCommand ?? (saveSessionAsyncCommand = new AsyncCommand(SaveSessionAsync));

        public AsyncCommand OpenSessionAsyncCommand =>
            openSessionAsyncCommand ?? (openSessionAsyncCommand = new AsyncCommand(OpenSessionAsync));


        /// <summary>
        /// Saves current session
        /// </summary>
        private async Task SaveSessionAsync()
        {
            IsReady = false;

            SaveFileDialog dialog = new SaveFileDialog
            {
                InitialDirectory = "Documents",
                Filter = "(*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                await Task.Run(() =>
                {
                    var result = sessionService.SaveSession(dialog.FileName, jsonOilWellsFileName, MapVMs, DebitGainVM);
                    if (result.IsSuccess)
                    {
                        Application.Current.Dispatcher.Invoke(() => Application.Current.MainWindow.Close());
                    }
                    else
                    {
                        MessageBox.Show(result.ErrorMessage);
                    }
                });
            }


            IsReady = true;
        }

        /// <summary>
        /// Opens session
        /// </summary>
        private async Task OpenSessionAsync()
        {
            IsReady = false;

            await Task.Run(() =>
            {
                var dialog = new OpenFileDialog
                {
                    InitialDirectory = "Documents",
                    Filter = "(*.json)|*.json"
                };
                var fileName = dialog.ShowDialog() == true ? dialog.FileName : string.Empty;

                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    // TODO: workaround return values
                    var result = sessionService.OpenSession(dialog.FileName);
                    if (result.IsSuccess)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            OilWellVMs = result.Data.ToList()[0] as IEnumerable<OilWellVM>;
                            MapVMs = new ObservableCollection<MapVM>(result.Data.ToList()[1] as IEnumerable<MapVM>);

                            if (MapVMs != null)
                            {
                                try
                                {
                                    SelectedMap = MapVMs.Where(x => x.IsSelected).First();
                                }
                                catch
                                {
                                    SelectedMap = MapVMs.First();
                                    MapVMs.First().IsSelected = true;
                                }
                            }

                            DebitGainVM = result.Data.ToList()[2] as DebitGainVM;

                            IsProdMapChecked = true;
                            OnPropertyChanged(nameof(OilWellVMs));
                            OnPropertyChanged(nameof(MapVMs));
                        }));
                    }
                    else
                    {
                        MessageBox.Show(result.ErrorMessage);
                    }
                }
            });

            IsReady = true;
        }
    }
}