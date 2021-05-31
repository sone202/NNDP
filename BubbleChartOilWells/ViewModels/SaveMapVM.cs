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
        private readonly SaveMapService saveMapService;

        public AsyncCommand SaveMapAsyncCommand =>
            saveMapAsyncCommand ?? (saveMapAsyncCommand = new AsyncCommand(SaveMapAsync));

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
                    var result =
                        saveMapService.SaveAsIrapService(MapVMs.Where(x => x.IsExporting == true), dialog.FileName);
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
    }
}