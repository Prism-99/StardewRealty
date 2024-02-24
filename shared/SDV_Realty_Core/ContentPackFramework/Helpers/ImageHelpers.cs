using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using StardewValley;

namespace SDV_Realty_Core.ContentPackFramework.Helpers
{
    class ImageHelpers
    {
        public static Texture2D LoadTextureFromImage(Image iSource)
        {
            Texture2D tNewTexture;
            using (MemoryStream ms = new MemoryStream())
            {
                iSource.Save(ms, ImageFormat.Png);
                tNewTexture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, ms);
            }
            return tNewTexture;
        }
        public static MemoryStream GetTextureStream(Texture2D tTexture)
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
                bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                safePtr = bitmapData.Scan0;
                Marshal.Copy(textureData, 0, safePtr, textureData.Length);
                bitmap.UnlockBits(bitmapData);

                bitmap.Save(stream, ImageFormat.Png);
            }

            return stream;
        }
    }
}
