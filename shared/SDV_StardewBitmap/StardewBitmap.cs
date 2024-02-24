
//
//  abstraction class to easily move graphics routines between
//  Stardew Valley environments (pre 1.5.5 and 1.5.5 and beyond)
//
//  When compiling for SDV 1.5.4 and less the 'Classic' conditional flag
//  is defined and all graphics are done with the System.Drawing library
//  (for 32 bit code)
//
//  When compiling for SDV 1.5.5 and above 'SKIA' conditional flag
//  is defined and all graphics are done with the SkiaSharp library
//  (for 64 bit code)
//
//  When compiling for SDV 1.5.5 and above the conditional flag 'Common'
//  is defined then the microsoft System.Drawing.Common library is used
//  (for 64 bit code)
//
//  All mod code uses the class StardewBitmap for graphic routines
//  allowing the changing of the conditional flags without requiring
//  refactoring of any of the mod code
//
//  Graphic properties use XNA objects for parameters to allow seamless
//  graphic library switching
//
//  Properties currently used
//  
//  Rectangle
//  Color



#if Classic || Common
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
#elif SKIA
using SkiaSharp;
#endif

//using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using Netcode;
using System.IO;
using xColor = Microsoft.Xna.Framework.Color;
using StardewModHelpers;

#if StardewWeb
using StardewWeb.Utilities;
#endif

namespace StardewModHelpers
{
    internal class Size
    {
        public Size() { }
        public Size(int iWidth, int iHeight)
        {
            Width = iWidth;
            Height = iHeight;
        }
        public int Width { get; set; }
        public int Height { get; set; }
    }
#if SKIA
    public class StardewBitmap
    {
        public SKBitmap SourceImage = null;

        private Texture2D txOutput = null;

        #region "Constructors"
        public StardewBitmap()
        {

        }
        public StardewBitmap(string sFilename)
        {
            SourceImage = SKBitmap.Decode(sFilename);
        }
        public StardewBitmap(Texture2D texture)
        {
            SourceImage = SKBitmap.Decode(StardewThreadSafeLoader.GetTextureMS("something", texture));

            //    SourceImage = SKBitmap.Decode(ms);
            //}
        }
        public StardewBitmap(SKBitmap texture)
        {
            SourceImage = texture;// SKBitmap.Decode(StardewThreadSafeLoader.GetTextureMS("something", texture));

            //    SourceImage = SKBitmap.Decode(ms);
            //}
        }
        public StardewBitmap(int width, int height)
        {
            SourceImage = new SKBitmap(width, height);
        }
        public StardewBitmap(MemoryStream ms)
        {
            SourceImage = SKBitmap.Decode(ms);
        }
        public StardewBitmap(NetArray<int, NetInt> oNetArray)
        {
            //
            //  real fun hack to be able to pass bubble images
            //  between players
            //
            int[] arBytes = new int[oNetArray.Count];
            oNetArray.CopyTo(arBytes, 0);
            byte[] result = new byte[arBytes.Length];
            for (int iPtr = 0; iPtr < arBytes.Length; iPtr++)
            {
                result[iPtr] = (byte)arBytes[iPtr];
            }

            MemoryStream ms = new MemoryStream(result, false);
            ms.Seek(0, SeekOrigin.Begin);
            SourceImage = SKBitmap.Decode(ms);
        }
        #endregion

        public static StardewBitmap LoadFromContent(string sContentPath)
        {
            return StardewThreadSafeLoader.LoadImageInUIThread(sContentPath);
        }

        #region "public properties"
        public int Height { get { return SourceImage.Height; } }
        public int Width { get { return SourceImage.Width; } }
        #endregion

        #region "public methods"
        public void DrawRectangle(xColor cLine, int iLeft, int iTop, int iWidth, int iHeight)
        {
            var canvas = new SKCanvas(SourceImage);
            var rect = SKRect.Create(iLeft, iTop, iWidth, iHeight);
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = ConvertColor(cLine)
            };

