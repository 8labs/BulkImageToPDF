using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Simple class for wrapping the conversion process and display status
    /// </summary>
    public class ConversionViewModel : ViewModelBase
    {

        #region Private Variables

        private BackgroundWorker currentWorker;

        #endregion

        #region Public Variables

        private int _progress;
        /// <summary>
        /// Current percentage of conversion progress...
        /// </summary>
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }

        private string _status;
        /// <summary>
        /// Current percentage of conversion progress...
        /// </summary>
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        private bool _hasError;
        /// <summary>
        /// Current percentage of conversion progress...
        /// </summary>
        public bool HasError
        {
            get { return _hasError; }
            set
            {
                _hasError = value;
                OnPropertyChanged("HasError");
            }
        }

        private bool _isCompleted;
        /// <summary>
        /// Current percentage of conversion progress...
        /// </summary>
        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                _isCompleted = value;
                OnPropertyChanged("IsCompleted");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Processes the files in the list
        /// </summary>
        private void ProcessFiles(List<IncomingFileViewModel> files, string outFile, BackgroundWorker worker)
        {

            worker.ReportProgress(0, "Starting conversion...");

            using (PdfDocument doc = new PdfDocument())
            {
                //todo let the end user configure some of these
                doc.Info.Title = "";
                doc.Info.Author = "";
                doc.Info.Subject = "";

                //TODO set some others here 
                doc.Info.Elements.SetString("/Producer", "8labs Bulk Image To Pdf converter.");
                doc.Info.Elements.SetString("/Creator", "8labs Bulk Image To Pdf converter.");

                int percent = 0;
                int fileworth = 100 / files.Count;
                //TODO any sorting, etc... or should happen prior to this function
                foreach (IncomingFileViewModel f in files)
                {
                    if (worker.CancellationPending) return; //canceled - exit this

                    //NOTE:  Any errors thrown here should end the conversion
                    BitmapSource bmp = f.Image;
                    //TODO handle page size, orientation, etc...
                    //TODO handle scaling here for better results/speed than the pdf lib scaling??
                    //TODO convert to monochrome or otherwise compress?
                    ImgToPdf.AddImagePage(doc, bmp); //add a page of the image

                    percent += fileworth;
                    worker.ReportProgress(percent, "Processed file: " + Path.GetFileName(f.FileName));
                }

                if (worker.CancellationPending) return; //canceled - exit this

                if (doc.PageCount > 0)
                {
                    //save the output 
                    doc.Save(outFile);
                    worker.ReportProgress(100, "Conversion Complete");
                }
                else
                {
                    //nothing converted or what?
                    worker.ReportProgress(100, "No files to convert");
                }

            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Cancels any current processing
        /// </summary>
        public void CancelProcessing()
        {
            if (this.currentWorker != null) this.currentWorker.CancelAsync();
        }

        /// <summary>
        /// Process the files in the list on a background worker...
        /// </summary>
        /// <param name="outFile"></param>
        public void ProcessFilesAsync(List<IncomingFileViewModel> files, string outFile)
        {
            //create a background worker for the process files method
            this.currentWorker = new BackgroundWorker();
            this.currentWorker.WorkerReportsProgress = true;  //make sure it's set to report progress
            this.currentWorker.WorkerSupportsCancellation = true;  //allow the user to cancel this

            this.currentWorker.DoWork += new DoWorkEventHandler(
                delegate(object s, DoWorkEventArgs args)
                {
                    this.ProcessFiles(files, outFile, this.currentWorker);
                });


            //everything is done
            this.currentWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                delegate(object s, RunWorkerCompletedEventArgs args)
                {
                    if (args.Cancelled)
                    {
                        //return to previous screen?
                        this.Status = "User Canceled";
                        
                    }

                    if (args.Error != null)
                    {
                        //MessageBox.Show(args.Error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Status = args.Error.Message;
                        this.HasError = true;
                    }
                    else
                    {
                        this.Progress = 100;
                        this.Status = "Conversion complete.";
                        this.IsCompleted = true;
                    }

                });

            //status updates
            this.currentWorker.ProgressChanged += new ProgressChangedEventHandler(
                delegate(object s, ProgressChangedEventArgs args)
                {
                    //update status
                    this.Status = args.UserState.ToString();
                    this.Progress = args.ProgressPercentage;
                });

            this.currentWorker.RunWorkerAsync();

        }


        #endregion
    }
}
