using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CommonObjects;

namespace Extensions
{
    public class OHLCPainter
    {
        public const int WIDTH = 300;
        public const int HEIGHT = 200;
        public Pen BarUpPen { get; set; }
        public Pen BarDownPen { get; set; }
        public Pen BorderPen { get; set; }
        public Pen BorderPen2 { get; set; }

        public OHLCPainter()
        {
            BarUpPen = new Pen(Color.Green);
            BarDownPen = new Pen(Color.Red);
            BorderPen = new Pen(Color.White);
            BorderPen.Width = 5;
            
            BorderPen2 = new Pen(Color.FromArgb(128, Color.DimGray));

        }

        public static String GenerateChartAsJsonString(List<Bar> data)
        {
            var bitmap = new Bitmap(WIDTH, HEIGHT);
            String imgJsonData = String.Empty;
            try
            {

                Graphics graphics = Graphics.FromImage(bitmap);

                var painter = new OHLCPainter();
                painter.PaintLineChart(graphics, new Rectangle(0, 0, WIDTH, HEIGHT), new Padding(10), data);
                var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);
                imgJsonData = Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                imgJsonData = String.Empty;
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                try
                {
                    bitmap.Dispose();
                }
                catch
                { }
            }

            return imgJsonData;
        }

        public void PaintOHLC(Graphics graphics, Rectangle size, Padding padding, List<Bar> data)
        {
            if (data.Count == 0)
                return;

            var drawingRectangle = new Rectangle(size.X, size.Y, size.Width, size.Height);
            drawingRectangle.Inflate(new Size(-10, -10));
            var width = (float)drawingRectangle.Width;
            var height = (float)drawingRectangle.Height;

            graphics.DrawLine(BorderPen, drawingRectangle.X, drawingRectangle.Y, width, drawingRectangle.Y);
            graphics.DrawLine(BorderPen2, drawingRectangle.X, height, width, height);
            graphics.DrawLine(BorderPen2, width, drawingRectangle.Y, width, height);
            
            
            var barSpace = (width - drawingRectangle.X) / data.Count;

            var maxValue = data.Max(record => GetBarMax(record));
            var minValue = data.Min(record => GetBarMin(record));

            height -= drawingRectangle.Y;

            double hMultiplier = 1;
            if (Math.Abs(maxValue - minValue) > double.Epsilon)
                hMultiplier = ((height) - padding.Top - padding.Bottom) / (maxValue - minValue);

            var offset = new Point(drawingRectangle.Left, drawingRectangle.Top);

            for (var i = 1; i < data.Count; i++)
            {
                var r = data[i];

                var hHigh = height - padding.Bottom - (float)((r.High - minValue) * hMultiplier);
                var hLow = height - padding.Bottom - (float)((r.Low - minValue) * hMultiplier);
                var hOpen = height - padding.Bottom - (float)((r.Open - minValue) * hMultiplier);
                var hClose = height - padding.Bottom - (float)((r.Close - minValue) * hMultiplier);

                var x = i * barSpace + barSpace / 2;
                var xLeftBarBorder = x - barSpace / 2;
                var xRightBarBorder = x + barSpace / 2;

                var xLeftVisualBarBorder = x - barSpace / 4;
                var xRightVisualBarBorder = x + barSpace / 4;

                var pen = r.Close >= r.Open
                              ? BarUpPen
                              : BarDownPen;

                DrawLineRelative(graphics, pen, offset, x, hHigh, x, hLow);
                DrawLineRelative(graphics, pen, offset, xLeftVisualBarBorder, hOpen, x, hOpen);
                DrawLineRelative(graphics, pen, offset, x, hClose, xRightVisualBarBorder, hClose);
            }
        }

        public void PaintLineChart(Graphics graphics, Rectangle size, Padding padding, List<Bar> data)
        {
            if (data.Count == 0)
                return;

            var drawingRectangle = new Rectangle(size.X, size.Y, size.Width, size.Height);
            drawingRectangle.Inflate(new Size(-10, -10));
            var width = (float)drawingRectangle.Width;
            var height = (float)drawingRectangle.Height;

            /*graphics.DrawLine(BorderPen, drawingRectangle.X, drawingRectangle.Y, width, drawingRectangle.Y);
            graphics.DrawLine(BorderPen, drawingRectangle.X, height, width, height);
            graphics.DrawLine(BorderPen, width, drawingRectangle.Y, width, height);*/

            var stepX = 25;
            var offsetX = 0;
            var linesCount = (width - offsetX)/stepX;

           /* for (int i = 1; i < linesCount; i ++)
            {
                graphics.DrawLine(BorderPen2, offsetX + i * stepX, drawingRectangle.Y, (offsetX + i) * stepX, height + 5);
            }*/

            var barSpace = (width - drawingRectangle.X) / data.Count;

            var maxValue = data.Max(record => GetBarMax(record));
            var minValue = data.Min(record => GetBarMin(record));

            height -= drawingRectangle.Y;

            double hMultiplier = 1;
            if (Math.Abs(maxValue - minValue) > double.Epsilon)
                hMultiplier = ((height) - padding.Top - padding.Bottom) / (maxValue - minValue);

            var offset = new Point(drawingRectangle.Left, drawingRectangle.Top);
            var path = new GraphicsPath();
            var fillPath = new GraphicsPath();
            var startX = 0.0f;
            for (var i = 0; i < data.Count; i++)
            {
                var r = data[i];
                var hClose = height - padding.Bottom - (float)((r.Close - minValue) * hMultiplier);

                var x = i * barSpace + barSpace / 2;
                if (i == 0 || i == data.Count - 1)
                {
                    if (i == 0)
                        startX = x;

                    AddLineRelative(fillPath, offset, x, hClose, x, height);

                }

                AddLineRelative(fillPath, offset, x, hClose, x, hClose);
                if (i == data.Count - 1)
                {
                    AddLineRelative(fillPath, offset, x, height, startX, height);
                }
            }
            
            for (var i = 0; i < data.Count; i++)
            {
                var r = data[i];
                var hClose = height - padding.Bottom - (float)((r.Close - minValue) * hMultiplier);

                var x = i * barSpace + barSpace / 2;
                AddLineRelative(path, offset, x, hClose, x, hClose);
            }

            Color cl1 = Color.FromArgb(156, Color.Red);
            Color cl2 = Color.FromArgb(156, Color.White);
            var gradientBrush = new LinearGradientBrush(fillPath.GetBounds(), cl1, cl2, LinearGradientMode.Vertical);
            graphics.FillPath(gradientBrush, fillPath);
            graphics.DrawPath(new Pen(Color.White), path);
            //graphics.FillPath(new SolidBrush(Color.Gray), path);
         }

        private static void DrawLineRelative(Graphics graphics, Pen pen, Point offset, float x1, float y1, float x2, float y2)
        {
            graphics.DrawLine(pen, x1 + offset.X, y1 + offset.Y, x2 + offset.X, y2 + offset.Y);
        }

        private static void AddLineRelative(GraphicsPath path, Point offset, float x1, float y1, float x2, float y2)
        {
            path.AddLine(x1 + offset.X, y1 + offset.Y, x2 + offset.X, y2 + offset.Y);
        }

        private static double GetBarMax(Bar r)
        {
            return Math.Max(r.Open, Math.Max(r.High, Math.Max(r.Low, r.Close)));
        }

        private static double GetBarMin(Bar r)
        {
            return Math.Min(r.Open, Math.Min(r.High, Math.Min(r.Low, r.Close)));
        }
    }
}
