using BubbleChartOilWells.Commands;
using BubbleChartOilWells.Interfaces;
using BubbleChartOilWells.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BubbleChartOilWells.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        static public List<Bubble> _oil_wells = new List<Bubble>();
        static public List<Path> _oil_wells_paths = new List<Path>();
        static public Bubble _current_selected;

        static public Dictionary<Bubble, OilWell> _data_bubble_OilWell = new Dictionary<Bubble, OilWell>();
        static public Dictionary<Path, OilWell> _data_path_OilWell = new Dictionary<Path, OilWell>();
        static public Dictionary<Path, Bubble> _data_path_bubble = new Dictionary<Path, Bubble>();




        private IAsyncCommand _importFileCommand;
        public IAsyncCommand ImportFileCommand
        {
            get
            {
                if (this._importFileCommand == null)
                {
                    this._importFileCommand = new RelayCommand(parm => FileImport());
                }
                return this._importFileCommand;
            }
        }
        private void FileImport()
        {

            //// adding wells to the grid
            //drawing_area.Children.Clear();
            //drawing_area.UpdateLayout();


            new DataImport(_oil_wells);
            foreach (var bubble in _oil_wells)
            {
                bubble.Update();
                foreach (var path in bubble.paths)
                {
                    _oil_wells_paths.Add(path);
                    //_data_path_bubble[path] = bubble;
                }
                //drawing_area.Children.Add(bubble.ID);
            }
        }



        private ICommand _openSettingsCommand;
        public ICommand OpenSettingsCommand
        {
            get
            {
                if (this._openSettingsCommand == null)
                {
                    this._openSettingsCommand = new RelayCommand(parm => OpenSettings());
                }
                return this._openSettingsCommand;
            }
        }
        private void OpenSettings()
        {
            TextBox box_settings = new TextBox();
            box_settings.Text = "settings test";
            //grid_tools.Children.Add(box_settings);
        }



        private ICommand _openTreeCommand;
        public ICommand OpenTreeCommand
        {
            get
            {
                if (this._openTreeCommand == null)
                {
                    this._openTreeCommand = new RelayCommand(parm => OpenTree());
                }
                return this._openTreeCommand;
            }
        }
        private void OpenTree()
        {
            TextBox box_tree = new TextBox();
            box_tree.Text = "tree test";
            //grid_tools.Children.Add(box_tree);
        }
    }
}
