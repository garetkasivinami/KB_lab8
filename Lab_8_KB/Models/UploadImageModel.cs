using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lab_8_KB.Models
{
    public class UploadImageModel
    {
        public HttpPostedFileBase fileToUpload { get; set; }
        public HttpPostedFileBase waterMark { get; set; }
        public string watermarkText { get; set; }
        public int fontSize { get; set; }
        public int offsetX { get; set; }
        public int offsetY { get; set; }
        public int opacity { get; set; }
    }
}