using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using com.eightlabs.WPFCommon.ViewModels;
using System.Collections.ObjectModel;
using System.IO;

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

        #endregion

        #region Private Methods

        //comparison for the file view models
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
                try
                {
                    BitmapSource bmp = ImgToPdf.GetImageFromFile(f);
                    fvm.Image = bmp;
                    ImageFilesList.Add(fvm);
                }
                catch (Exception ex)
                {
                    //log the errors so we can view them in bulk
                    fvm.Error = ex.Message;
                    BadFilesList.Add(fvm);
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

        /// <summary>
        /// Clears any loaded or failed files
        /// </summary>
        public void ClearFiles()
        {
            this.BadFilesList.Clear();
            this.ImageFilesList.Clear();
        }

        /// <summary>
        /// Process the files in the list on a background worker...
        /// </summary>
        /// <param name="outFile"></param>
        public void ProcessFilesAsync(string outFile)
        {
            this.Converter.ProcessFilesAsync(this.ImageFilesList, outFile);
        }


        #endregion

    }
}
