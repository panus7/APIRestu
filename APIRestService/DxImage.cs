using DevExpress.Pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ZXing;
using ZXing.QrCode;

namespace APIRestServiceRestaurant
{
    public class DxImage
    {  
        private static Random random = new Random();

        public static string QRCodeToText(string imageQRBase64)
        {
            try
            {

                IBarcodeReader reader = new BarcodeReader();
                var bArray = ConvertBase64ImageToByteArray(imageQRBase64, false);
                MemoryStream ms = new MemoryStream(bArray);
                Bitmap bmp = (Bitmap)Image.FromStream(ms);
                var result = reader.Decode(bmp);
                return result.Text;

            }
            catch (Exception e)
            { }

            return string.Empty;

        }

        public static Stream QRCodeGenerateStream(string strTextGenQRCode)
        {
            byte[] Byresult;
            QrCodeEncodingOptions options = new QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 300,
                Height = 300,
            };

            var qr = new ZXing.BarcodeWriter();
            qr.Options = options;
            qr.Format = ZXing.BarcodeFormat.QR_CODE;

            var result = new Bitmap(qr.Write(strTextGenQRCode));

            MemoryStream ms = new MemoryStream();
            result.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            return ms;
        }

        public static byte[] QRCodeGenerate(string strTextGenQRCode)
        {
            byte[] Byresult;
            QrCodeEncodingOptions options = new QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 300,
                Height = 300,
            };

            var qr = new ZXing.BarcodeWriter();
            qr.Options = options;
            qr.Format = ZXing.BarcodeFormat.QR_CODE;

            var result = new Bitmap(qr.Write(strTextGenQRCode));

            MemoryStream ms = new MemoryStream();
            {
                result.Save(ms, ImageFormat.Jpeg);
                Byresult = ms.ToArray();
            }

