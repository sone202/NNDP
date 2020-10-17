using AsyncAwaitBestPractices.MVVM;
using BubbleChartOilWells.Models;
using Microsoft.Vbe.Interop;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BubbleChartOilWells.ViewModels
{
    public class MainViewModel : BaseViewModel
    {

        private bool fileImportIsBusy = false;
        private bool treeVisibility = false;
        private bool settingsVisibility = false;
        private bool anySelected = false;
        private string wellInfo;

        private AsyncCommand importFileAsyncCommand;
        private AsyncCommand openSettingsAsyncCommand;
        private AsyncCommand openTreeAsyncCommand;
        private AsyncCommand bubbleSelectingAsyncCommand;

        static private List<Bubble> oilWell;
        static public Bubble CurrentSelected;

        static private Dictionary<Bubble, OilWell> dataBubbleOilWell = new Dictionary<Bubble, OilWell>();

        static public ObservableCollection<object> OilWellsPaths { get; private set; } = new ObservableCollection<object>();


        public bool FileImportIsBusy
        {
            get => fileImportIsBusy;
            set
            {
                if (fileImportIsBusy != value)
                {
                    fileImportIsBusy = value;
                    ImportFileAsyncCommand.RaiseCanExecuteChanged();
                }
            }
        }
        public bool TreeVisibility
        {
            get => treeVisibility;
            set
            {
                treeVisibility = value;
                OnPropertyChanged(nameof(TreeVisibility));
            }
        }
        public bool SettingsVisibility
        {
            get => settingsVisibility;
            set
            {
                settingsVisibility = value;
                OnPropertyChanged(nameof(SettingsVisibility));
            }
        }
        public bool AnySelected
        {
            get => anySelected;
            set
            {
                anySelected = value;
                OnPropertyChanged(nameof(AnySelected));
            }
        }
        public string WellInfo
        {
            get => wellInfo;
            set
            {
                wellInfo = value;
                OnPropertyChanged(nameof(WellInfo));
            }
        }


        public AsyncCommand ImportFileAsyncCommand
        {
            get { return importFileAsyncCommand ?? (importFileAsyncCommand = new AsyncCommand(FileImportAsync, CanExecute)); }
        }
        private async Task FileImportAsync()
        {
            // adding wells to the grid
            try
            {
                FileImportIsBusy = true;
                await Task.Run(() => oilWell = DataImport.GetDataList());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                FileImportIsBusy = false;
            }
            if (oilWell?.Count > 0)
                foreach (var bubble in oilWell)
                {
                    bubble.Update();
                    foreach (var path in bubble.Paths)
                    {
                        OilWellsPaths.Add(path);
                    }
                    OilWellsPaths.Add(bubble.ID);

                    dataBubbleOilWell[bubble] = bubble.Data;
                }

            FileImportIsBusy = false;
        }
        private bool CanExecute(object sender)
        {
            return !FileImportIsBusy;
        }

        public AsyncCommand OpenSettingsAsyncCommand
        {
            get { return openSettingsAsyncCommand ?? (openSettingsAsyncCommand = new AsyncCommand(OpenSettingsAsync)); }
        }
        private async Task OpenSettingsAsync()
        {
            await Task.Run(() => SettingsVisibility = SettingsVisibility == false ? true : false);
        }

        public AsyncCommand OpenTreeAsyncCommand
        {
            get { return openTreeAsyncCommand ?? (openTreeAsyncCommand = new AsyncCommand(OpenTreeAsync)); }
        }
        private async Task OpenTreeAsync()
        {
            await Task.Run(() => TreeVisibility = TreeVisibility == false ? true : false);
        }

        public AsyncCommand BubbleSelectingAsyncCommand
        {
            get { return bubbleSelectingAsyncCommand ?? (bubbleSelectingAsyncCommand = new AsyncCommand(BubbleSelectingAsync)); }
        }
        private async Task BubbleSelectingAsync()
        {
            // getting element under mouse cursor
            var el = Mouse.DirectlyOver;
            Bubble clicked_well = null;
            if (OilWellsPaths.Contains(el))
            {
                System.Threading.Tasks.Parallel.ForEach(oilWell, well =>
                {
                    if (well.Contains(el))
                    {
                        clicked_well = well;
                        return;
                    }
                });
                CurrentSelected?.Unselect();
                clicked_well?.Select();
                CurrentSelected = clicked_well;
                AnySelected = true;
            }
            else
            {
                CurrentSelected?.Unselect();
                CurrentSelected = null;
                AnySelected = false;
            }
            if (CurrentSelected != null)
                WellInfo = "Номер скважины: " + CurrentSelected.Data.ID +
                "\nX = " + CurrentSelected.Data.X +
                "\nY = " + CurrentSelected.Data.Y +
                "\nТекущий дебит нефти = " + CurrentSelected.Data.oil_debit + " т/сут" +
                "\nТекущий дебит жидкости = " + CurrentSelected.Data.liquid_debit + " т/сут" +
                "\nНакопленная добыча нефти = " + CurrentSelected.Data.oil_prod + " тыс. т" +
                "\nНакопленная добыча жидкости = " + CurrentSelected.Data.liquid_prod + " тыс. т";
        }
    }
}

