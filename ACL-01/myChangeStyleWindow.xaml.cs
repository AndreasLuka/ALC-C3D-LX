using System;
using System.Collections.Generic;
using System.Windows;


namespace ACL_01.Civil3D
{
    /// <summary>
    /// Interaction logic for myWindow.xaml
    /// </summary>
    public partial class myChangeStyleWindow : Window
    {
        public List<BaseC3DObject> Alignments;
        public BaseC3DObject AlignmentStyle;
        public myChangeStyleWindow(List<BaseC3DObject> _alignments, List<BaseC3DObject> _alignmentStyles)
        {
            InitializeComponent();
            Alignments = _alignments;
            ComboBox_Style.ItemsSource = _alignmentStyles;
            ListBox_Aligns.DataContext = Alignments;
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
