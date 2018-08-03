using System;
using System.Collections.Generic;
using System.Windows;



namespace LX_SolidStair
{
    /// <summary>
    /// Interaction logic for myWindow.xaml
    /// </summary>
    
    public partial class winListStairs : Window
    {
        // public List<BaseStairObject> ListStair;

        public winListStairs(List<BaseStairObject> _listStairs)
        {

            InitializeComponent();
            this.Stairs.ItemsSource = _listStairs;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
