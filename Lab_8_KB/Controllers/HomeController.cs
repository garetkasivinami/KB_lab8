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

        public ActionResult Preview(string text, int? fontSize)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                text = "I`m null text";
            }
            if (Cache.Contains($"{text}_img"))
                return new FileContentResult(Cache.Get<byte[]>($"{text}_img_{fontSize}"), ".png");

            var image = ImageGenerator.GetTextWatermark(text, fontSize ?? 30, Color.White);
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Bmp);
                var bytes = ms.ToArray();
                Cache.Add($"{text}_img_{fontSize}", bytes);
                return new FileContentResult(bytes, ".png");
            }
        }

        public ActionResult WaterMarkImage(UploadImageModel model)
        {
            var fileToUpload = model.fileToUpload;
            if (model.fileToUpload == null)
            {
                return RedirectToAction("Index", model);
            }
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
                } else if (model.watermarkText != null)
                {
                    watermarkImage = ImageGenerator.GetTextWatermark(model.watermarkText, model.fontSize);
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
                return RedirectToAction("Index", model);
            }
        }
    }
}