            canvas.DrawRect(rect, paint);

            canvas.Flush();
        }
        public NetArray<int, NetInt> TextureNetArray()
        {
            //
            //  probably not the most efficient method but it
            //  provides the ability to passs textures between
            //  multiplayer players
            //
            NetArray<int, NetInt> arReturn = new NetArray<int, NetInt>();
            MemoryStream ms = new MemoryStream();
            Texture().SaveAsPng(ms, SourceImage.Width, SourceImage.Height);
            foreach (byte bBtyte in ms.ToArray())
            {
                arReturn.Add(bBtyte);
            }

            return arReturn;
        }
        public void Save(MemoryStream sourceStream, SKEncodedImageFormat format)
        {
            using (var image = SKImage.FromBitmap(SourceImage))
            using (var data = image.Encode(format, 80))
            {
                // save the data to a stream
            
                    data.SaveTo(sourceStream);
            }
        }
        public void Save(string sFilename, SKEncodedImageFormat format)
        {
            using (var image = SKImage.FromBitmap(SourceImage))
            using (var data = image.Encode(format, 80))
            {
                // save the data to a stream
                using (var stream = File.OpenWrite(sFilename))
                {
                    data.SaveTo(stream);
                }
            }
        }
        public void Save(string sFilename)
        {
           Save(sFilename,SKEncodedImageFormat.Png);
        }
        public void FillRectangle(xColor cFill, int iLeft, int iTop, int iWidth, int iHeight)
        {

            var canvas = new SKCanvas(SourceImage);
            // the rectangle
            var rect = SKRect.Create(iLeft, iTop, iWidth, iHeight);

            // the brush
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = ConvertColor(cFill)
            };

            // draw fill
            canvas.DrawRect(rect, paint);

            canvas.Flush();
        }
        public StardewBitmap GetBoundedImage(Rectangle rBounds)
        {

            StardewBitmap imPort = new StardewBitmap(rBounds.Width, rBounds.Height);
            imPort.DrawImage(this, new Rectangle(0, 0, rBounds.Width, rBounds.Height), rBounds);

            return imPort;
        }
        public StardewBitmap GetBoundedImage(int iWidth, int iHeight, int iImageIndex)
        {
            int iRow = 0;
            int iCol = 0;

            int iCols = SourceImage.Width / iWidth;

            if (iImageIndex > -1)
            {
                iRow = iImageIndex / iCols;
                iCol = iImageIndex % iCols;
            }

            StardewBitmap imPort = new StardewBitmap(iWidth, iHeight);
            imPort.DrawImage(this, new Rectangle(0, 0, iWidth, iHeight), new Rectangle(iWidth * iCol, iRow * iHeight, iWidth, iHeight));

            return imPort;
        }
        public Texture2D Texture()
        {
            if (txOutput == null)
            {
#if false
                return null;
#else
                txOutput = Texture2D.FromStream(Game1.graphics.GraphicsDevice, SourceStream());
#endif
             }
#if LOG_DEBUG
            //SDV_Logger.DumpObject("txOutput", txOutput);
#endif
            return txOutput;
        }

        public void DrawImage(StardewBitmap image, Rectangle destination, Rectangle source)
        {
#if LOG_TRACE
            SDV_Logger.DumpObject("drawimage image", image);
            SDV_Logger.DumpObject("drawimage SourceImage", SourceImage);
            SDV_Logger.DumpObject("  d rectange", destination);
            SDV_Logger.DumpObject("  d skrectange", ConvertRectangle(destination));
            SDV_Logger.DumpObject("  s rectange", source);
            SDV_Logger.DumpObject("  s krectange", ConvertRectangle(source));

#endif
            var canvas = new SKCanvas(SourceImage);

            canvas.DrawBitmap(image.SourceImage, ConvertRectangle(source), ConvertRectangle(destination));
            canvas.Flush();
            canvas.Save();
        }
        public  Vector2 getTextCenterToDraw(string text, Rectangle region, SKPaint paint)
        {
            SKRect textBounds = new SKRect();
            paint.MeasureText(text,ref textBounds);
            float x = region.Width/2- textBounds.Width * 0.4f;
            float y = region.Height/2 + textBounds.Height * 0.4f;
            return new Vector2(x, y);
        }
        public void DrawString(string text, int x, int y)
        {
            DrawString(text, "Arial", 64.0f, new xColor(255, 255, 255, 255), x, y);
        }
        public void DrawString(string text, string sFontName, float fFontSize, xColor cFontColor, int x, int y)
        {
            var canvas = new SKCanvas(SourceImage);
            var font = SKTypeface.FromFamilyName(sFontName);
            var brush = new SKPaint
            {
                Typeface = font,
                TextSize = fFontSize,
                TextAlign = SKTextAlign.Center,
                IsAntialias = false,
                Color = ConvertColor(cFontColor)
            };
            //Vector2 textCenter = getTextCenterToDraw(text,new Rectangle(0,0,Width,Height),brush);
            //canvas.DrawText(text,textCenter.X, textCenter.Y, brush);
            canvas.DrawText(text, x, y, brush);
        }
        public MemoryStream SourceStream()
        {
            SKImage image = SKImage.FromBitmap(SourceImage);
            SKData encoded = image.Encode();
            var memoryStream = new MemoryStream();
            encoded.AsStream().CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

#if LOG_DEBUG
            //SDV_Logger.LogInfo("SourceStream", $"Stream length: {memoryStream.Length}");
#endif
            return memoryStream;
        }
        public void FillArray(ref NetArray<int, NetInt> oArray)
        {
            oArray.Clear();
            MemoryStream ms = StardewThreadSafeLoader.GetTextureMS("FillArray", Texture());
            //Texture().SaveAsPng(ms, SourceImage.Width, SourceImage.Height);
            foreach (byte bBtyte in ms.ToArray())
            {
                oArray.Add(bBtyte);
            }
        }
        public void ResizeImage(int width, int height)
        {
            SourceImage = SourceImage.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);
        }
