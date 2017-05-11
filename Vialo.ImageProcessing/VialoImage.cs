using System.Collections.Generic;
using System.Drawing;
using static Vialo.ImageProcessing.Helper;
using CNTK;
using System.IO;
using System;

namespace Vialo.ImageProcessing
{
    public class VialoImage
    {
        private Bitmap bmp;
        private string path;

        public Image GetImage()
        {
            return bmp;
        }

        public VialoImage Init(Image image)
        {
            bmp = new Bitmap(image);
            return this;
        }

        public VialoImage Init(string path)
        {
            this.path = path;
            bmp = new Bitmap(path);
            return this;
        }

        public List<Bitmap> Evaluate(string modelFilePath)
        {

            var images = Fragment(32);
            var list = Evaluator.EvaluationBatchOfImages(DeviceDescriptor.CPUDevice, images, modelFilePath);

            return list;
        }

        public List<Bitmap> Fragment(int size)
        {
            var list = new List<Bitmap>();
            var width = bmp.Width;
            var height = bmp.Height;
            var horizontalMargin = width % size / 2;
            var verticalMargin = height % size / 2;

            for (var i = vMargin; i + size <= h - vMargin; i += size)
            {
                for (var j = hMargin; j + size <= w - hMargin; j += size)

                {
                    if ((i + size) > (height - verticalMargin) || (j + size) > (width - horizontalMargin))
                    {
                        Console.WriteLine("i={0}, j={1}", i, j);
                        continue;
                    }
                    list.Add(bmp.Clone(new Rectangle(j, i, size, size), bmp.PixelFormat));
                }
            }
            return list;
        }

        public VialoImage BooleanEdgeDetectionFilter(
                                BooleanFilterType filterType,
                                float redFactor = 1.0f,
                                float greenFactor = 1.0f,
                                float blueFactor = 1.0f,
                                byte threshold = 0)
        {
            Bitmap result = Helper.BooleanEdgeDetectionFilter(bmp, filterType, redFactor, greenFactor, blueFactor, threshold);
            bmp = result;
            return this;
        }

        public VialoImage Contrast(int threshold)
        {
            Bitmap result = Helper.Contrast(bmp, threshold);
            bmp = result;
            return this;
        }

        public VialoImage MarkForeground()
        {
            Bitmap result = Helper.MarkForeground(bmp);
            bmp = result;
            return this;
        }

        public VialoImage MarkForeground(int matrixSize)
        {
            Bitmap result = Helper.MarkForeground(bmp, matrixSize);
            bmp = result;
            return this;
        }

        public VialoImage ConvolutionFilter(double[,] filterMatrix,
                                          double factor = 1,
                                               int bias = 0,
                                     bool grayscale = false)
        {
            Bitmap result = Helper.ConvolutionFilter(bmp, filterMatrix, factor, bias, grayscale);
            bmp = result;
            return this;
        }

        public VialoImage ConvolutionFilter(double[,] xFilterMatrix,
                                        double[,] yFilterMatrix,
                                              double factor = 1,
                                                   int bias = 0,
                                         bool grayscale = false)
        {
            Bitmap result = Helper.ConvolutionFilter(bmp, xFilterMatrix, yFilterMatrix, factor, bias, grayscale);
            bmp = result;
            return this;
        }

        public VialoImage PrewittFilter(bool grayscale = true)
        {
            Bitmap result = Helper.PrewittFilter(bmp, grayscale);
            bmp = result;
            return this;
        }

        public VialoImage GaussianFilter(bool grayscale = true)
        {
            Bitmap result = Helper.GaussianFilter(bmp, grayscale);
            bmp = result;
            return this;
        }

        public VialoImage DilateAndErodeFilter(
                           int matrixSize,
                           //MorphologyType morphType,
                           bool applyBlue = true,
                           bool applyGreen = true,
                           bool applyRed = true)
        {
            Bitmap result = Helper.DilateAndErodeFilter(bmp, matrixSize, applyBlue, applyGreen, applyRed);
            bmp = result;
            return this;
        }

        public VialoImage MedianFilter(int matrixSize,
                                              int bias = 0,
                                    bool grayscale = false)
        {
            Bitmap result = Helper.MedianFilter(bmp, matrixSize, bias, grayscale);
            bmp = result;
            return this;
        }
        public VialoImage GradientBasedEdgeDetectionFilter(
                                EdgeFilterType filterType,
                                int derivativeLevel,
                                float redFactor = 1.0f,
                                float greenFactor = 1.0f,
                                float blueFactor = 1.0f,
                                byte threshold = 0)
        {
            Bitmap result = Helper.GradientBasedEdgeDetectionFilter(bmp, filterType, derivativeLevel, redFactor, greenFactor, blueFactor, threshold);
            bmp = result;
            return this;
        }
        public void Save(string path)
        {
            bmp.Save(path);
        }
    }
}
