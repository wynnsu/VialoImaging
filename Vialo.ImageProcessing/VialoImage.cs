using System.Collections.Generic;
using System.Drawing;
using static Vialo.ImageProcessing.Helper;
using Vialo.Evaluate;
using CNTK;

namespace Vialo.ImageProcessing
{
    public class VialoImage
    {
        private Bitmap bmp;

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
            bmp = new Bitmap(path);
            return this;
        }

        public List<Bitmap> Evaluation()
        {
            string modelFilePath = "ConvNet_CIFAR10_DataAug_14.dnn";
            var list=Evaluator.EvaluationBatchOfImages(DeviceDescriptor.CPUDevice, Fragment(32), modelFilePath);
            return list;
        }

        public List<Bitmap> Fragment(int size)
        {
            var list = new List<Bitmap>();
            var w = bmp.Width;
            var h = bmp.Height;
            var hMargin = w % size / 2;
            var vMargin = h % size / 2;

            for (var i = vMargin; i < h - vMargin; i += size)
            {
                for (var j = hMargin; j < w - hMargin; j += size)
                {
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
