using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.eightlabs.WPFCommon.ViewModels;
using System.Collections.ObjectModel;
using System.IO;

using System.ComponentModel;
using System.Windows.Threading;

using System.Windows.Media;
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
        /// Any failure by any file will halt the entire conversion process
        /// </summary>
        private void ProcessFiles(List<IncomingFileViewModel> files, string outFile, BackgroundWorker worker)
        {

            worker.ReportProgress(0, "Starting conversion...");

            using (PdfDocument doc = new PdfDocument())
            {
                //let the end user configure some of these
                doc.Info.Title = Properties.Settings.Default.Title;
                doc.Info.Author = Properties.Settings.Default.Author;
                doc.Info.Subject = Properties.Settings.Default.Subject;
                doc.Info.Keywords = Properties.Settings.Default.Keywords;

                //Set some advertisments here... 
                doc.Info.Elements.SetString("/Producer", "8labs Bulk Image To Pdf converter.  http://www.8labs.com");
                doc.Info.Elements.SetString("/Creator", "8labs Bulk Image To Pdf converter. http://www.8labs.com");


                float percent = 0;
                float fileworth = 100F / files.Count;
                //sorting will have already happened.. just process
                foreach (IncomingFileViewModel f in files)
                {
                    //skip errored files that the user already knew about...
                    if (!String.IsNullOrEmpty(f.Error)) { continue; }

                    if (worker.CancellationPending) return; //canceled - exit this

                    //handle multi page image files (multi frame tiffs)
                    List<BitmapSource> imgs = ImgToPdf.GetImagesFromFile(f.FileName);
                    foreach (BitmapSource bmp in imgs)
                    {
                        //create the new page with the appropriate paper type and rotation
                        PdfPage page = doc.AddPage();
                        page.Size = Properties.Settings.Default.PaperType;  //one paper size for all converted documents... 
                        //TODO auto detect landscape rotations..
                        double ratio;
                        if (Properties.Settings.Default.Rotation == PageOrientation.Landscape)
                        {
                            page.Rotate = 90;
                            ratio = Math.Min(page.Height / bmp.Width, page.Width / bmp.Height);
                        } else {
                            ratio = Math.Min(page.Width / bmp.Width, page.Height / bmp.Height);
                        }

                        //handle scaling here for better results/speed than the pdf lib scaling (keep aspect ratio)
                        Transform robotInDisguise = new ScaleTransform(ratio, ratio);
                        BitmapSource optimus = new TransformedBitmap(bmp, robotInDisguise);

                        //TODO convert to monochrome or otherwise compress?
                        if (Properties.Settings.Default.ConvertToMonochrome)
                        {
                            optimus = new FormatConvertedBitmap(optimus, PixelFormats.BlackWhite, BitmapPalettes.BlackAndWhite, 0);
                        }


                        using (XGraphics gfx = XGraphics.FromPdfPage(page))
                        using (XImage ximg = XImage.FromBitmapSource(optimus))
                        {
                            //draw the image full page onto the document (no margins)
                            gfx.DrawImage(ximg, 0, 0, optimus.Width, optimus.Height);  //already scaled
                        }
                    }

                    percent = percent + fileworth;
                    worker.ReportProgress((int)percent, "Processed file: " + Path.GetFileName(f.FileName));
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
                        this.HasError = true;
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
                    }
                    this.IsCompleted = true;

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
