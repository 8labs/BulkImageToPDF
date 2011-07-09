﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using com.eightlabs.WPFCommon.ViewModels;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Threading;

using System.Windows.Media.Imaging;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace com.eightlabs.BulkImageToPdf.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            this.ImageFilesList = new List<IncomingFileViewModel>();
            this.BadFilesList = new List<IncomingFileViewModel>();
            this.Converter = new ConversionViewModel();

            //setup the screens
            List<ScreenBaseViewModel> screens = new List<ScreenBaseViewModel>();
            screens.Add(new DropFilesScreenViewModel(this));
            //screens.Add(new ChooseFilesScreenViewModel(this));
            screens.Add(new ConversionOptionsScreenViewModel(this));
            screens.Add(new ProcessingFilesScreenViewModel(this));

            this.Screens = new CollectionView(screens);
            this.Screens.MoveCurrentToFirst();
        }

        #region Private Variables


        #endregion

        #region Public Variables

        /// <summary>
        /// View model for displaying the status of files being converted.
        /// </summary>
        public ConversionViewModel Converter { get; set; }

        /// <summary>
        /// Image files to be converted
        /// </summary>
        public List<IncomingFileViewModel> ImageFilesList { get; set; }

        /// <summary>
        /// Image files to be converted
        /// </summary>
        public List<IncomingFileViewModel> BadFilesList { get; set; }

        /// <summary>
        /// currently selected file
        /// </summary>
        public IncomingFileViewModel CurrentFile { get; set; }

        /// <summary>
        /// The currently displayed screen vm (contains helpers for next/previous)
        /// </summary>
        public CollectionView Screens { get; set; }

        #endregion

        #region Private Methods

        //comparison for the file view models sorting
        private static int CompareFileName(IncomingFileViewModel x, IncomingFileViewModel y)
        {
            return x.FileName.CompareTo(y.FileName);
        }

        #endregion

        #region Public Methods

          /// <summary>
        /// Add files to the list, parsing out only ones we use.  Sort when complete
        /// </summary>
        /// <param name="files"></param>
        public void AddFiles(StringCollection files)
        {
            //let the processor handle it for now...
            foreach (string f in files)
            {
                //create a view model for handling these
                IncomingFileViewModel fvm = new IncomingFileViewModel();
                fvm.FileName = f;
                ImageFilesList.Add(fvm);
                try
                {
                    //create a decoder for the image - tells us if this is a supported type
                    BitmapDecoder bd = BitmapDecoder.Create(
                        new Uri(f, UriKind.RelativeOrAbsolute),
                        BitmapCreateOptions.None,
                        BitmapCacheOption.None);  //just trash created - unused due to threading issues
                }
                catch (Exception ex)
                {
                    //log the errors so we can view them in bulk
                    fvm.Error = ex.Message;
                }
            }

            //sort the list
            ImageFilesList.Sort(CompareFileName);
        }

        /// <summary>
        /// Cancels any current processing
        /// </summary>
        public void CancelProcessing()
        {
            this.Converter.CancelProcessing();
        }

        public void SaveSettings()
        {
            BulkImageToPdf.Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Clears any loaded or failed files
        /// </summary>
        public void ClearFiles()
        {
            this.BadFilesList.Clear();
            this.ImageFilesList.Clear();
        }

        public void Cancel()
        {
            this.ClearFiles();
            this.Converter.Reset();
            this.Screens.MoveCurrentToFirst();
        }

        /// <summary>
        /// Process the files in the list on a background worker...
        /// </summary>
        /// <param name="outFile"></param>
        public void ProcessFilesAsync(string outFile)
        {
            this.Converter.ProcessFilesAsync(this.ImageFilesList, outFile);
        }

        public void Convert()
        {
            // Configure open file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".pdf"; // Default file extension
            dlg.Filter = "PDF documents (.pdf)|*.pdf"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                this.ProcessFilesAsync(dlg.FileName);
            }
        }


        #endregion

    }
}
