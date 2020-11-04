using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Lab_8_KB.Helpers
{
    #region WatermarkPosition
    public enum WatermarkPosition
    {
        Absolute,
        TopLeft,
        TopRight,
        TopMiddle,
        BottomLeft,
        BottomRight,
        BottomMiddle,
        MiddleLeft,
        MiddleRight,
        Center
    }
    #endregion

    #region Watermark
    public class Watermark
    {

        #region Private Fields
        private Image m_image;
        private Image m_originalImage;
        private Image m_watermark;
        private float m_opacity = 1.0f;
        private WatermarkPosition m_position = WatermarkPosition.Absolute;
        private int m_x = 0;
        private int m_y = 0;
        private Color m_transparentColor = Color.Empty;
        private RotateFlipType m_rotateFlip = RotateFlipType.RotateNoneFlipNone;
        private Size margin = new Size();
        private float m_scaleRatio = 1.0f;
        private Size offsetBetweenWatermarksScale = new Size(2,2);
        private Size offset;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the image with drawn watermarks
        /// </summary>
        [Browsable(false)]
        public Image Image { get { return m_image; } }

        public Size OffsetBetweenWatermarksScale
        {
            get => offsetBetweenWatermarksScale;
            set => offsetBetweenWatermarksScale = value;
        }

        /// <summary>
        /// Watermark position relative to the image sizes. 
        /// If Absolute is chosen, watermark positioning is being done via PositionX and PositionY 
        /// properties (0 by default)\n
        /// </summary>        
        public WatermarkPosition Position { get { return m_position; } set { m_position = value; } }

        public Size Offset
        {
            get => offset;
            set => offset = value;
        }
        /// <summary>
        /// Watermark X coordinate (works if Position property is set to WatermarkPosition.Absolute)
        /// </summary>
        public int PositionX { get { return m_x; } set { m_x = value; } }

        /// <summary>
        /// Watermark Y coordinate (works if Position property is set to WatermarkPosition.Absolute)
        /// </summary>
        public int PositionY { get { return m_y; } set { m_y = value; } }

        /// <summary>
        /// Watermark opacity. Can have values from 0.0 to 1.0
        /// </summary>
        public float Opacity { get { return m_opacity; } set { m_opacity = value; } }

        /// <summary>
        /// Transparent color
        /// </summary>
        public Color TransparentColor { get { return m_transparentColor; } set { m_transparentColor = value; } }

        /// <summary>
        /// Watermark rotation and flipping
        /// </summary>
        public RotateFlipType RotateFlip { get { return m_rotateFlip; } set { m_rotateFlip = value; } }

        /// <summary>
        /// Spacing between watermark and image edges
        /// </summary>
        public Size Margin { get { return margin; } set { margin = value; } }

        /// <summary>
        /// Watermark scaling ratio. Must be greater than 0. Only for image watermarks
        /// </summary>
        public float ScaleRatio { get { return m_scaleRatio; } set { m_scaleRatio = value; } }

        #endregion

        #region Constructors
        public Watermark(Image image)
        {
            LoadImage(image);
        }

        public Watermark(string filename)
        {
            LoadImage(Image.FromFile(filename));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Resets image, clearing all drawn watermarks
        /// </summary>
        public void ResetImage()
        {
            m_image = new Bitmap(m_originalImage);
        }

        public void DrawImage(string filename)
        {
            DrawImage(Image.FromFile(filename));
        }

        public void DrawImage(Image watermark)
        {

            if (watermark == null)
                throw new ArgumentOutOfRangeException("Watermark");

            if (m_opacity < 0 || m_opacity > 1)
                throw new ArgumentOutOfRangeException("Opacity");

            if (m_scaleRatio <= 0)
                throw new ArgumentOutOfRangeException("ScaleRatio");

            // Creates a new watermark with margins (if margins are not specified returns the original watermark)
            m_watermark = GetWatermarkImage(watermark);

            // Rotates and/or flips the watermark
            m_watermark.RotateFlip(m_rotateFlip);

            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][] {
                    new float[] { 1, 0f, 0f, 0f, 0f},
                    new float[] { 0f, 1, 0f, 0f, 0f},
                    new float[] { 0f, 0f, 1, 0f, 0f},
                    new float[] { 0f, 0f, 0f, m_opacity, 0f},
                    new float[] { 0f, 0f, 0f, 0f, 1}
                });

            ImageAttributes attributes = new ImageAttributes();

            // Set the opacity of the watermark
            attributes.SetColorMatrix(colorMatrix);

            // Set the transparent color 
            if (m_transparentColor != Color.Empty)
            {
                attributes.SetColorKey(m_transparentColor, m_transparentColor);
            }

            // Draw the watermark
            using (Graphics gr = Graphics.FromImage(m_image))
            {
                for (int i = offset.Height - margin.Height; i < Image.Height; i++)
                {
                    for (int j = offset.Width - margin.Width; j < Image.Width; j++)
                    {

                        PositionX = j;
                        PositionY = i;

                        // Calculate watermark position
                        Point waterPos = GetWatermarkPosition();
                        // Watermark destination rectangle
                        Rectangle destRect = new Rectangle(waterPos.X, waterPos.Y, m_watermark.Width, m_watermark.Height);

                        // Set the properties for the log
                        // Draw the logo
                        gr.DrawImage(m_watermark, destRect, 0, 0, m_watermark.Width, m_watermark.Height, GraphicsUnit.Pixel, attributes);
                        //Draw the Text
                        //objWatermarker.DrawText("WaterMarkDemo")

                        j = j + m_watermark.Width;// watermark image width 
                    }
                    i = i + m_watermark.Height;//
                }
                
            }
        }

        public void DrawText(string text, int fontSize)
        {
            // Convert text to image, so we can use opacity etc.
            Image textWatermark = ImageGenerator.GetTextWatermark(text, fontSize);

            DrawImage(textWatermark);
        }
        #endregion

        #region Private Methods
        private void LoadImage(Image image)
        {
            m_originalImage = image;
            ResetImage();
        }

        private Image GetWatermarkImage(Image watermark)
        {

            // If there are no margins specified and scale ration is 1, no need to create a new bitmap
            if (margin.Width == 0 && margin.Height == 0 && m_scaleRatio == 1.0f)
                return watermark;

            // Create a new bitmap with new sizes (size + margins) and draw the watermark
            int newWidth = Convert.ToInt32(watermark.Width * m_scaleRatio);
            int newHeight = Convert.ToInt32(watermark.Height * m_scaleRatio);

            Rectangle sourceRect = new Rectangle(margin.Width, margin.Height, newWidth, newHeight);
            Rectangle destRect = new Rectangle(0, 0, watermark.Width, watermark.Height);

            Bitmap bitmap = new Bitmap(newWidth + margin.Width, newHeight + margin.Height);
            bitmap.SetResolution(watermark.HorizontalResolution, watermark.VerticalResolution);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(watermark, sourceRect, destRect, GraphicsUnit.Pixel);
            }

            return bitmap;
        }

        private Point GetWatermarkPosition()
        {
            int x = 0;
            int y = 0;

            switch (m_position)
            {
                case WatermarkPosition.Absolute:
                    x = m_x; y = m_y;
                    break;
                case WatermarkPosition.TopLeft:
                    x = 0; y = 0;
                    break;
                case WatermarkPosition.TopRight:
                    x = m_image.Width - m_watermark.Width; y = 0;
                    break;
                case WatermarkPosition.TopMiddle:
                    x = (m_image.Width - m_watermark.Width) / 2; y = 0;
                    break;
                case WatermarkPosition.BottomLeft:
                    x = 0; y = m_image.Height - m_watermark.Height;
                    break;
                case WatermarkPosition.BottomRight:
                    x = m_image.Width - m_watermark.Width; y = m_image.Height - m_watermark.Height;
                    break;
                case WatermarkPosition.BottomMiddle:
                    x = (m_image.Width - m_watermark.Width) / 2; y = m_image.Height - m_watermark.Height;
                    break;
                case WatermarkPosition.MiddleLeft:
                    x = 0; y = (m_image.Height - m_watermark.Height) / 2;
                    break;
                case WatermarkPosition.MiddleRight:
                    x = m_image.Width - m_watermark.Width; y = (m_image.Height - m_watermark.Height) / 2;
                    break;
                case WatermarkPosition.Center:
                    x = (m_image.Width - m_watermark.Width) / 2; y = (m_image.Height - m_watermark.Height) / 2;
                    break;
                default:
                    break;
            }

            return new Point(x, y);
        }
        #endregion

    }

    public static class ImageGenerator {
        private static string fontFamily = FontFamily.GenericSerif.Name;
        private static Color DefaultFontColor { get; set; } = Color.Black;
        private static Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);

        public static Bitmap GetTextWatermark(string text, int fontSize, Color? fontColor = null)
        {
            var font = new Font(fontFamily, fontSize);
            var _fontColor = fontColor ?? DefaultFontColor;
            Brush brush = new SolidBrush(_fontColor);
            SizeF size = graphics.MeasureString(text, font);
            Bitmap bitmap = new Bitmap((int)size.Width, (int)size.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawString(text, font, brush, 0, 0);
                return bitmap;
            }
        }
    }

    #endregion
}