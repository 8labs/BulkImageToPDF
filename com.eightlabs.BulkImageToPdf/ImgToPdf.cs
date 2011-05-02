using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace com.eightlabs.BulkImageToPdf
{
    public static class ImgToPdf
    {
        public static PdfDocument GetDocumentFromFiles(List<string> files, bool throwOnFail)
        {
            PdfDocument doc = new PdfDocument();
            doc.Info.Title = "";

            //TODO set some others here 
            doc.Info.Elements.SetString("/Producer", "8labs Bulk Image To Pdf converter.");
            doc.Info.Elements.SetString("/Creator", "8labs Bulk Image To Pdf converter.");

            //TODO any sorting, etc... or should happen prior to this function
            foreach (string f in files)
            {
                try
                {
                    if (File.Exists(f)) //TODO more verification on what we're working on.
                    {
                        List<BitmapSource> imgs = ImgToPdf.GetImagesFromFile(f);
                        foreach (BitmapSource bmp in imgs)
                        {
                            //TODO handle scaling here for better results/speed than the pdf lib scaling??
                            //TODO convert to monochrome or otherwise compress?
                            AddImagePage(doc, bmp); //add a page of the image
                        }
                    }
                }
                catch (Exception)
                {
                    //only throw if specified
                    if (throwOnFail) throw;
                }

            }

            return doc;
        }

        /// <summary>
        /// Returns a list of images in the file - (Usually just one, except for multi page tiffs)
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<BitmapSource> GetImagesFromFile(string fileName)
        {
            BitmapDecoder bd = BitmapDecoder.Create(
                new Uri(fileName, UriKind.RelativeOrAbsolute),
                BitmapCreateOptions.None,
                BitmapCacheOption.None);
            
            List<BitmapSource> imgs = new List<BitmapSource>();
            foreach (BitmapFrame frame in bd.Frames)
            {               
                frame.Freeze();  //allows reading on all threads
                imgs.Add((BitmapSource)frame);
            }

            return imgs;
        }


        public static void AddImagePage(PdfDocument doc, BitmapSource img)
        {
            PdfPage page = doc.AddPage();
 
            //TODO we need to figure out landscape handling - auto detect or what...
            //page.Rotate

            using (XGraphics gfx = XGraphics.FromPdfPage(page))
            using (XImage ximg = XImage.FromBitmapSource(img))
            {
                //draw the image full page onto the document (no margins)
                gfx.DrawImage(ximg, 0, 0, page.Width, page.Height);
            }
        }

    }
}