            return CropWhiteSpace(Byresult);
        }


        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
 
     
        public static Bitmap ReductNois(Bitmap Image)
        {
            int Size = 2;
            System.Drawing.Bitmap TempBitmap = Image;
            System.Drawing.Bitmap NewBitmap = new System.Drawing.Bitmap(TempBitmap.Width, TempBitmap.Height);
            System.Drawing.Graphics NewGraphics = System.Drawing.Graphics.FromImage(NewBitmap);
            NewGraphics.DrawImage(TempBitmap, new System.Drawing.Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height), new System.Drawing.Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height), System.Drawing.GraphicsUnit.Pixel);
            NewGraphics.Dispose();
            Random TempRandom = new Random();
            int ApetureMin = -(Size / 2);
            int ApetureMax = (Size / 2);
            for (int x = 0; x < NewBitmap.Width; ++x)
            {
                for (int y = 0; y < NewBitmap.Height; ++y)
                {
                    List<int> RValues = new List<int>();
                    List<int> GValues = new List<int>();
                    List<int> BValues = new List<int>();
                    for (int x2 = ApetureMin; x2 < ApetureMax; ++x2)
                    {
                        int TempX = x + x2;
                        if (TempX >= 0 && TempX < NewBitmap.Width)
                        {
                            for (int y2 = ApetureMin; y2 < ApetureMax; ++y2)
                            {
                                int TempY = y + y2;
                                if (TempY >= 0 && TempY < NewBitmap.Height)
                                {
                                    Color TempColor = TempBitmap.GetPixel(TempX, TempY);
                                    RValues.Add(TempColor.R);
                                    GValues.Add(TempColor.G);
                                    BValues.Add(TempColor.B);
                                }
                            }
                        }
                    }
                    RValues.Sort();
                    GValues.Sort();
                    BValues.Sort();
                    Color MedianPixel = Color.FromArgb(RValues[RValues.Count / 2],
                        GValues[GValues.Count / 2],
                        BValues[BValues.Count / 2]);
                    NewBitmap.SetPixel(x, y, MedianPixel);
                }
            }
            return NewBitmap;
        }

        public static object CreateImgThumbnail(string strBase64Image) {
            return CreateImgThumbnail(ConvertBase64ImageToByteArray(strBase64Image,false));
        }

        public static object CreateImgThumbnail(byte[] bImgArryOrg)
        {
            Size x_SizeThumbnail = new Size(120, 150);
            return CreateImgThumbnail(bImgArryOrg, x_SizeThumbnail);
        }

        public static object CreateImgThumbnail(byte[] bImgArryOrg, Size size)
        {
            try
            {
                using (MemoryStream MsImg = new MemoryStream(bImgArryOrg))
                {
                    Image imgToResize = Image.FromStream(MsImg);
                    ////
                    int sourceWidth = imgToResize.Width;
                    int sourceHeight = imgToResize.Height;

                    float nPercent = 0;
                    float nPercentW = 0;
                    float nPercentH = 0;

                    nPercentW = ((float)size.Width / (float)sourceWidth);
                    nPercentH = ((float)size.Height / (float)sourceHeight);

                    if (nPercentH < nPercentW)
                        nPercent = nPercentH;
                    else
                        nPercent = nPercentW;

                    using (MemoryStream msThumb = new MemoryStream())
                    {
                        int destWidth = (int)(sourceWidth * nPercent);
                        int destHeight = (int)(sourceHeight * nPercent);

                        Bitmap b = new Bitmap(destWidth, destHeight);
                        Graphics g = Graphics.FromImage((Image)b);
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                        g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
                        g.Dispose();

                        b.Save(msThumb, ImageFormat.Jpeg);

                        return msThumb.ToArray();
                    }
                }
            }
            catch (Exception)    //Exception ex
            { }

            return DBNull.Value;
        }

        public static Byte[] getPDFThumbnail(Byte[] bPdfArray) //$$$$@@@@
        {
            Byte[] bThumbnail = null;

            if (null == bPdfArray || bPdfArray.Length < 100)
                return bThumbnail;

            using (MemoryStream msPDF = new MemoryStream(bPdfArray))
            {
                PdfDocumentProcessor pdf = new PdfDocumentProcessor();
                pdf.LoadDocument(msPDF);

                int pageCount = pdf.Document.Pages.Count;
                for (int i = 0; i < pageCount; i++)
                {
                    if (null == bThumbnail)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            Bitmap bmpThumbnail = pdf.CreateBitmap(i + 1, 180);
                            bmpThumbnail.Save(ms, ImageFormat.Jpeg);
                            bThumbnail = ms.ToArray();
                            break;
                        }
                    }
                }
            }

            return bThumbnail;
        }


        public static Byte[] getPDFThumbnail(String strPDFBase64)
        {
            Byte[] bThumbnail = null;

            if (null == strPDFBase64 || strPDFBase64.Length < 100)
                return bThumbnail;

            var PDFbyArr = ConvertBase64PDFToByteArray(strPDFBase64);
            if (null == PDFbyArr)
                return null;

            using (MemoryStream msPDF = new MemoryStream(PDFbyArr))
            {
                PdfDocumentProcessor pdf = new PdfDocumentProcessor();
                pdf.LoadDocument(msPDF);

                int pageCount = pdf.Document.Pages.Count;
                for (int i = 0; i < pageCount; i++)
                {
                    if (null == bThumbnail)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            Bitmap bmpThumbnail = pdf.CreateBitmap(i + 1 , 180);
                            bmpThumbnail.Save(ms, ImageFormat.Jpeg);
                            bThumbnail = ms.ToArray();
                            break;
                        }
                    }
                }
            }

            return bThumbnail;
        }


        public static Byte[] convertPDFToJpeg(byte[] PDFByteArr, out Byte[] bThumbnail) 
        {
            byte[] bJpgDocArray = null;
            bThumbnail = null;

            string strTextInPdf = string.Empty;

            if (null == PDFByteArr || PDFByteArr.Length < 100)
                return bJpgDocArray;

            using (MemoryStream msPDF = new MemoryStream(PDFByteArr))
            {
                PdfDocumentProcessor pdf = new PdfDocumentProcessor();
                pdf.LoadDocument(msPDF);

                int pageCount = pdf.Document.Pages.Count;
                for (int i = 0; i < pageCount; i++)
                { 
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Bitmap bmpDocument = pdf.CreateBitmap(i + 1, 2400);
                        bmpDocument.Save(ms, ImageFormat.Jpeg);
                        bJpgDocArray = ms.ToArray();
                    }

                    using (MemoryStream msThumb = new MemoryStream())
                    {
                        Bitmap bmpThumbnail = pdf.CreateBitmap(i + 1, 180);
                        bmpThumbnail.Save(msThumb, ImageFormat.Jpeg);
                        bThumbnail = msThumb.ToArray();
                    }

                    break; //First Page
                }
            }

            return bJpgDocArray;
        }
         

        //public static List<String> convertPDFToListJpg(byte[] PDFByteArr)
        //{
        //    List<String> ListOfImgBaste64Array = new List<String>();


        //    string strTextInPdf = string.Empty;

        //    if (null == PDFByteArr || PDFByteArr.Length < 100)
        //        return new List<String>();

        //    int largestEdgeLength = 1800;

        //    using (MemoryStream msPDF = new MemoryStream(PDFByteArr))
        //    {

        //        PdfDocumentProcessor pdf = new PdfDocumentProcessor();
        //        pdf.LoadDocument(msPDF);

        //        int pageCount = pdf.Document.Pages.Count;
        //        for (int i = 0; i < pageCount; i++)
        //        {
        //            using (MemoryStream ms = new MemoryStream())
        //            {
        //                Bitmap bmpDocument = pdf.CreateBitmap(i + 1 , DevExpress.Office.Utils.Units.PointsToPixels(largestEdgeLength, 72));
        //                bmpDocument.Save(ms, ImageFormat.Jpeg);
        //                ListOfImgBaste64Array.Add(DxImage.ConvertByteArrayToBase64(ms.ToArray()));

        //            }

        //        }
        //    }

        //    return ListOfImgBaste64Array;
        //}

        public static List<Byte[]> convertPDFToListJpg(byte[] PDFByteArr, out Byte[] bThumbnail)
        {
            List<Byte[]> ListOfImgArray = new List<byte[]>();

            bThumbnail = null;

            string strTextInPdf = string.Empty;

            if (null == PDFByteArr || PDFByteArr.Length < 100)
                return new List<byte[]>();

            int largestEdgeLength = 2800;

            using (MemoryStream msPDF = new MemoryStream(PDFByteArr))
            {

                PdfDocumentProcessor pdf = new PdfDocumentProcessor();
                pdf.LoadDocument(msPDF);

                int pageCount = pdf.Document.Pages.Count;
                for (int i = 0; i < pageCount; i++)
                {
                    if (i == 0)
                    {
                        using (MemoryStream msThumb = new MemoryStream())
                        {
                            Bitmap bmpThumbnail = pdf.CreateBitmap(i + 1, 240);
                            bmpThumbnail.Save(msThumb, ImageFormat.Jpeg);
                            bThumbnail = msThumb.ToArray();
                        }
                    }
                     
                    using (MemoryStream ms = new MemoryStream())
                    { 
                        int iHeight = (int)pdf.Document.Pages[i].MediaBox.Height;
                        int iWidth = (int)pdf.Document.Pages[i].MediaBox.Width;

                        largestEdgeLength = iWidth;
                        if (iHeight > iWidth) largestEdgeLength = iHeight;

                        Bitmap bmpDocument = pdf.CreateBitmap(i + 1, largestEdgeLength);
                        bmpDocument.Save(ms, ImageFormat.Jpeg);
                        ListOfImgArray.Add(ms.ToArray());
                    }

                    //break; //First Page
                }
            }

            return ListOfImgArray;
        }

        public static List<Byte[]> convertPDFToListJpg(byte[] PDFByteArr, out Byte[] bThumbnail, int largestEdgeLength)
        {
            bool bCompress = false;
            return convertPDFToListJpg(PDFByteArr, out bThumbnail, largestEdgeLength, bCompress);
        }
        public static List<Byte[]> convertPDFToListJpg(byte[] PDFByteArr, out Byte[] bThumbnail , int largestEdgeLength, bool bCompress)
        {
            List<Byte[]> ListOfImgArray = new List<byte[]>();

            bThumbnail = null;

            string strTextInPdf = string.Empty;

            if (null == PDFByteArr || PDFByteArr.Length < 100)
                return new List<byte[]>();

            if(largestEdgeLength == 0) largestEdgeLength = 2400;

            using (MemoryStream msPDF = new MemoryStream(PDFByteArr))
            {

                PdfDocumentProcessor pdf = new PdfDocumentProcessor();
                pdf.LoadDocument(msPDF);

                int pageCount = pdf.Document.Pages.Count;
                for (int i = 0; i < pageCount; i++)
                {
                    if (i == 0)
                    {
                        using (MemoryStream msThumb = new MemoryStream())
                        {
                            Bitmap bmpThumbnail = pdf.CreateBitmap(i+1, 300);
                            bmpThumbnail.Save(msThumb, ImageFormat.Jpeg);
                            bThumbnail = msThumb.ToArray();
                        }
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        //Bitmap bmpDocument = pdf.CreateBitmap(i, largestEdgeLength);
                        //PNS Bitmap bmpDocument = pdf.CreateBitmap(i, DevExpress.Office.Utils.Units.PointsToPixels(largestEdgeLength, 300));
                        int iPointsToPixels = DevExpress.Office.Utils.Units.PointsToPixels(largestEdgeLength, 300);
                        Bitmap bmpDocument = pdf.CreateBitmap(i +1 , iPointsToPixels);
                        bmpDocument.Save(ms, ImageFormat.Jpeg);                        
                        if (bCompress)
                        {
                            ListOfImgArray.Add(DxImage.DevCompress(ms.ToArray()));
                        }
                        else
                        {
                            ListOfImgArray.Add(ms.ToArray());
                        }
                    }

                    //break; //First Page
                }
            }

            return ListOfImgArray;
        }

        public static Byte[] convertPDFToBmp(byte[] PDFByteArr, out Byte[] bThumbnail)
        {
            return convertPDFToBmp(PDFByteArr, 2400, out bThumbnail);
        }

        public static Byte[] convertPDFToBmp(byte[] PDFByteArr, int largestEdgeLength, out Byte[] bThumbnail) //$$$$@@@@
        {
            byte[] bJpgDocArray = null;
            bThumbnail = null;

            string strTextInPdf = string.Empty;

            if (null == PDFByteArr || PDFByteArr.Length < 100)
                return bJpgDocArray;

            using (MemoryStream msPDF = new MemoryStream(PDFByteArr))
            {

                PdfDocumentProcessor pdf = new PdfDocumentProcessor();
                pdf.LoadDocument(msPDF);

                int pageCount = pdf.Document.Pages.Count;
                for (int i = 0; i < pageCount; i++)
                {
                    ///strTextInPdf += "," + pdf.GetPageText(i);
                    ///
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Bitmap bmpDocument = pdf.CreateBitmap(i +1 , largestEdgeLength);
                        bmpDocument.Save(ms, ImageFormat.Bmp);
                        bJpgDocArray = ms.ToArray();
                    }

                    using (MemoryStream msThumb = new MemoryStream())
                    {
                        Bitmap bmpThumbnail = pdf.CreateBitmap(i +1 , 180);
                        bmpThumbnail.Save(msThumb, ImageFormat.Jpeg);
                        bThumbnail = msThumb.ToArray();
                    }

                    break; //First Page
                }
            }

            return bJpgDocArray;
        }

        public static Image getImage(string strOriginalPath)
        {
            try
            {
                if (File.Exists(strOriginalPath))
                {
                    Image img;
                    using (FileStream stream = new FileStream(strOriginalPath, FileMode.Open, FileAccess.Read))
                    {
                        img = Image.FromStream(stream);
                    }
                    return img;
                }
            }
            catch (Exception)   //Exception ex
            {
                //MessageBox.Show(ex.Message.ToString());
            }
            return null;
        }

        public static byte[] getImageToByteArray(System.Drawing.Image imageIn)
        {
            if (null != imageIn)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Bitmap bm = new Bitmap(imageIn);
                    bm.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    return ms.ToArray();
                }
            }
            else
            {
                return null;
            }
        }

        public static byte[] getImageToJpgByteArray(System.Drawing.Image imageIn)
        {
            if (null != imageIn)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Bitmap bm = new Bitmap(imageIn);
                    bm.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return ms.ToArray();
                }
            }
            else
            {
                return null;
            }
        }

        public static Byte[] getImageByteArray(string strOriginalPath)
        {
            try
            {
                if (File.Exists(strOriginalPath))
                {
                    Image img;
                    using (FileStream stream = new FileStream(strOriginalPath, FileMode.Open, FileAccess.Read))
                    {
                        img = Image.FromStream(stream);
                    }
                    using (MemoryStream ms = new MemoryStream())
                    {
                        img.Save(ms, ImageFormat.Bmp);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception)    //Exception ex
            {
                //MessageBox.Show(ex.Message.ToString());
            }
            return null;
        }

        public static List<byte[]> getImageTiffToPng(string fileName)
        {
            List<byte[]> bmpLst = new List<byte[]>();
            using (Image imageFile = Image.FromFile(fileName))
            {
                FrameDimension frameDimensions = new FrameDimension(
                    imageFile.FrameDimensionsList[0]);

                int frameNum = imageFile.GetFrameCount(frameDimensions);
                //string[] jpegPaths = new string[frameNum];

                for (int frame = 0; frame < frameNum; frame++)
                {
                    imageFile.SelectActiveFrame(frameDimensions, frame);
                    using (Bitmap bmp = new Bitmap(imageFile))
                    {
                        MemoryStream msNew = new MemoryStream();
                        //jpegPaths[frame] = String.Format("{0}\\{1}{2}.jpg",
                        //    Path.GetDirectoryName(fileName),
                        //    Path.GetFileNameWithoutExtension(fileName),
                        //    frame);
                        bmp.Save(msNew, ImageFormat.Png);
                        bmpLst.Add(msNew.ToArray());
                    }
                }

                return bmpLst;
            }

        }

        public static Image FixedSize(Image imgPhoto, int Width, int Height) //
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width - (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height - (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height,PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Transparent);
            grPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        public static string FixedSizeAndConvertToBase64(Image imgPhoto, int Width, int Height) //
        {
            Bitmap bmp = (Bitmap)FixedSize(imgPhoto, Width, Height);
            return ConvertImageToBase64((Image)bmp);
        }

        public static Byte[] FixedSizeAndConvertToByteArray(Image imgPhoto, int Width, int Height) //
        {
            Bitmap bmp = (Bitmap)FixedSize(imgPhoto, Width, Height);
            return getImageToByteArray((Image)bmp);
        }

        public static string ConvertImgToBase64(string strOriginalPath) //
        {
            try
            {
                if (File.Exists(strOriginalPath))
                {
                    FileInfo ff = new FileInfo(strOriginalPath);
                    string exten = ff.Extension;
                    exten = exten.Replace(".", "");
                    Image img;
                    using (FileStream stream = new FileStream(strOriginalPath, FileMode.Open, FileAccess.Read))
                    {
                        img = Image.FromStream(stream);

                        using (MemoryStream m = new MemoryStream())
                        {
                            // img.Save(m, img.RawFormat);
                            img.Save(m, ImageFormat.Png);
                            byte[] imageBytes = m.ToArray();

                            string base64String = Convert.ToBase64String(imageBytes);
                            //data:image/png;base64,
                            return string.Format("data:image/{0};base64,{1}", exten.ToLower(), base64String);
                        }
                    }
                }
            }
            catch (Exception)    //Exception ex
            {

            }
            return string.Empty;
        }

        public static string ConvertImageToBase64(Image img) //
        {
            try
            {
                //if (byArrayImage.Length > 20)
                {
                    //using (MemoryStream stream = new MemoryStream(byArrayImage))
                    {
                        //img = Image.FromStream(stream);
                        using (MemoryStream m = new MemoryStream())
                        {
                            //img.Save(m, ImageFormat.Png);
                            byte[] imageBytes = m.ToArray();
                            ///
                            ///Image imgToResize = Image.FromStream(img);
                            ///
                            int sourceWidth = img.Width;
                            int sourceHeight = img.Height;
                              
                            int destWidth = (int)(sourceWidth / 2);
                            int destHeight = (int)(sourceHeight / 2);

                            Bitmap b = new Bitmap(destWidth, destHeight);
                            Graphics g = Graphics.FromImage((Image)b);
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                            g.DrawImage(img, 0, 0, destWidth, destHeight);
                            g.Dispose();

                            b.Save(m, ImageFormat.Png);

                            string base64String = Convert.ToBase64String(m.ToArray());
                            //data:image/png;base64,
                            return string.Format("data:image/{0};base64,{1}", "png", base64String);
                        }
                    }
                }
            }
            catch (Exception ex)    //Exception ex
            {
                throw new Exception("Error in ConvertImageToBase64 : " + ex.Message);
            }
            return string.Empty;
        }

        public static byte[] ConvertImageToByteArray(Image img) //
        {
            return ConvertImageToByteArray(img, ImageFormat.Png);
        }

        public static byte[] ConvertImageToByteArray(Image img , ImageFormat xImageFormat) //
        {
            try
            {
                using (MemoryStream m = new MemoryStream())
                {
                    img.Save(m, xImageFormat);
                    return m.ToArray();
                }
            }
            catch (Exception ex)    //Exception ex
            {
                throw new Exception("Error in ConvertImageToBase64 : " + ex.Message);
            }
            return null;
        }

        public static Image ConvertByteArrayImage(byte[] byArrayImage) //
        {
            try
            {
                if (byArrayImage.Length > 20)
                {
                    Image img;
                    using (MemoryStream stream = new MemoryStream(byArrayImage))
                    {
                        return Image.FromStream(stream);
                         
                    }
                }
            }
            catch (Exception)    //Exception ex
            {

            }
            return null;
        }

        public static string ConvertByteArrayImageToBase64(byte[] byArrayImage) //
        {
            try
            {
                if (byArrayImage.Length > 20)
                {
                    Image img;
                    using (MemoryStream stream = new MemoryStream(byArrayImage))
                    {
                        img = Image.FromStream(stream);
                        using (MemoryStream m = new MemoryStream())
                        {
                            img.Save(m, img.RawFormat);
                            byte[] imageBytes = m.ToArray();

                            string base64String = Convert.ToBase64String(imageBytes);
                            //data:image/png;base64,
                            return string.Format("data:image/{0};base64,{1}", "png", base64String);
                        }
                    }
                }
            }
            catch (Exception)    //Exception ex
            {

            }
            return string.Empty;
        }

        public static string ConvertByteArrayToBase64(byte[] byArrayData) //
        {
            try
            {
                return Convert.ToBase64String(byArrayData);
            }
            catch (Exception)    //Exception ex
            {

            }
            return string.Empty;
        }

        public static byte[] ConvertBase64PDFToByteArray(string base64String)
        {
            try
            {
                byte[] PDFBytes = new byte[0];

                if (!string.IsNullOrEmpty(base64String))
                {
                    string strTemplate = "data:application/pdf;base64,";

                    if (base64String.Contains(strTemplate))
                        base64String = base64String.Replace(strTemplate, "");

                    PDFBytes = Convert.FromBase64String(base64String);
                }

                return PDFBytes;
            }
            catch (Exception e)
            {
            }

            return null;
        }

        public static byte[] ConvertBase64ImageToByteArray(string base64String, bool bCropWhiteSpace)
        {
            try
            {
                byte[] imageBytes = new byte[0];

                if (!string.IsNullOrEmpty(base64String))
                {
                    List<string> listofTemplateReplace = new List<string>();

                    listofTemplateReplace.Add("data:image/jpeg;base64,");
                    listofTemplateReplace.Add("data:image/png;base64,");

                    foreach (string strTempItem in listofTemplateReplace)
                    {
                        if (base64String.Contains(strTempItem))
                        {
                            base64String = base64String.Replace(strTempItem, "");
                        }
                    }

                    imageBytes = Convert.FromBase64String(base64String);

                    if (bCropWhiteSpace)
                    {
                        return CropWhiteSpace(imageBytes);
                    }
                }

                return imageBytes;
            }
            catch (Exception e)
            {
            }

            return null;
        }

        public static string getTextInPDFAndThumbnail(string strPDFFullPath, out Byte[] bPdfArray, out Byte[] bThumbnail)
        {
            bPdfArray = null;
            bThumbnail = null;

            string strTextInPdf = string.Empty;

            if (!File.Exists(strPDFFullPath))
                return string.Empty;

            bPdfArray = System.IO.File.ReadAllBytes(strPDFFullPath);

            PdfDocumentProcessor pdf = new PdfDocumentProcessor();
            //<@>pdf.LoadDocument(strPDFFullPath);            
            pdf.LoadDocument(new MemoryStream(bPdfArray));

            int pageCount = pdf.Document.Pages.Count;
            for (int i = 0; i < pageCount; i++)
            {
                strTextInPdf += "," + pdf.GetPageText(i);
                ///
                if (null == bThumbnail)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Bitmap bmpThumbnail = pdf.CreateBitmap(i+1, 180);
                        bmpThumbnail.Save(ms, ImageFormat.Jpeg);
                        bThumbnail = ms.ToArray();
                    }
                }
            }

            return strTextInPdf;
        }

        #region CropUnwantedBackground
        public static Bitmap CropUnwantedBackground(Bitmap bmp)
        {
            var backColor = GetMatchedBackColor(bmp);
            if (backColor.HasValue)
            {
                var bounds = GetImageBounds(bmp, backColor);
                var diffX = bounds[1].X - bounds[0].X + 1;
                var diffY = bounds[1].Y - bounds[0].Y + 1;
                var croppedBmp = new Bitmap(diffX, diffY);
                var g = Graphics.FromImage(croppedBmp);
                var destRect = new Rectangle(0, 0, croppedBmp.Width, croppedBmp.Height);
                var srcRect = new Rectangle(bounds[0].X, bounds[0].Y, diffX, diffY);
                g.DrawImage(bmp, destRect, srcRect, GraphicsUnit.Pixel);
                bmp.Dispose();
                return croppedBmp;
            }
            else
            {
                bmp.Dispose();
                return null;
            }
        }
        #endregion

        #region Private Methods

        #region GetImageBounds
        private static Point[] GetImageBounds(Bitmap bmp, Color? backColor)
        {
            //--------------------------------------------------------------------
            // Finding the Bounds of Crop Area bu using Unsafe Code and Image Proccesing
            Color c;
            int width = bmp.Width, height = bmp.Height;
            bool upperLeftPointFounded = false;
            var bounds = new Point[2];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    c = bmp.GetPixel(x, y);
                    bool sameAsBackColor = ((c.R <= backColor.Value.R * 1.1 && c.R >= backColor.Value.R * 0.9) &&
                                            (c.G <= backColor.Value.G * 1.1 && c.G >= backColor.Value.G * 0.9) &&
                                            (c.B <= backColor.Value.B * 1.1 && c.B >= backColor.Value.B * 0.9));
                    if (!sameAsBackColor)
                    {
                        if (!upperLeftPointFounded)
                        {
                            bounds[0] = new Point(x, y);
                            bounds[1] = new Point(x, y);
                            upperLeftPointFounded = true;
                        }
                        else
                        {
                            if (x > bounds[1].X)
                                bounds[1].X = x;
                            else if (x < bounds[0].X)
                                bounds[0].X = x;
                            if (y >= bounds[1].Y)
                                bounds[1].Y = y;
                        }
                    }
                }
            }
            return bounds;
        }
        #endregion

        #region GetMatchedBackColor
        private static Color? GetMatchedBackColor(Bitmap bmp)
        {
            // Getting The Background Color by checking Corners of Original Image
            var corners = new Point[]{
            new Point(0, 0),
            new Point(0, bmp.Height - 1),
            new Point(bmp.Width - 1, 0),
            new Point(bmp.Width - 1, bmp.Height - 1)
        }; // four corners (Top, Left), (Top, Right), (Bottom, Left), (Bottom, Right)
            for (int i = 0; i < 4; i++)
            {
                var cornerMatched = 0;
                var backColor = bmp.GetPixel(corners[i].X, corners[i].Y);
                for (int j = 0; j < 4; j++)
                {
                    var cornerColor = bmp.GetPixel(corners[j].X, corners[j].Y);// Check RGB with some offset
                    if ((cornerColor.R <= backColor.R * 1.1 && cornerColor.R >= backColor.R * 0.9) &&
                        (cornerColor.G <= backColor.G * 1.1 && cornerColor.G >= backColor.G * 0.9) &&
                        (cornerColor.B <= backColor.B * 1.1 && cornerColor.B >= backColor.B * 0.9))
                    {
                        cornerMatched++;
                    }
                }
                if (cornerMatched > 2)
                {
                    return backColor;
                }
            }
            return null;
        }
        #endregion

        #endregion
    

        public static Byte[] CropBlackSpace(Byte[] bImgArr)
        {
            if (null == bImgArr) return bImgArr;

            using (MemoryStream MsImg = new MemoryStream(bImgArr))
            {
                Bitmap bmp = (Bitmap)Image.FromStream(MsImg);
                var bmpNew = CropUnwantedBackground(bmp);
                if (null != bmpNew)
                {
                    return DxImage.ConvertImageToByteArray(bmpNew);
                }
                else
                {
                    return bImgArr;
                }
            }

            return null;
        }

        public static Byte[] CropWhiteSpace(Byte[] bImgArr)
        {
            if (null == bImgArr) return bImgArr;

            using (MemoryStream MsImg = new MemoryStream(bImgArr))
            {
                Bitmap bmp = (Bitmap)Image.FromStream(MsImg);
                ///
                int w = bmp.Width;
                int h = bmp.Height;
                int white = 0xffffff;

                Func<int, bool> allWhiteRow = r =>
                {
                    for (int i = 0; i < w; ++i)
                        if ((bmp.GetPixel(i, r).ToArgb() & white) != white)
                            return false;
                    return true;
                };

                Func<int, bool> allWhiteColumn = c =>
                {
                    for (int i = 0; i < h; ++i)
                        if ((bmp.GetPixel(c, i).ToArgb() & white) != white)
                            return false;
                    return true;
                };

                int topmost = 0;
                for (int row = 0; row < h; ++row)
                {
                    if (!allWhiteRow(row))
                        break;
                    topmost = row;
                }

                int bottommost = 0;
                for (int row = h - 1; row >= 0; --row)
                {
                    if (!allWhiteRow(row))
                        break;
                    bottommost = row;
                }

                int leftmost = 0, rightmost = 0;
                for (int col = 0; col < w; ++col)
                {
                    if (!allWhiteColumn(col))
                        break;
                    leftmost = col;
                }

                for (int col = w - 1; col >= 0; --col)
                {
                    if (!allWhiteColumn(col))
                        break;
                    rightmost = col;
                }

                if (rightmost == 0) rightmost = w; // As reached left
                if (bottommost == 0) bottommost = h; // As reached top.

                int croppedWidth = rightmost - leftmost;
                int croppedHeight = bottommost - topmost;

                if (croppedWidth == 0) // No border on left or right
                {
                    leftmost = 0;
                    croppedWidth = w;
                }

                if (croppedHeight == 0) // No border on top or bottom
                {
                    topmost = 0;
                    croppedHeight = h;
                }

                try
                {
                    var target = new Bitmap(croppedWidth, croppedHeight);
                    using (Graphics g = Graphics.FromImage(target))
                    {
                        g.DrawImage(bmp,
                          new RectangleF(0, 0, croppedWidth, croppedHeight),
                          new RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                          GraphicsUnit.Pixel);
                    }

                    MemoryStream newMem = new MemoryStream();
                    target.Save(newMem, ImageFormat.Png);
                    return newMem.ToArray();
                    //return target;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                      string.Format("Values are topmost={0} btm={1} left={2} right={3} croppedWidth={4} croppedHeight={5}", topmost, bottommost, leftmost, rightmost, croppedWidth, croppedHeight),
                      ex);
                }
            }

            return null;
        }

        public static DataTable getDataCheckBoxInDevExpressPDF(byte[] byArr, out string strErrorMsg)
        {
            strErrorMsg = string.Empty;

            DataTable tbResult = new DataTable();
            tbResult.Columns.Add("CheckBoxColumnName");
            tbResult.Columns.Add("CheckBoxColumnValue");

            if (null == byArr || byArr.Length < 100)
            {
                strErrorMsg ="Invalid data!";
                return tbResult;
            }

            try
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                using (PdfDocumentProcessor pdfProcessor = new PdfDocumentProcessor())
                {
                    pdfProcessor.LoadDocument(new MemoryStream(byArr));

                    if (pdfProcessor.Document == null)
                        pdfProcessor.CreateEmptyDocument();

                    var formdata = pdfProcessor.GetFormData();
                    DataRow newrow;
                    foreach (string strFieldname in pdfProcessor.GetFormFieldNames())
                    {
                        if (null != formdata[strFieldname].Value)
                        {
                            if (formdata[strFieldname].Value.ToString().ToUpper() == "YES"
                                || formdata[strFieldname].Value.ToString().ToUpper() == "OK"
                                || formdata[strFieldname].Value.ToString().ToUpper() == "ON")
                            {
                                newrow = tbResult.NewRow();
                                newrow["CheckBoxColumnName"] = strFieldname;
                                newrow["CheckBoxColumnValue"] = formdata[strFieldname].Value.ToString().ToUpper();
                                tbResult.Rows.Add(newrow);
                            }
                        }
                    }


                }
            }
            catch (Exception e)
            {  
            }
            
            return tbResult;
        }

        public static Image MergeIconImage(List<Image> ListOfIcon, int imageWidth)
        {
            int space = 2;
            int width = imageWidth * ListOfIcon.Count;
            int height = imageWidth;

            MemoryStream msNew = new MemoryStream();

            try
            {
                using (var bitmap = new Bitmap(width, height))
                {
                    int iPositionY = 0;
                    int iPositionX = 0;
                    foreach (Image imgicon in ListOfIcon)
                    {
                        using (var canvas = Graphics.FromImage(bitmap))
                        {
                            canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;

                            canvas.DrawImage(imgicon, iPositionX, iPositionY);
                            iPositionX = iPositionX + imageWidth + space;

                            canvas.Save();
                        }
                    }
                    
                    bitmap.Save(msNew, ImageFormat.Png);
                    return Image.FromStream(msNew);
                }
            }
            catch (Exception)
            { }

            return null;

        }
         
        public static string GetFileSize(long fileLength)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            while (fileLength >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                fileLength = fileLength / 1024;
            }
            string result = String.Format("{0:0.##} {1}", fileLength, sizes[order]);
            return result;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }

        public static byte[] DevCompress(byte[] data)
        {
            return DevCompress(data, 30L);
        }

        public static byte[] DevCompress(byte[] data, long value)
        {
            var jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            using (var inStream = new MemoryStream(data))
            using (var outStream = new MemoryStream())
            {
                var image = Image.FromStream(inStream); //FixedSize();

                // if we aren't able to retrieve our encoder
                // we should just save the current image and
                // return to prevent any exceptions from happening
                if (jpgEncoder == null)
                {
                    image.Save(outStream, ImageFormat.Jpeg);
                }
                else
                {
                    var qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(qualityEncoder, value); //10L หยาบ   50L ชัด
                    image.Save(outStream, jpgEncoder, encoderParameters);
                }

                return outStream.ToArray();
            }
        }


        public static byte[] ListImagesToPDF(List<Byte[]> ListBArrayImages , string strWaterMark, Color color)
        {
            if (ListBArrayImages.Count == 0)
                return null;

            if (null == color)
            {
                color = Color.FromArgb(204, 153, 255);
            }

            PdfRectangle pagesize = PdfPaperSize.A4;
            MemoryStream msPDF = new MemoryStream();
            string fontName = "CordiaUPC";
            float fontSize;
            float setupFontSize = 120;
            int iCapacity = 80;
            int watermarkSize = 0;

            PdfStringFormat stringFormat = PdfStringFormat.GenericTypographic;
            stringFormat.Alignment = PdfStringAlignment.Center;
            stringFormat.LineAlignment = PdfStringAlignment.Center;

            using (PdfDocumentProcessor pdfDocumentProcessor = new PdfDocumentProcessor())
            {
                pdfDocumentProcessor.CreateEmptyDocument();
                ///
                PdfGraphics graph = pdfDocumentProcessor.CreateGraphics();
                graph.ConvertImagesToJpeg = true;
                graph.JpegImageQuality = PdfGraphicsJpegImageQuality.Medium; //to do: adjust quality settings
                 
                foreach (byte[] bArray in ListBArrayImages)
                {
                    Image ImgPaper = DxImage.ConvertByteArrayImage(bArray);
                    graph.DrawImage(ImgPaper, GetScaledBounds(ImgPaper.Width, ImgPaper.Height, (float)(double)PdfPaperSize.A4.Width, (float)(double)PdfPaperSize.A4.Height));
                    pdfDocumentProcessor.RenderNewPage(pagesize, graph, 72, 72); 
                }

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(iCapacity, color)))
                {
                    using (Font font = new Font(fontName, setupFontSize))
                    {
                        foreach (var page in pdfDocumentProcessor.Document.Pages)
                        {
                            //1
                            watermarkSize = Convert.ToInt32(page.CropBox.Width * 1.75);
                            if (page.CropBox.Width > page.CropBox.Height) watermarkSize = Convert.ToInt32(page.CropBox.Height * 1.50);

                            fontSize = setupFontSize;

                            using (PdfGraphics graphics = pdfDocumentProcessor.CreateGraphics())
                            {
                                SizeF stringSize = graphics.MeasureString(strWaterMark, font);
                                Single scale = Convert.ToSingle(watermarkSize / stringSize.Width);
                                graphics.TranslateTransform(Convert.ToSingle(page.CropBox.Width * 0.45), Convert.ToSingle(page.CropBox.Height * 0.45));
                                graphics.RotateTransform(45);
                                graphics.TranslateTransform(Convert.ToSingle(-stringSize.Width * scale * 0.45), Convert.ToSingle(-stringSize.Height * scale * 0.45));
                                using (Font actualFont = new Font(fontName, fontSize * scale))
                                {
                                    RectangleF rect = new RectangleF(0, 0, stringSize.Width * scale, stringSize.Height * scale);
                                    graphics.DrawString(strWaterMark, actualFont, brush, rect, stringFormat);
                                }
                                 

                                graphics.AddToPageForeground(page, 75, 75);
                            } 
                        }
                    }
                }
  
                pdfDocumentProcessor.SaveDocument(msPDF);
            }


            return msPDF.ToArray();
        }

        private static RectangleF GetScaledBounds(float imageWidth, float imageHeight, float pageWidth, float pageHeight)
        {
            float ratioX = (pageWidth / imageWidth);
            float ratioY = (pageHeight / imageHeight);
            float ratio = Math.Min(ratioX, ratioY);
            float newWidth = (imageWidth * ratio);
            float newHeight = (imageHeight * ratio);
            if ((newWidth < pageWidth))
            {
                float deltaX = (pageWidth - newWidth) / 2f;
                return new RectangleF(deltaX, 0, newWidth, newHeight);
            }
            else
            {
                float deltaY = (pageHeight - newHeight) / 2f;
                return new RectangleF(0, deltaY, newWidth, newHeight);
            }
        }

        public static bool SavePDFToFile(string filePath, byte[] bPdfArray, out string strError)
        {
            strError = string.Empty;
            try
            {
                using (MemoryStream msPDF = new MemoryStream(bPdfArray))
                {
                    PdfDocumentProcessor pdf = new PdfDocumentProcessor();
                    pdf.LoadDocument(msPDF);

                    pdf.SaveDocument(filePath);
                    return true;
                }
            }
            catch (Exception e)
            {
                strError = e.Message.ToString();
            }

            return false;

        }

        public static bool SaveImagePDFFile(string filePath, List<byte[]> ListBArrayImages, out string strError)
        {
            strError = string.Empty;
            try
            {
                using (PdfDocumentProcessor pdfDocumentProcessor = new PdfDocumentProcessor())
                {
                    pdfDocumentProcessor.CreateEmptyDocument();
                    ///
                    PdfGraphics graph = pdfDocumentProcessor.CreateGraphics();
                    graph.ConvertImagesToJpeg = true;
                    graph.JpegImageQuality = PdfGraphicsJpegImageQuality.Medium; //to do: adjust quality settings

                    foreach (byte[] bArray in ListBArrayImages)
                    {
                        Image ImgPaper = DxImage.ConvertByteArrayImage(bArray);
                        graph.DrawImage(ImgPaper, GetScaledBounds(ImgPaper.Width, ImgPaper.Height, (float)(double)PdfPaperSize.A4.Width, (float)(double)PdfPaperSize.A4.Height));
                        pdfDocumentProcessor.RenderNewPage(PdfPaperSize.A4, graph, 72, 72);
                    }
                     
                    pdfDocumentProcessor.SaveDocument(filePath);
                }

            }
            catch (Exception e)
            {
                strError = e.Message.ToString();
            }

            return false;

        }

        public static Stream ToStream(Image image, ImageFormat formaw)
        {
            try
            {
                var stream = new System.IO.MemoryStream();
                image.Save(stream, formaw);
                stream.Position = 0;
                return stream;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static Color colorOfBlue = Color.Blue; 
        private static Color colorOfTemp = Color.Blue;
        private static Color colorOfSystolic = Color.OrangeRed;
        private static Color colorOfRR = Color.MediumPurple;
        private static Color colorOfPules = Color.Red; 
        private static Color colorOfDiastolic = Color.Teal;
        private static Color colorOfPainScale = Color.Red;
         
    }
}
