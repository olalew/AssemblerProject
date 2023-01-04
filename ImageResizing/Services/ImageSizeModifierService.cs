using ImageResizing.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizing.Services
{
    public class ImageSizeModifierService
    {
        [DllImport(@"ImageResizingASM.dll")]
        private static extern int MyProc(byte[] source, byte[] target, int y);
        [DllImport(@"ImageResizingASM.dll")]
        private static extern void SetParameters(int w1, int w2, int h1, int h2);

        private readonly Bitmap targetBitmap;

        private ExcecutionConfiguration ExcecutionConfiguration { get; set; }

        public ImageSizeModifierService(ExcecutionConfiguration configuration)
        {
            ExcecutionConfiguration = configuration;
            targetBitmap = new Bitmap(configuration.Width, configuration.Height);
        }

        public (Bitmap, long) ExecuteAlgorithm()
        {
            Rectangle initialRectangle = new(0, 0, ExcecutionConfiguration.InitialBitmap.Width, ExcecutionConfiguration.InitialBitmap.Height);
            Rectangle targetRectangle = new(0, 0, targetBitmap.Width, targetBitmap.Height);

            System.Drawing.Imaging.BitmapData initBitmapData =
                ExcecutionConfiguration.InitialBitmap.LockBits(initialRectangle, System.Drawing.Imaging.ImageLockMode.ReadWrite, ExcecutionConfiguration.InitialBitmap.PixelFormat);

            System.Drawing.Imaging.BitmapData targetBitmapData =
               targetBitmap.LockBits(targetRectangle, System.Drawing.Imaging.ImageLockMode.ReadWrite, targetBitmap.PixelFormat);

            // Get the address of the first line.
            IntPtr initBitmapPointer = initBitmapData.Scan0;
            IntPtr targetBitmapPointer = targetBitmapData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int inputBytesCount = Math.Abs(initBitmapData.Stride) * ExcecutionConfiguration.InitialBitmap.Height;
            int outputBytesCount = Math.Abs(targetBitmapData.Stride) * targetBitmap.Height;

            byte[] initImageRGB = new byte[inputBytesCount];
            byte[] targetImageRGB = new byte[outputBytesCount];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(initBitmapPointer, initImageRGB, 0, inputBytesCount);

            long executionTime = 0;
            if (ExcecutionConfiguration.IsAssembly)
            {
                executionTime = ExecuteOnBytesWithAssebmly(initImageRGB, ExcecutionConfiguration.InitialBitmap.Width, ExcecutionConfiguration.InitialBitmap.Height,
                  targetImageRGB, targetBitmap.Width, targetBitmap.Height);
            }
            else {
                executionTime = ExecuteOnBytes(initImageRGB, ExcecutionConfiguration.InitialBitmap.Width, ExcecutionConfiguration.InitialBitmap.Height,
                     targetImageRGB, targetBitmap.Width, targetBitmap.Height);
            }
           

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(targetImageRGB, 0, targetBitmapPointer, outputBytesCount);

            // Unlock the bits.
            ExcecutionConfiguration.InitialBitmap.UnlockBits(initBitmapData);
            targetBitmap.UnlockBits(targetBitmapData);

            // Draw the modified image.
            return (targetBitmap, executionTime);
        }

        private long ExecuteOnBytes(byte[] initArray, int w1, int h1, byte[] targetArray, int w2, int h2)
        {
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

            int widthRatio = ((w1 << 16) / w2) + 1;
            int heightRatio = ((h1 << 16) / h2) + 1;

            _ = Parallel.For(0, h2, new ParallelOptions { MaxDegreeOfParallelism = ExcecutionConfiguration.ThreadCount }, y =>
              {
                  int x2, y2;

                  y2 = (y * heightRatio) >> 16;
                  for (int j = 0; j < w2; j++)
                  {
                      x2 = (j * widthRatio) >> 16;

                      int sourceIndex = ((y2 * w1) + x2) * 4;
                      int targetIndex = ((y * w2) + j) * 4;

                    /*
                        COPY PIXEL FROM ONE ARRAY TO THE OTHER
                     */
                      targetArray[targetIndex] = initArray[sourceIndex];
                      targetArray[targetIndex + 1] = initArray[sourceIndex + 1];
                      targetArray[targetIndex + 2] = initArray[sourceIndex + 2];
                      targetArray[targetIndex + 3] = initArray[sourceIndex + 3];
                  }
              });

            watch.Stop();

            return watch.ElapsedMilliseconds;
        }

        private long ExecuteOnBytesWithAssebmly(byte[] initArray, int w1, int h1, byte[] targetArray, int w2, int h2)
        {
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            SetParameters(w1, w2, h1, h2);

            _ = Parallel.For(0, h2, new ParallelOptions { MaxDegreeOfParallelism = ExcecutionConfiguration.ThreadCount }, y =>
              {
                  MyProc(initArray, targetArray, y);
              });

            watch.Stop();
            
            return watch.ElapsedMilliseconds;
        }
    }
}
