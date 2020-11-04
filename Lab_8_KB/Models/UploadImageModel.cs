using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;

namespace Lab_8_KB.Models
{
    public class UploadImageModel
    {
        public HttpPostedFileBase fileToUpload { get; set; }
        public HttpPostedFileBase waterMark { get; set; }
        public string watermarkText { get; set; }
        public int fontSize { get; set; } = 30;
        public int offsetX { get; set; } = 200;
        public int offsetY { get; set; } = 200;
        public int marginX { get; set; } = 0;
        public int marginY { get; set; } = 0;
        public int opacity { get; set; } = 50;
        [BindNever]
        public string imgName { get; set; }
    }
}