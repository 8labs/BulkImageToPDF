using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.eightlabs.WPFCommon.ViewModels;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading.Tasks;

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
        public ConversionViewModel()
        {
            //collect the ratios 
            PdfPage page = new PdfPage();
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
            //determine the best
            PageSize best = PageSize.Letter;
            double ratio = width / height;
            foreach (KeyValuePair<PageSize, double> kvp in _paperRatios)
            {
                //determine if each is a closer match than the default
                if (kvp.Value - ratio >= 0 && kvp.Value - ratio < _paperRatios[best] - ratio)
                    best = kvp.Key;
            }

            page.Size = best;

        }

        /// <summary>
        /// Gets the root path of a group of files 
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private static string FindRootFolder(List<IncomingFileViewModel> files)
        {

            List<string> rootPath = null;

            foreach (IncomingFileViewModel f in files)
            {
                string[] thisPath = Path.GetFullPath(f.Info.FullName).Split(Path.DirectorySeparatorChar);
                if (rootPath == null)
                {
                    rootPath = new List<string>(thisPath);
                }
                else
                {
                    List<string> newRoot = new List<string>();
                    for (int x = 0; x < Math.Min(rootPath.Count, thisPath.Length); x++)
                    {
                        if (rootPath[x] == thisPath[x])
                            newRoot.Add(rootPath[x]);
                        else
                            break;

                    }

                    rootPath = newRoot;

                }

            }

            return String.Join(Path.DirectorySeparatorChar.ToString(), rootPath.ToArray());
        }

        /// <summary>
        /// Sets the author, title, etc... for the document
        /// </summary>
        /// <param name="doc"></param>
        private void SetDocumentMeta(PdfDocument doc)
        {
            //let the end user configure some of these
            doc.Info.Title = Properties.Settings.Default.Title;
            doc.Info.Author = Properties.Settings.Default.Author;
            doc.Info.Subject = Properties.Settings.Default.Subject;
            doc.Info.Keywords = Properties.Settings.Default.Keywords;

            //Set some advertisments here... 
            doc.Info.Elements.SetString("/Producer", "8labs Bulk Image To Pdf converter.  http://www.8labs.com");
            doc.Info.Elements.SetString("/Creator", "8labs Bulk Image To Pdf converter. http://www.8labs.com");
        }

        /// <summary>
        /// Processes the files into individual PDFs
        /// Similar logic to the single pdf
        /// </summary>
        /// <param name="files"></param>
        /// <param name="outFolder"></param>
        /// <param name="worker"></param>
        private void ProcessFilesMultiPdfs(List<IncomingFileViewModel> files, string outFolder, BackgroundWorker worker)
        {
            worker.ReportProgress(0, "Starting conversion...");

            //determine the root folder first
            string rootFolder = FindRootFolder(files);

            float percent = 0;
            float fileworth = 100F / files.Count;

            //sorting will have already happened.. just process
            //and because we're crazy - process them using the Parallel framework
            Parallel.For(0, files.Count, (i, loopState) =>
            {
                IncomingFileViewModel f = files[i];
                using (PdfDocument doc = new PdfDocument())
                {
                    //set the author, title, etc...
                    this.SetDocumentMeta(doc);

                    //skip errored files that the user already knew about...
                    if (!String.IsNullOrEmpty(f.Error)) { return; }

                    if (worker.CancellationPending) loopState.Break(); //canceled 

                    if (i > 4)
                        this.AddFileToDoc(f, doc, true);
                    else
                        this.AddFileToDoc(f, doc, false);

                    if (worker.CancellationPending) loopState.Break(); //canceled 

                    if (doc.PageCount > 0)
                    {
                        //figure out any sub folder handling (remove the root folder, trim any extra slashes, and remove any colons if it has drive letters)
                        string newFolder = "";
                        if (Properties.Settings.Default.RetainFolderStructure)  //only if we're retaining
                            newFolder = f.Info.Directory.FullName.Substring(rootFolder.Length).Trim(Path.DirectorySeparatorChar).Replace(":", "");

                        //combine it with the outFolder
                        newFolder = Path.Combine(
                            outFolder,
                            newFolder);

                        //create the full folder path
                        Directory.CreateDirectory(newFolder);

                        //figure out where the next extension will be.
                        int ext = f.Info.Name.LastIndexOf('.');
                        ext = ext > -1 ? ext : f.Info.Name.Length;  //make sure it had an extension already...
                        string newFileName = f.Info.Name.Substring(0, ext) + ".pdf";

                        //save it out...
                        doc.Save(Path.Combine(newFolder, newFileName));
                    }

                    percent = percent + fileworth;
                    worker.ReportProgress((int)percent, "Processed file: " + Path.GetFileName(f.Info.Name));

                }
            });

            worker.ReportProgress(100, "Conversion Complete");
        }

        /// <summary>
        /// Processes the files in the list
        /// Any failure by any file will halt the entire conversion process
        /// </summary>
        private void ProcessFilesSinglePDF(List<IncomingFileViewModel> files, string outFile, BackgroundWorker worker)
        {

            worker.ReportProgress(0, "Starting conversion...");

            using (PdfDocument doc = new PdfDocument())
            {
                //set the author, title, etc...
                this.SetDocumentMeta(doc);

                float percent = 0;
                float fileworth = 100F / files.Count;
                //sorting will have already happened.. just process
                for (int i = 0; i < files.Count; i++)
                {
                    IncomingFileViewModel f = files[i];

                    //skip errored files that the user already knew about...
                    if (!String.IsNullOrEmpty(f.Error)) { continue; }

                    if (worker.CancellationPending) return; //canceled - exit this

                    if (i > 4)
                        this.AddFileToDoc(f, doc, true);
                    else
                        this.AddFileToDoc(f, doc, false);

                    percent = percent + fileworth;
                    worker.ReportProgress((int)percent, "Processed file: " + Path.GetFileName(f.Info.Name));
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

        /// <summary>
        /// Simple watermark function
        /// http://www.pdfsharp.net/wiki/Watermark-sample.ashx
        /// </summary>
        /// <param name="page"></param>
        /// <param name="watermark"></param>
        /// <param name="font"></param>
        [Conditional("TRIAL")]
        public void AddWaterMark(PdfPage page, string watermark, XFont font)
        {

            // Get an XGraphics object for drawing beneath the existing content
            using (XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
            {

                // Get the size (in point) of the text
                XSize size = gfx.MeasureString(watermark, font);

                // Define a rotation transformation at the center of the page
                double w = page.Width;
                double h = page.Height;
                if (page.Rotate == 90)
                {
                    w = page.Height;
                    h = page.Width;
                }

                gfx.TranslateTransform(w / 2, h / 2);
                gfx.RotateTransform(-Math.Atan(h / w) * 180 / Math.PI);
                gfx.TranslateTransform(-w / 2, -h / 2);

                // Create a graphical path
                XGraphicsPath path = new XGraphicsPath();

                // Add the text to the path
                path.AddString(watermark, font.FontFamily, XFontStyle.BoldItalic, 48,
                  new XPoint((w - size.Width) / 2, (h - size.Height) / 2),
                  XStringFormats.Default);

                // Create a dimmed red pen and brush
                XPen pen = new XPen(XColor.FromArgb(50, 0, 0, 0), 1);
                XBrush brush = new XSolidBrush(XColor.FromArgb(128, 201, 235, 143));

                // Stroke the outline of the path
                gfx.DrawPath(pen, brush, path);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="doc"></param>
        /// <param name="worker"></param>
        private void AddFileToDoc(IncomingFileViewModel file, PdfDocument doc, bool watermark)
        {

            //handle multi page image files (multi frame tiffs)
            List<BitmapSource> imgs = ImgToPdf.GetImagesFromFile(file.Info.FullName);
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

                if (watermark)
                {
                    this.AddWaterMark(page, "http://www.8labs.com", new XFont("Arial", 48));
                }

                using (XGraphics gfx = XGraphics.FromPdfPage(page))
                using (XImage ximg = XImage.FromBitmapSource(optimus))
                {
                    //draw the image full page onto the document (no margins)
                    gfx.DrawImage(ximg, 0, 0, optimus.Width, optimus.Height);  //already scaled
                }

                if (watermark)
                {
                    this.AddWaterMark(page, "http://www.8labs.com", new XFont("Arial", 48));
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
        public void ProcessFilesAsync(List<IncomingFileViewModel> files, string outFile, string outFolder)
        {
            //create a background worker for the process files method
            this.currentWorker = new BackgroundWorker();
            this.currentWorker.WorkerReportsProgress = true;  //make sure it's set to report progress
            this.currentWorker.WorkerSupportsCancellation = true;  //allow the user to cancel this

            this.currentWorker.DoWork += new DoWorkEventHandler(
                delegate(object s, DoWorkEventArgs args)
                {
                    if (outFile != null)
                        this.ProcessFilesSinglePDF(files, outFile, this.currentWorker);
                    else
                        this.ProcessFilesMultiPdfs(files, outFolder, this.currentWorker);
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
