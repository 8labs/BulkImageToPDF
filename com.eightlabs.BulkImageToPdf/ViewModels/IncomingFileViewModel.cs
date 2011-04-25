using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public string FileName { get; set; }

        /// <summary>
        /// The image source for this file
        /// </summary>
        public BitmapSource Image { get; set; }

    }
}
