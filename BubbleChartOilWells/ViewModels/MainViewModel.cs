using BubbleChartOilWells.Commands;
using BubbleChartOilWells.Interfaces;
using BubbleChartOilWells.Models;
using System;
using System.Activities.Statements;
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
    public class MainViewModel : BaseViewModel
    {
        static public List<Bubble> _oil_wells = new List<Bubble>();
        static public List<Path> _oil_wells_paths = new List<Path>();
        static public Bubble _current_selected;

        static public Dictionary<Bubble, OilWell> _data_Bubble_OilWell = new Dictionary<Bubble, OilWell>();
        static public Dictionary<Path, OilWell> _data_Path_OilWell = new Dictionary<Path, OilWell>();
        static public Dictionary<Path, Bubble> _data_Path_Bubble = new Dictionary<Path, Bubble>();




        //private IAsyncCommand _importFileCommand;
        //public IAsyncCommand ImportFileCommand
        //{
        //    get
        //    {
        //        if (this._importFileCommand == null)
        //        {
        //            this._importFileCommand = new AsyncCommand(() => FileImport(), () => true, null);
        //        }
        //        return this._importFileCommand;
        //    }
        //}
        private AsyncCommand _ButtonTest;
        public AsyncCommand ButtonTest
        {
            get
            {
                return _ButtonTest ?? (_ButtonTest = new AsyncCommand(FileImport));
            }
        }
        public MainViewModel()
        {
            //ButtonTest = new AsyncCommand(FileImport, CanExecuteSubmit);
            //ButtonTest = new AsyncCommand(FileImport);
        }
        
        private async Task FileImport()
        {

            //// adding wells to the grid
            //drawing_area.Children.Clear();
            //drawing_area.UpdateLayout();


            await Task.Delay(1);
            MessageBox.Show("2");
            _oil_wells = new DataImport().data_list;
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



        private IAsyncCommand _openSettingsCommand;
        public IAsyncCommand OpenSettingsCommand
        {
            get
            {
                if (this._openSettingsCommand == null)
                {
                    this._openSettingsCommand = new AsyncCommand(() => OpenSettings(), () => true, null);
                }
                return this._openSettingsCommand;
            }
        }
        private async Task OpenSettings()
        {
            TextBox box_settings = new TextBox();
            box_settings.Text = "settings test";
            //grid_tools.Children.Add(box_settings);
        }



        private IAsyncCommand _openTreeCommand;
        public IAsyncCommand OpenTreeCommand
        {
            get
            {
                if (this._openTreeCommand == null)
                {
                    this._openTreeCommand = new AsyncCommand(() => OpenTree(), () => true, null);
                }
                return this._openTreeCommand;
            }
        }
        private async Task OpenTree()
        {
            TextBox box_tree = new TextBox();
            box_tree.Text = "tree test";
            //grid_tools.Children.Add(box_tree);
        }
    }
}