#endregion

                #region "private methods"

        public static SKColor ConvertColor(xColor cColor)
        {
            return new SKColor(cColor.R, cColor.G, cColor.B);
        }
        public static SKRect ConvertRectangle(Rectangle rect)
        {
            return new SKRect(rect.Left, rect.Top, rect.Left + rect.Width, rect.Top + rect.Height);
        }

        private SKBitmap StardewToNative(StardewBitmap source)
        {
            return SKBitmap.Decode(source.SourceStream());
        }
                #endregion
    }

#endif

#if Classic || Common
    internal class StardewBitmap
    {
        public Bitmap SourceImage = null;
        private Texture2D txOutput = null;

    #region "Constructors"
        public StardewBitmap()
        {

        }
        public StardewBitmap(Image source)
        {
            SourceImage = new Bitmap(source);
        }
        public StardewBitmap(StardewBitmap source)
        {
            SourceImage = new Bitmap(source.SourceImage);
        }
        public StardewBitmap(string pngFileName)
        {
            SourceImage = (Bitmap)Bitmap.FromFile(pngFileName);
        }
        public StardewBitmap(Texture2D texture)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                texture.SaveAsPng(ms, texture.Width, texture.Height);
                //Go To the  beginning of the stream.
                ms.Seek(0, SeekOrigin.Begin);
                //Create the image based on the stream.
                SourceImage = new Bitmap(ms);
            }
        }
        public StardewBitmap(int width, int height)
        {
            SourceImage = new Bitmap(width, height);
        }
        public StardewBitmap(MemoryStream ms)
        {
            SourceImage = new Bitmap(ms);
        }
        public StardewBitmap(NetArray<int, NetInt> oNetArray)
        {
            //
            //  real fun hack to be able to pass bubble images
            //  between players
            //
            int[] arBytes = new int[oNetArray.Count];
            oNetArray.CopyTo(arBytes, 0);
            byte[] result = new byte[arBytes.Length];
            for (int iPtr = 0; iPtr < arBytes.Length; iPtr++)
            {
                result[iPtr] = (byte)arBytes[iPtr];
            }

            MemoryStream ms = new MemoryStream(result, false);
            ms.Seek(0, SeekOrigin.Begin);
            SourceImage = new Bitmap(ms);
        }
    #endregion

    #region "public properties"
        public int Height { get { return SourceImage.Height; } }
        public int Width { get { return SourceImage.Width; } }
    #endregion

    #region "public methods"
        public static StardewBitmap LoadFromContent(string sContentPath)
        {
            return StardewThreadSafeLoader.LoadImageInUIThread(sContentPath);
        }
        public void FillArray(ref NetArray<int, NetInt> oArray)
        {
            oArray.Clear();
            MemoryStream ms = new MemoryStream();
            Texture().SaveAsPng(ms, SourceImage.Width, SourceImage.Height);
            foreach (byte bBtyte in ms.ToArray())
            {
                oArray.Add(bBtyte);
            }
        }
        public NetArray<int, NetInt> TextureNetArray()
        {
            //
            //  probably not the most efficient method but it
            //  provides the ability to passs textures between
            //  multiplayer players
            //
            NetArray<int, NetInt> arReturn = new NetArray<int, NetInt>();
            MemoryStream ms = new MemoryStream();
            Texture().SaveAsPng(ms, SourceImage.Width, SourceImage.Height);
            foreach (byte bBtyte in ms.ToArray())
            {
                arReturn.Add(bBtyte);
            }

            return arReturn;
        }
        public Texture2D Texture()
        {
            if (txOutput == null)
            {
                txOutput = Texture2D.FromStream(Game1.graphics.GraphicsDevice, SourceStream());
            }
            return txOutput;
        }

        public void DrawImage(StardewBitmap image, Rectangle destination, Rectangle source)
        {
#if LOG_TRACE && StardewWeb
            StardewLogger.DumpObject("source rect", source);
            StardewLogger.DumpObject("dest rect", destination);
#endif
            ImageAttributes iAttr = new ImageAttributes();
            using (Graphics gr = Graphics.FromImage(SourceImage))
            {
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                gr.DrawImage(image.SourceImage, ConvertRectangle(destination), ConvertRectangle(source), GraphicsUnit.Pixel);
            }
        }
        public void DrawString(string text, int x, int y)
        {
            DrawString(text, "Verdana", 16.0f, xColor.Black, x, y, 0, 0);
        }
        public void DrawString(string text, int x, int y, int width, int height)
        {
            DrawString(text, "Verdana", 16.0f, xColor.Black, x, y, width, height);
        }
        public void DrawString(string text, string sFontName, float fFontSize, xColor cTextColor, int x, int y, int width, int height)
        {
            Font fText = new Font(sFontName, fFontSize, FontStyle.Bold);
            using (Graphics gr = Graphics.FromImage(SourceImage))
            {
                SizeF szStrring = gr.MeasureString(text, fText);
                if (width == 0)
                {
                    gr.DrawString(text, fText, new SolidBrush(ConvertColor(cTextColor)), x, y);
                }
                else
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
                    gr.DrawString(text, fText, new SolidBrush(ConvertColor(cTextColor)), new System.Drawing.Rectangle(x, y, width, height), sf);
                }
            }
        }
        public MemoryStream SourceStream()
        {
            MemoryStream memoryStream = new MemoryStream();
            SourceImage.Save(memoryStream, ImageFormat.Png);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }
        public StardewBitmap GetBoundedImage(Rectangle rBounds)
        {
            StardewBitmap imPort = new StardewBitmap(rBounds.Width, rBounds.Height);
            imPort.DrawImage(this, new Rectangle(0, 0, rBounds.Width, rBounds.Height), rBounds);

            return imPort;
        }
        public void FillRectangle(xColor cFill, int iLeft, int iTop, int iWidth, int iHeight)
        {
            using (Graphics gr = Graphics.FromImage(SourceImage))
            {
                gr.FillRectangle(new SolidBrush(ConvertColor(cFill)), new System.Drawing.Rectangle(iLeft, iTop, iWidth, iHeight));
            }
        }
        public void DrawRectangle(xColor cLine, int iLeft, int iTop, int iWidth, int iHeight)
        {
            using (Graphics gr = Graphics.FromImage(SourceImage))
            {
                gr.DrawRectangle(new Pen(new SolidBrush(ConvertColor(cLine)), 1), iLeft, iTop, iWidth, iHeight);
            }
        }
        public StardewBitmap GetBoundedImage(int iWidth, int iHeight, int iImageIndex)
        {

            int iRow = 0;
            int iCol = 0;

            int iCols = SourceImage.Width / iWidth;

            if (iImageIndex > -1)
            {
                iRow = iImageIndex / iCols;
                iCol = iImageIndex % iCols;
            }

            StardewBitmap imPort = new StardewBitmap(iWidth, iHeight);
            imPort.DrawImage(this, new Rectangle(0, 0, iWidth, iHeight), new Rectangle(iWidth * iCol, iRow * iHeight, iWidth, iHeight));

            return imPort;
        }
        public void ResizeImage(int width, int height)
        {
            Image imNew = new Bitmap(SourceImage, width, height);
            SourceImage = new Bitmap(imNew);
        }
        public void Save(string sFilename)
        {
            SourceImage.Save(sFilename);
        }
    #endregion

    #region "private methods"
        private MemoryStream GetTextureStream(Texture2D tTexture)
        {
            MemoryStream stream = new MemoryStream();

            using (Bitmap bitmap = new Bitmap(tTexture.Width, tTexture.Height, PixelFormat.Format32bppArgb))
            {
                byte blue;
                IntPtr safePtr;
                BitmapData bitmapData;
                Rectangle rect = new Rectangle(0, 0, tTexture.Width, tTexture.Height);
                byte[] textureData = new byte[4 * tTexture.Width * tTexture.Height];

                tTexture.GetData<byte>(textureData);
                for (int i = 0; i < textureData.Length; i += 4)
                {
                    blue = textureData[i];
                    textureData[i] = textureData[i + 2];
                    textureData[i + 2] = blue;
                }
                bitmapData = bitmap.LockBits(ConvertRectangle(rect), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                safePtr = bitmapData.Scan0;
                Marshal.Copy(textureData, 0, safePtr, textureData.Length);
                bitmap.UnlockBits(bitmapData);

                bitmap.Save(stream, ImageFormat.Png);
            }

            return stream;
        }
        public static System.Drawing.Color ConvertColor(xColor cXNA)
        {
            return System.Drawing.Color.FromArgb(cXNA.R, cXNA.G, cXNA.B);
        }
        public static System.Drawing.Rectangle ConvertRectangle(Rectangle rXNA)
        {
            return new System.Drawing.Rectangle(rXNA.X, rXNA.Y, rXNA.Width, rXNA.Height);
        }

        private Bitmap StardewToNative(StardewBitmap source)
        {
            return new Bitmap(source.SourceStream());
        }
        private static MemoryStream ReadImage(string sImage)
        {
            MemoryStream ms = new MemoryStream();

            using (FileStream fs = new FileStream(sImage, FileMode.Open, FileAccess.Read))
            {
                fs.CopyTo(ms);
            }

            return ms;
        }

        private static Image GetImage(string sFilename)
        {
            Bitmap originalBmp = new Bitmap(Image.FromFile(sFilename));
            Bitmap tempBitmap = new Bitmap(originalBmp.Width, originalBmp.Height);

            using (Graphics g = Graphics.FromImage(tempBitmap))
            {
                g.DrawImage(originalBmp, 0, 0);
            }

            return tempBitmap;
        }

    #endregion
    }


#endif
}
