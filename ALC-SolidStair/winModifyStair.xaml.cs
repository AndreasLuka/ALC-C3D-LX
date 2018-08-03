using System.Collections.Generic;
using System.Windows;


namespace LX_SolidStair
{
    /// <summary>
    /// Interaction logic for winCreateStair.xaml
    /// </summary>
    public partial class winModifyStairSolid : Window
    {
        public List<LayerObject> Layers;
        public LayerObject layer;


        public winModifyStairSolid(List<LayerObject> _layers, SolidStairObject _bso)
        {
            InitializeComponent();

            this.DataContext = _bso;

            List<LayerObject> Layers = _layers;

            int i = 0;
            foreach (LayerObject layer in Layers)
            {
                if (layer.IsSelected) break;
                else i++;
            }

            CB_Layers.ItemsSource = Layers;
            CB_Layers.SelectedIndex = i;
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
