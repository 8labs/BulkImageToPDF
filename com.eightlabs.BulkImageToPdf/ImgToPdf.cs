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
        public static PdfDocument GetDocumentFromFiles(string[] files)
        {
            PdfDocument doc = new PdfDocument();
            doc.Info.Title = "";

            //TODO set some others here 
            doc.Info.Elements.SetString("/Producer", "8labs Bulk Image To Pdf converter.");
            doc.Info.Elements.SetString("/Creator", "8labs Bulk Image To Pdf converter.");

            //TODO any sorting, etc... or should happen prior to this function
            foreach (string f in files)
            {

                BitmapSource bmp = GetImageFromFile(f);
                //TODO handle scaling here for better results/speed than the pdf lib scaling??
                //TODO convert to monochrome or otherwise compress?
                AddImagePage(doc, bmp); //add a page of the image
            }

            return doc;
        }

        public static BitmapSource GetImageFromFile(string fileName)
        {
            BitmapImage bi = new BitmapImage();
            // BitmapImage.UriSource must be in a BeginInit/EndInit block.
            bi.BeginInit();
            bi.UriSource = new Uri(fileName, UriKind.RelativeOrAbsolute);
            bi.EndInit();

            return bi;
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
