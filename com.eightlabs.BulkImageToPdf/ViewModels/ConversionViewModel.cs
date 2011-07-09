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
    public enum Orientation
    {
        Landscape,
        Portrait,
        AutoSelect
    }

    /// <summary>
    /// Simple class for wrapping the conversion process and display status
    /// </summary>
    public class ConversionViewModel : ViewModelBase
    {

        #region Private Variables

        private BackgroundWorker currentWorker;

        private Dictionary<PageSize, double> _paperRatios;

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
        /// determines the paper size from the passed image size
        /// </summary>
        /// <param name="widthToHeight"></param>
        /// <returns></returns>
        private void SetPageSizeByRatio(PdfPage page, double width, double height)
        {
            //collect the ratios if they haven't been already
            if (_paperRatios == null)
            {
                _paperRatios = new Dictionary<PageSize, double>();
                foreach (PageSize p in Enum.GetValues(typeof(PageSize)))
                {
                    if (p != PageSize.Undefined)
                    {
                        page.Size = p;
                        _paperRatios.Add(p, page.Width.Value / page.Height.Value);
                    }
                }
            }

            //determine the best
            PageSize best = PageSize.Letter;
            double ratio = width/height;
            foreach (KeyValuePair<PageSize, double> kvp in _paperRatios)
            {
                //determine if each is a closer match than the default
                if (kvp.Value - ratio >= 0 && kvp.Value - ratio < _paperRatios[best] - ratio)
                    best = kvp.Key;
            }

            page.Size = best;

        }

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

                        //get the orientation
                        Orientation o = Properties.Settings.Default.Rotation;
                        if (o == Orientation.AutoSelect)  //auto select based off image
                        {
                            if (bmp.Height / bmp.Width > bmp.Width / bmp.Height)
                                o = Orientation.Portrait;
                            else
                                o = Orientation.Landscape;
                        }

                        double ratio;
                        if (o == Orientation.Landscape)
                        {
                            page.Rotate = 90;
                            
                            //auto detect paper size on undefined
                            if (Properties.Settings.Default.PaperType == PageSize.Undefined)
                                SetPageSizeByRatio(page, bmp.Height, bmp.Width);
                            else
                                page.Size = Properties.Settings.Default.PaperType;  //one paper size for all converted documents... 

                            ratio = Math.Min(page.Height.Value / bmp.Width, page.Width.Value / bmp.Height);
                        }
                        else
                        {
                            //auto detect paper size on undefined
                            if (Properties.Settings.Default.PaperType == PageSize.Undefined)
                                SetPageSizeByRatio(page, bmp.Width, bmp.Height);
                            else
                                page.Size = Properties.Settings.Default.PaperType;  //one paper size for all converted documents... 

                            ratio = Math.Min(page.Width.Value / bmp.Width, page.Height.Value / bmp.Height);
                        }

                        //convert to monochrome or otherwise compress prior to resizing
                        BitmapSource converted = bmp;
                        if (Properties.Settings.Default.ConvertToMonochrome)
                        {
                            converted = new FormatConvertedBitmap(converted, PixelFormats.BlackWhite, BitmapPalettes.BlackAndWhite, 0);
                        }

                        //handle scaling here for better results/speed than the pdf lib scaling (keep aspect ratio)
                        Transform robotInDisguise = new ScaleTransform(ratio, ratio);
                        BitmapSource optimus = new TransformedBitmap(converted, robotInDisguise);

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

        public void Reset()
        {
            this.Status = "";
            this.Progress = 0;
            this.HasError = false;
        }

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
