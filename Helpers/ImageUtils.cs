﻿/**
 *   Copyright (C) 2021 okaygo
 *
 *   https://github.com/misterokaygo/D2RAssist/
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 **/

using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;

namespace D2RAssist.Helpers
{
    public static class ImageUtils
    {
        public static (Bitmap, Point) RotateImage(Image inputImage, float angleDegrees, bool upsizeOk, bool clipOk,
            Color backgroundColor, Point localPlayerPosition)
        {
            // Test for zero rotation and return a clone of the input image
            if (angleDegrees == 0f)
                return ((Bitmap)inputImage.Clone(), localPlayerPosition);

            // Set up old and new image dimensions, assuming upsizing not wanted and clipping OK
            int oldWidth = inputImage.Width;
            int oldHeight = inputImage.Height;
            int newWidth = oldWidth;
            int newHeight = oldHeight;
            var scaleFactor = 1f;

            // If upsizing wanted or clipping not OK calculate the size of the resulting bitmap
            if (upsizeOk || !clipOk)
            {
                double angleRadians = angleDegrees * Math.PI / 180d;

                double cos = Math.Abs(Math.Cos(angleRadians));
                double sin = Math.Abs(Math.Sin(angleRadians));
                newWidth = (int)Math.Round(oldWidth * cos + oldHeight * sin);
                newHeight = (int)Math.Round(oldWidth * sin + oldHeight * cos);
            }

            // If upsizing not wanted and clipping not OK need a scaling factor
            if (!upsizeOk && !clipOk)
            {
                scaleFactor = Math.Min((float)oldWidth / newWidth, (float)oldHeight / newHeight);
                newWidth = oldWidth;
                newHeight = oldHeight;
            }

            // Create the new bitmap object. If background color is transparent it must be 32-bit, 
            //  otherwise 24-bit is good enough.
            var newBitmap = new Bitmap(newWidth, newHeight,
                backgroundColor == Color.Transparent ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
            newBitmap.SetResolution(inputImage.HorizontalResolution, inputImage.VerticalResolution);

            // Create the Graphics object that does the work
            using (Graphics graphicsObject = Graphics.FromImage(newBitmap))
            {
                graphicsObject.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsObject.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphicsObject.SmoothingMode = SmoothingMode.HighQuality;

                // Fill in the specified background color if necessary
                if (backgroundColor != Color.Transparent)
                    graphicsObject.Clear(backgroundColor);

                // Set up the built-in transformation matrix to do the rotation and maybe scaling
                graphicsObject.TranslateTransform (newWidth / 2f, newHeight / 2f);

                if (scaleFactor != 1f)
                    graphicsObject.ScaleTransform(scaleFactor, scaleFactor);

                graphicsObject.RotateTransform(angleDegrees);
                graphicsObject.TranslateTransform (-oldWidth / 2f, -oldHeight / 2f);


                double angleRadians = angleDegrees * Math.PI / 180d;
                double cos = Math.Abs(Math.Cos(angleRadians));
                double sin = Math.Abs(Math.Sin(angleRadians));

                int localPlayerPositionXFromCenter = localPlayerPosition.X - oldWidth / 2;
                int localPlayerPositionYFromCenter = localPlayerPosition.Y - oldHeight / 2;

                localPlayerPosition.X = (int)(-localPlayerPositionYFromCenter * sin + localPlayerPositionXFromCenter * cos) + newWidth / 2;
                localPlayerPosition.Y = (int)(localPlayerPositionYFromCenter * cos + localPlayerPositionXFromCenter * sin) + newHeight / 2;

                // Draw the result
                graphicsObject.DrawImage(inputImage, 0, 0);
            }

            return (newBitmap, localPlayerPosition);
        }

        public static (Bitmap, Point) CropBitmap(Bitmap originalBitmap)
        {
            // Find the min/max non-white/transparent pixels
            var min = new Point(int.MaxValue, int.MaxValue);
            var max = new Point(int.MinValue, int.MinValue);

            for (var x = 0; x < originalBitmap.Width; ++x)
            {
                for (var y = 0; y < originalBitmap.Height; ++y)
                {
                    Color pixelColor = originalBitmap.GetPixel(x, y);
                    if (pixelColor.A == 255)
                    {
                        if (x < min.X) min.X = x;
                        if (y < min.Y) min.Y = y;

                        if (x > max.X) max.X = x;
                        if (y > max.Y) max.Y = y;
                    }
                }
            }

            // Create a new bitmap from the crop rectangle
            var cropRectangle = new Rectangle(min.X, min.Y, max.X - min.X, max.Y - min.Y);
            var newBitmap = new Bitmap(cropRectangle.Width, cropRectangle.Height);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.DrawImage(originalBitmap, 0, 0, cropRectangle, GraphicsUnit.Pixel);
            }

            return (newBitmap, min);
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static (Bitmap, Point) ResizeImage(Image image, double multiplier, Point localPlayerPosition)
        {
            int width = (int)(image.Width * multiplier);
            int height = (int)(image.Height * multiplier);

            localPlayerPosition.X = (int)(localPlayerPosition.X * multiplier);
            localPlayerPosition.Y = (int)(localPlayerPosition.Y * multiplier);

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            float imageResolution = (float)Math.Min(image.Width, image.Height) / Math.Max(image.Width, image.Height);
            


            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(destImage))
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

            return (destImage, localPlayerPosition);
        }

        public static Bitmap CreateFilledRectangle(Color color, int width, int height)
        {
            var rectangle = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(rectangle);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillRectangle(new SolidBrush(color), 0, 0, width, height);
            graphics.Dispose();
            return rectangle;
        }

        public static Bitmap CreateFilledEllipse(Color color, int width, int height)
        {
            var ellipse = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(ellipse);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillEllipse(new SolidBrush(color), 0, 0, width, height);
            graphics.Dispose();
            return ellipse;
        }
    }
}
