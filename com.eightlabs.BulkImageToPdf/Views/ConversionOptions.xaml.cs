using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using com.eightlabs.BulkImageToPdf.ViewModels;


namespace com.eightlabs.BulkImageToPdf.Views
{
    /// <summary>
    /// Interaction logic for ConversionOptions.xaml
    /// </summary>
    public partial class ConversionOptions : UserControl
    {
        public ConversionOptions()
        {
            InitializeComponent();
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            ((ConversionOptionsScreenViewModel)this.DataContext).Main.Screens.MoveCurrentToNext();
            ((ConversionOptionsScreenViewModel)this.DataContext).Main.SaveSettings();
            ((ConversionOptionsScreenViewModel)this.DataContext).Main.Convert();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ((ConversionOptionsScreenViewModel)this.DataContext).Main.Cancel();
        }
    }
}
