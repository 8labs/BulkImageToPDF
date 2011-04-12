using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using com.eightlabs.WPFCommon.ViewModels;
using System.Collections.ObjectModel;
using System.IO;

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
            this.FilesList = new List<string>();
        }

        #region Private Variables

        #endregion

        #region Public Variables

        public List<string> FilesList { get; set; }

        #endregion

        #region Private Methods

        #endregion

        #region Public Methods

        /// <summary>
        /// Add files to the list, parsing out only ones we use.  Sort when complete
        /// </summary>
        /// <param name="files"></param>
        public void AddFiles(StringCollection files)
        {
            //let the processor handle it for now...
            this.FilesList.AddRange(files.Cast<string>().ToList());
            this.FilesList.Sort();
        }

        /// <summary>
        /// Processes the files in the list
        /// </summary>
        public void ProcessFiles(string outFile)
        {
            using (PdfDocument doc = ImgToPdf.GetDocumentFromFiles(this.FilesList, false))
            {
                //save the output 
                doc.Save(outFile);
            }

        }

        #endregion

        #region Commands

        #endregion
    }
}
