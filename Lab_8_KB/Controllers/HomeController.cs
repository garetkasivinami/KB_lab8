using Lab_8_KB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lab_8_KB.Helpers;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace Lab_8_KB.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(UploadImageModel model)
        {
            return View(model);
        }

        public ActionResult Preview(string text, string textColor, int? fontSize)
        {
            if (string.IsNullOrWhiteSpace(text))
                text = "I`m null text";
            
            var parsedColor = ColorTranslator.FromHtml($"#{textColor}");
            var image = ImageGenerator.GetTextWatermark(text, fontSize ?? 30, parsedColor, true);
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Bmp);
                var bytes = ms.ToArray();
                return new FileContentResult(bytes, ".png");
            }
        }

        public ActionResult WaterMarkImage(UploadImageModel model)
        {
            var fileToUpload = model.fileToUpload;
            if (model.fileToUpload == null)
                return RedirectToAction("Index", model);

            using (Image image = Image.FromStream(fileToUpload.InputStream, true, false))
            {
                string name = Path.GetFileNameWithoutExtension(fileToUpload.FileName);
                var ext = Path.GetExtension(fileToUpload.FileName);
                string myfile = DateTime.Now.Ticks.ToString() + ext;
                var saveImagePath = Path.Combine(Server.MapPath("~/Img/Watermark"), myfile);

                var objWatermarker = new Watermark(image);

                Image watermarkImage;
                if (model.waterMark != null)
                {
                    watermarkImage = Image.FromStream(model.waterMark.InputStream, true, false);
                } 
                else if (model.watermarkText != null)
                {
                    var color = ColorTranslator.FromHtml(model.textColor);
                    watermarkImage = ImageGenerator.GetTextWatermark(model.watermarkText, model.fontSize, color);
                    objWatermarker.OffsetBetweenWatermarksScale = new Size(2, 3);
                }
                else
                {
                    watermarkImage = Image.FromFile(Server.MapPath("/Img/watermarklogo.png"));
                }

                objWatermarker.Position = WatermarkPosition.Absolute;
                objWatermarker.Margin = new Size(model.marginX, model.marginY);
                objWatermarker.Offset = new Size(model.offsetX, model.offsetY);
                objWatermarker.Opacity = model.opacity / 100f;
                objWatermarker.TransparentColor = Color.White;
                objWatermarker.ScaleRatio = 1;

                objWatermarker.DrawImage(watermarkImage);

                objWatermarker.Image.Save(saveImagePath, objWatermarker.Image.RawFormat);
                model.imgName = myfile;

                model.fileToUpload = null;
                model.waterMark = null;

                return RedirectToAction("Index", model);
            }
        }
    }
}