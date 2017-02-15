using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OCRTest
{
    public static class FilterBitmap
    {
        private static Bitmap GetArgbCopy(Image sourceImage)
        {
            Bitmap bmpNew = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(bmpNew))
            {
                graphics.DrawImage(sourceImage, new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), GraphicsUnit.Pixel);
                graphics.Flush();
            }

            return bmpNew;
        }

        private static Bitmap ApplyColorMatrix(Image sourceImage, ColorMatrix colorMatrix)
        {
            Bitmap bmp32BppSource = GetArgbCopy(sourceImage);
            Bitmap bmp32BppDest = new Bitmap(bmp32BppSource.Width, bmp32BppSource.Height, PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(bmp32BppDest))
            {
                ImageAttributes bmpAttributes = new ImageAttributes();
                bmpAttributes.SetColorMatrix(colorMatrix);

                graphics.DrawImage(bmp32BppSource, new Rectangle(0, 0, bmp32BppSource.Width, bmp32BppSource.Height),
                                 0, 0, bmp32BppSource.Width, bmp32BppSource.Height, GraphicsUnit.Pixel, bmpAttributes);

            }

            bmp32BppSource.Dispose();

            return bmp32BppDest;
        }

        public static Bitmap CopyAsGrayscale(this Image sourceImage)
        {
            Bitmap bmpNew = GetArgbCopy(sourceImage);
            BitmapData bmpData = bmpNew.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            IntPtr ptr = bmpData.Scan0;

            byte[] byteBuffer = new byte[bmpData.Stride * bmpNew.Height];

            System.Runtime.InteropServices.Marshal.Copy(ptr, byteBuffer, 0, byteBuffer.Length);

            float rgb = 0;

            for (int k = 0; k < byteBuffer.Length; k += 4)
            {
                rgb = byteBuffer[k] * 0.11f;
                rgb += byteBuffer[k + 1] * 0.59f;
                rgb += byteBuffer[k + 2] * 0.3f;

                byteBuffer[k] = (byte)rgb;
                byteBuffer[k + 1] = byteBuffer[k];
                byteBuffer[k + 2] = byteBuffer[k];

                byteBuffer[k + 3] = 255;
            }

            Marshal.Copy(byteBuffer, 0, ptr, byteBuffer.Length);

            bmpNew.UnlockBits(bmpData);

            bmpData = null;
            byteBuffer = null;

            return bmpNew;
        }

        public static Bitmap DrawAsGrayscale(this Image sourceImage)
        {
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                                                {
                                                    //new float[] {.3f, .3f, .3f, 0, 0},  //red
                                                    //new float[] {.59f, .59f, .59f, 0, 0}, // green
                                                    //new float[] {.11f, .11f, .11f, 0, 0}, // blue
                                                    //new float[] {0, 0, 0, 1, 0},   //amber
                                                    //new float[] {0, 0, 0, 0, 1}    // white
                                                    new float[] { 0.299f, 0.299f, 0.299f, 0, 0 },
                                                    new float[] { 0.587f, 0.587f, 0.587f, 0, 0 },
                                                    new float[] { 0.114f, 0.114f, 0.114f, 0, 0 },
                                                    new float[] { 0,      0,      0,      1, 0 },
                                                    new float[] { 0,      0,      0,      0, 1 }
                                                });

            return ApplyColorMatrix(sourceImage, colorMatrix);
        }


    }
}
