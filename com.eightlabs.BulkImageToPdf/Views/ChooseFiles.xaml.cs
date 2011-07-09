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
    /// Interaction logic for ChooseFiles.xaml
    /// </summary>
    public partial class ChooseFiles : UserControl
    {
        public ChooseFiles()
        {
            InitializeComponent();
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            ((ChooseFilesScreenViewModel)this.DataContext).Main.Screens.MoveCurrentToNext();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ((ChooseFilesScreenViewModel)this.DataContext).Main.Cancel();
        }
    }
}
