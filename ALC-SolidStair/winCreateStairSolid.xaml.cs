using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace LX_SolidStair
{
    /// <summary>
    /// Interaction logic for winCreateStair.xaml
    /// </summary>
    public partial class winCreateStair : Window
    {
        public List<LayerObject> Layers;
        public LayerObject layer;

        public winCreateStair(List<LayerObject> _layers, BaseStairObject _bso)
        {
            InitializeComponent();
            // Layers = _layers;
            CB_Layers.ItemsSource = _layers;
            this.DataContext =_bso;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }


    }
}
