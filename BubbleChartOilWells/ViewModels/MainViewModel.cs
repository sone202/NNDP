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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BubbleChartOilWells.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        static public List<Region> REGIONS = new List<Region>();

        public MainViewModel()
        {
            _oil_wells = DataImport._oil_wells;
            ;
        }


        static private List<OilWell> _oil_wells;
        static public Bubble _current_selected;
        static private Dictionary<Bubble, OilWell> _data_Bubble_OilWell = new Dictionary<Bubble, OilWell>();
        //static private Dictionary<Path, OilWell> _data_Path_OilWell = new Dictionary<Path, OilWell>();
        //static private Dictionary<Path, Bubble> _data_Path_Bubble = new Dictionary<Path, Bubble>();

        static public CompositeCollection oil_wells_paths { get; private set; } = new CompositeCollection();
        static public ObservableCollection<object> oil_wells_prod { get; private set; } = new ObservableCollection<object>();
        static public ObservableCollection<object> oil_wells_debit { get; private set; } = new ObservableCollection<object>();

        #region Observable valuables
        private bool _fileImport_isBusy = false;
        public bool fileImport_isBusy
        {
            get => _fileImport_isBusy;
            set
            {
                if (_fileImport_isBusy != value)
                {
                    _fileImport_isBusy = value;
                    ImportFileAsyncCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private int _scale = Bubble.scale;
        public int scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                Bubble.scale = value;
                if (_oil_wells != null && _oil_wells.Count != 0)
                    foreach (var el in _oil_wells)
                        //  el.Update();
                        OnPropertyChanged(nameof(scale));
            }
        }

        private bool _scale_focus = true;
        public bool scale_focus
        {
            get { return _scale_focus; }
            set
            {
                _scale_focus = value;
                OnPropertyChanged(nameof(scale_focus));
            }
        }

        private bool _tree_visibility = false;
        public bool tree_visibility
        {
            get => _tree_visibility;
            set
            {
                _tree_visibility = value;
                OnPropertyChanged(nameof(tree_visibility));
            }
        }


        private bool _settings_visibility = false;
        public bool settings_visibility
        {
            get => _settings_visibility;
            set
            {
                _settings_visibility = value;
                OnPropertyChanged(nameof(settings_visibility));
            }
        }


        private bool _any_selected = false;
        public bool any_selected
        {
            get => _any_selected;
            set
            {
                _any_selected = value;
                OnPropertyChanged(nameof(any_selected));
            }
        }


        //private string _well_info;
        //public string well_info
        //{
        //    get => _well_info;
        //    set
        //    {
        //        _well_info = value;
        //        OnPropertyChanged(nameof(well_info));
        //    }
        //}

        //private bool _debit_shown = true;
        //public bool debit_shown
        //{
        //    get => _debit_shown;
        //    set
        //    {
        //        _debit_shown = value;
        //        foreach (Bubble el in _oil_wells)
        //        {
        //            el.ShowDebit(value);
        //            el.ShowID(_prod_shown, _debit_shown);
        //        }
        //        OnPropertyChanged(nameof(debit_shown));
        //    }
        //}

        //private bool _prod_shown = true;
        //public bool prod_shown
        //{
        //    get => _prod_shown;
        //    set
        //    {
        //        _prod_shown = value;
        //        foreach (var el in _oil_wells)
        //        {
        //            el.ShowProd(value);
        //            el.ShowID(_prod_shown, _debit_shown);
        //        }
        //        OnPropertyChanged(nameof(prod_shown));
        //    }
        //}
        #endregion

        #region Async commands
        private AsyncCommand _importFileAsyncCommand;
        public AsyncCommand ImportFileAsyncCommand
        {
            get { return _importFileAsyncCommand ?? (_importFileAsyncCommand = new AsyncCommand(FileImportAsync, CanFileImport)); }
        }
        private async Task FileImportAsync()
        {
            // adding wells to the grid
            try
            {
                fileImport_isBusy = true;
                await Task.Run(() => _oil_wells = DataImport._oil_wells);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                fileImport_isBusy = false;
            }



            //if (_oil_wells?.Count > 0)
            //{
            //    foreach (var bubble in _oil_wells)
            //    {
            //        bubble.Update();
            //        foreach (var path in bubble.paths_debit)
            //        {
            //            oil_wells_debit.Add(path);
            //            oil_wells_paths.Add(path);
            //        }
            //        foreach (var path in bubble.paths_prod)
            //        {
            //            oil_wells_prod.Add(path);
            //            oil_wells_paths.Add(path);
            //        }
            //        //oil_wells_debit.Add(bubble.ID);
            //        //.oil_wells_prod.Add(bubble.ID);
            //        oil_wells_paths.Add(bubble.ID);

            //        _data_Bubble_OilWell[bubble] = bubble.data;
            //    }
            //}
            fileImport_isBusy = false;
        }
        private bool CanFileImport(object sender) { return !fileImport_isBusy; }


        private AsyncCommand _openSettingsAsyncCommand;
        public AsyncCommand OpenSettingsAsyncCommand
        {
            get { return _openSettingsAsyncCommand ?? (_openSettingsAsyncCommand = new AsyncCommand(OpenSettingsAsync)); }
        }
        private async Task OpenSettingsAsync()
        {
            await Task.Run(() => settings_visibility = settings_visibility == false ? true : false);
        }



        private AsyncCommand _openTreeAsyncCommand;
        public AsyncCommand OpenTreeAsyncCommand
        {
            get { return _openTreeAsyncCommand ?? (_openTreeAsyncCommand = new AsyncCommand(OpenTreeAsync)); }
        }
        private async Task OpenTreeAsync()
        {
            await Task.Run(() => tree_visibility = tree_visibility == false ? true : false);
        }


        //private AsyncCommand _bubbleSelectingAsyncCommand;
        //public AsyncCommand BubbleSelectingAsyncCommand
        //{
        //    get { return _bubbleSelectingAsyncCommand ?? (_bubbleSelectingAsyncCommand = new AsyncCommand(BubbleSelectingAsync)); }
        //}
        //private async Task BubbleSelectingAsync()
        //{
        //    // getting element under mouse cursor
        //    var el = Mouse.DirectlyOver;
        //    Bubble clicked_well = null;
        //    if (oil_wells_paths.Contains(el))
        //    {
        //        System.Threading.Tasks.Parallel.ForEach(_oil_wells, well =>
        //        {
        //            if (well.Contains(el))
        //            {
        //                clicked_well = well;
        //                return;
        //            }
        //        });
        //        _current_selected?.Unselect();
        //        clicked_well?.Select();
        //        _current_selected = clicked_well;
        //        any_selected = true;
        //    }
        //    else
        //    {
        //        _current_selected?.Unselect();
        //        _current_selected = null;
        //        any_selected = false;
        //    }
        //    if (_current_selected != null)
        //        well_info = "Номер скважины: " + _current_selected.data.ID +
        //        "\nX = " + _current_selected.data.X +
        //        "\nY = " + _current_selected.data.Y +
        //        "\nТекущий дебит нефти = " + _current_selected.data.oil_debit + " т/сут" +
        //        "\nТекущий дебит жидкости = " + _current_selected.data.liquid_debit + " т/сут" +
        //        "\nНакопленная добыча нефти = " + _current_selected.data.oil_prod + " тыс. т" +
        //        "\nНакопленная добыча жидкости = " + _current_selected.data.liquid_prod + " тыс. т";
        //}
        #endregion
    }
}

