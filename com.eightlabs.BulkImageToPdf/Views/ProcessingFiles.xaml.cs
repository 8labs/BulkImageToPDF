﻿using System;
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
    /// Interaction logic for ProcessingFiles.xaml
    /// </summary>
    public partial class ProcessingFiles : UserControl
    {
        public ProcessingFiles()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

            ((ProcessingFilesScreenViewModel)this.DataContext).Main.CancelProcessing();
            ((ProcessingFilesScreenViewModel)this.DataContext).Main.Screens.MoveCurrentToPrevious();
        }

        private void Cancel_All_Click(object sender, RoutedEventArgs e)
        {
            ((ProcessingFilesScreenViewModel)this.DataContext).Main.CancelProcessing();
            ((ProcessingFilesScreenViewModel)this.DataContext).Main.Cancel();
        }

    }
}
