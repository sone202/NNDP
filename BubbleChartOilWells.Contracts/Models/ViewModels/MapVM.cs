﻿using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;

namespace BubbleChartOilWells.Contracts.Models.ViewModels
{
    public class MapVM : INotifyPropertyChanged
    {
        private bool isSelected;

        public double CellWidth { get; set; }
        public double CellHeight { get; set; }
        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public List<double> Z { get; set; }
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
        public bool IsExporting { get; set; }
        public Point LeftBottomCoordinate { get; set; }
        public BitmapSource BitmapSource { get; set; }

        public ObjectiveVM Objective { get; set; }

        // TODO: refactor
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
