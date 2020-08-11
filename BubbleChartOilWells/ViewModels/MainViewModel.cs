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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BubbleChartOilWells.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        static public List<Bubble> _oil_wells = new List<Bubble>();
        static public ObservableCollection<object> _oil_wells_paths { get; private set; } = new ObservableCollection<object>();
        static public Bubble _current_selected;

        static public Dictionary<Bubble, OilWell> _data_Bubble_OilWell = new Dictionary<Bubble, OilWell>();
        static public Dictionary<Path, OilWell> _data_Path_OilWell = new Dictionary<Path, OilWell>();
        static public Dictionary<Path, Bubble> _data_Path_Bubble = new Dictionary<Path, Bubble>();


        static public ObservableCollection<object> _widgets { get; private set; } = new ObservableCollection<object>();

        private Border box_settings = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
        };
        private TreeView _tree = new TreeView { };
       



        private AsyncCommand _importFileAsyncCommand;
        public AsyncCommand ImportFileAsyncCommand
        {
            get { return _importFileAsyncCommand ?? (_importFileAsyncCommand = new AsyncCommand(FileImportAsync)); }
        }
        private async Task FileImportAsync()
        {

            // adding wells to the grid
            _oil_wells = await Task.Run(() => DataImport.GetDataList());
            foreach (var bubble in _oil_wells)
            {
                bubble.Update();

                foreach (var path in bubble.paths)
                    _oil_wells_paths.Add(path);

                _oil_wells_paths.Add(bubble.ID);
            }
        }



        private AsyncCommand _openSettingsAsyncCommand;
        public AsyncCommand OpenSettingsAsyncCommand
        {
            get { return _openSettingsAsyncCommand ?? (_openSettingsAsyncCommand = new AsyncCommand(OpenSettingsAsync)); }
        }
        private async Task OpenSettingsAsync()
        {
            TextBox tmp = new TextBox();
            tmp.Text = "settings test";
            box_settings.Child = tmp;
            if (!_widgets.Contains(box_settings))
                _widgets.Add(box_settings);
            else
                _widgets.Remove(box_settings);
        }



        private AsyncCommand _openTreeAsyncCommand;
        public AsyncCommand OpenTreeAsyncCommand
        {
            get { return _openTreeAsyncCommand ?? (_openTreeAsyncCommand = new AsyncCommand(OpenTreeAsync)); }
        }
        private async Task OpenTreeAsync()
        {
            ;
            if (!_widgets.Contains(_tree))
                _widgets.Add(_tree);
            else
                _widgets.Remove(_tree);
        }
    }
}
