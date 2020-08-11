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
        static private List<Bubble> _oil_wells = new List<Bubble>();
        static private ObservableCollection<Node> _tree_data { get; set; }
        //static public Bubble _current_selected;

        //static private Dictionary<Bubble, OilWell> _data_Bubble_OilWell = new Dictionary<Bubble, OilWell>();
        //static private Dictionary<Path, OilWell> _data_Path_OilWell = new Dictionary<Path, OilWell>();
        //static private Dictionary<Path, Bubble> _data_Path_Bubble = new Dictionary<Path, Bubble>();


        static public ObservableCollection<object> oil_wells_paths { get; private set; } = new ObservableCollection<object>();

        static public ObservableCollection<object> widgets { get; private set; }




        public MainViewModel()
        {
            widgets = new ObservableCollection<object>();
            for (int i = 0; i < 2; i++)
                widgets.Add(new Border
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Visibility = Visibility.Collapsed
                });

            TreeView test = new TreeView
            {
            };
            _tree_data = new ObservableCollection<Node>
            {
                new Node { Name = "Скважины" },
                new Node
                {
                    Name ="Пузырьковые карты",
                    Nodes = new ObservableCollection<Node>
                    {
                         new Node { Name = "Карта текущих отборов" },
                         new Node { Name = "Карта накопленных отборов" }
                    }
                },
                new Node { Name = "Карты" }
            };
        }



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
                    oil_wells_paths.Add(path);

                oil_wells_paths.Add(bubble.ID);
            }
        }



        private AsyncCommand _openSettingsAsyncCommand;
        public AsyncCommand OpenSettingsAsyncCommand
        {
            get { return _openSettingsAsyncCommand ?? (_openSettingsAsyncCommand = new AsyncCommand(OpenSettingsAsync)); }
        }
        private async Task OpenSettingsAsync()
        {
            (widgets[0] as Border).Visibility = (widgets[0] as Border).Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

            if ((widgets[0] as Border).Visibility == Visibility.Visible)
                (widgets[0] as Border).Child = new TextBox { Text = "Settings_test" };

        }



        private AsyncCommand _openTreeAsyncCommand;
        public AsyncCommand OpenTreeAsyncCommand
        {
            get { return _openTreeAsyncCommand ?? (_openTreeAsyncCommand = new AsyncCommand(OpenTreeAsync)); }
        }
        private async Task OpenTreeAsync()
        {
            (widgets[1] as Border).Visibility = (widgets[1] as Border).Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

            if ((widgets[1] as Border).Visibility == Visibility.Visible)
                (widgets[1] as Border).Child = new TreeView { ItemsSource = _tree_data };

            TreeView tmp = new TreeView { ItemsSource = _tree_data };
               
        }
    }
}
