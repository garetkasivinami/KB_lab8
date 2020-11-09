using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lab_8_KB.Helpers
{
    public static class ImageGenerator
    {
        private static string fontFamily = FontFamily.GenericSerif.Name;
        private static Color DefaultFontColor { get; set; } = Color.Black;
        private static Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
        private static Color fillColor = Color.FromArgb(128, 128, 128);

        public static Bitmap GetTextWatermark(string text, int fontSize, Color? fontColor = null, bool preview = false)
        {
            var font = Cache.GetOrCreate($"Font_{fontFamily}_{fontSize}", () => new Font(fontFamily, fontSize));
            var _fontColor = fontColor ?? DefaultFontColor;
            var brush = Cache.GetOrCreate($"SolidBrush_{_fontColor}", () => new SolidBrush(_fontColor));
            SizeF size = graphics.MeasureString(text, font);
            Bitmap bitmap = new Bitmap((int)size.Width, (int)size.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                if (preview)
                    g.Clear(fillColor);

                g.DrawString(text, font, brush, 0, 0);
                return bitmap;
            }
        }
    }
}