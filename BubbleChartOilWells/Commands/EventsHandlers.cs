//using System;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Input;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;


//namespace BubbleChartOilWells.Commands
//{
//    public class Commands
//    {


//        // drawing area events
//        private void drawing_area_MouseDown(object sender, MouseButtonEventArgs e)
//        {

//            bool path_is_found = false;
//            for (int i = 0; i < drawing_area.Children.Count; i++)
//            {
//                if (drawing_area.Children[i] is Path)
//                {
//                    if (drawing_area.Children[i].IsMouseOver)
//                    {
//                        // info about selected oil well
//                        border_info.Visibility = Visibility.Visible;
//                        textblock_info.Text =
//                            "Номер скважины: " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.ID +
//                            "\nX = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.X +
//                            "\nY = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Y +
//                            "\nТекущий дебит нефти = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Oil_Debit + " т/сут" +
//                            "\nТекущий дебит жидкости = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Liquid_Debit + " т/сут" +
//                            "\nНакопленная добыча нефти = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Oil_Production + " тыс. т" +
//                            "\nНакопленная добыча жидкости = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Liquid_Production + " тыс. т";
//                        //textblock_info.Text =
//                        //    "Номер скважины: " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.ID +
//                        //    "\nX = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.X +
//                        //    "\nY = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Y +
//                        //    "\nТекущий дебит нефти = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Oil_Debit + " т/сут" +
//                        //    "\nТекущий дебит жидкости = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Liquid_Debit + " т/сут" +
//                        //    "\nНакопленная добыча нефти = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Oil_Production + " тыс. т" +
//                        //    "\nНакопленная добыча жидкости = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Liquid_Production + " тыс. т";

//                        // unselecting previous selected oilwell
//                        if (_current_selected != null)
//                            _current_selected.Unselect();

//                        // selecting current well
//                        _data_path_bubble[(drawing_area.Children[i] as Path)].Select();
//                        _current_selected = _data_path_bubble[(drawing_area.Children[i] as Path)];


//                        path_is_found = true;
//                        break;
//                    }

//                }
//            }
//            if (!path_is_found)
//            {
//                // unselecting previous selected oilwell
//                if (_current_selected != null)
//                    _current_selected.Unselect();

//                // hiding info
//                border_info.Visibility = Visibility.Hidden;
//            }
//        }

//        private void drawing_area_MouseMove(object sender, MouseEventArgs e) { label_coordinates.Content = e.GetPosition(drawing_area); }
//        private void MenuItem_Click(object sender, RoutedEventArgs e)
//        {

//            // adding wells to the grid
//            drawing_area.Children.Clear();
//            drawing_area.UpdateLayout();


//            new DataImport(_oil_wells);
//            foreach (var bubble in _oil_wells)
//            {
//                bubble.Update();
//                foreach (var path in bubble._paths)
//                {
//                    drawing_area.Children.Add(path);
//                    _data_path_bubble[path] = bubble;
//                }
//                drawing_area.Children.Add(bubble.ID);
//            }
//        }


//        // toolbar buttons
//        private void Settings_Click(object sender, RoutedEventArgs e)
//        {
//            TextBox box_settings = new TextBox();
//            box_settings.Text = "settings test";
//            grid_tools.Children.Add(box_settings);
//        }
//        private void Tree_Click(object sender, RoutedEventArgs e)
//        {
//            TextBox box_tree = new TextBox();
//            box_tree.Text = "tree test";
//            grid_tools.Children.Add(box_tree);
//        }


        

//    }
//}
