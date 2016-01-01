using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Mvc;
using DAL;

namespace AdvSpareAuto.Controllers
{
    public class PhotoController : Controller
    {
        private IImageRepository _imageRepository;

        public PhotoController(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        //
        // GET: /Photo/


        public ActionResult Index(int id)
        {
            byte[] imageData = _imageRepository.Get(id);
            if (imageData == null) return null;
            Stream stream = new MemoryStream(imageData);
            //Image objImage = Bitmap.FromStream(stream);
            // var objImage = Crop(objImage, objImage.Width, objImage.Height - 20);
            //objImage.Save(Server.MapPath("~/App_data/img.jpg"));
            return File(new MemoryStream(imageData), "image/jpeg");
          //  return FileResult(objImage);
                // Might need to adjust the content type based on your actual image type

        }

        public ActionResult Thumb(int id)
        {
            byte[] imageData = _imageRepository.Get(id);
            if (imageData == null) return null;


            return File(new MemoryStream(imageData), "image/jpeg");
            /*Image objImage = Image.FromStream(new MemoryStream(imageData));
            objImage = objImage.GetThumbnailImage(100, objImage.Width / objImage.Height * 100, () => { return true; }, new IntPtr(1));

            return FileResult(objImage);*/
        }

        private ActionResult FileResult(Image objImage)
        {
            Stream ms = new MemoryStream();
            
                objImage.Save(ms, ImageFormat.Jpeg);
             

                return File(ms, "image/jpeg");
            
        }

        /*       private Bitmap WaterMark(Bitmap image) {


          /*  using (Bitmap watermarkImage = Bitmap.(Server.MapPath("~Images/watermark.png")))
            {
                using (Graphics imageGraphics = Graphics.FromImage(image))
                {
                    watermarkImage.SetResolution(imageGraphics.DpiX, imageGraphics.DpiY);

        int x = ((image.Width - watermarkImage.Width) / 2);
        int y = ((image.Height - watermarkImage.Height) / 2);

        imageGraphics.DrawImage(watermarkImage, x, y, watermarkImage.Width, watermarkImage.Height);
            }#1#
        

    
    

        
    
        }*/

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public Image Crop(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height-30);
            var destImage = new Bitmap(width, height-30);

            
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(destImage, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    return destImage;
                }
            }

            
        }

        public void ReleaseContext()
        {
            _imageRepository.ReleaseContext();
        }
    }


}
