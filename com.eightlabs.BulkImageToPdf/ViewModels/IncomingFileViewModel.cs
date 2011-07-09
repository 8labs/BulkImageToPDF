using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;

namespace com.eightlabs.BulkImageToPdf.ViewModels
{
    /// <summary>
    /// Handles display of a file be converted
    /// </summary>
    public class IncomingFileViewModel
    {
        /// <summary>
        /// Any errors encountered while processing this file
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Actual filename for this file
        /// </summary>
        public FileInfo Info { get; set; }

        /// <summary>
        /// The image source for this file
        /// </summary>
        public BitmapSource Image { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file"></param>
        public IncomingFileViewModel(string file)
        {
            this.Info = new FileInfo(file);

            try
            {
                //create a decoder for the image - tells us if this is a supported type
                BitmapDecoder bd = BitmapDecoder.Create(
                    new Uri(file, UriKind.RelativeOrAbsolute),
                    BitmapCreateOptions.None,
                    BitmapCacheOption.None);  //just trash created - unused due to threading issues
            }
            catch (Exception ex)
            {
                //log the errors so we can view them in bulk
                this.Error = ex.Message;
            }
        }

    }
}
