using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using CNTK;

namespace Vialo.ImageProcessing
{
    public static class Helper
    {

        public static List<Bitmap> EvaluationBatchOfImages(DeviceDescriptor device, List<Bitmap> images, string modelPath)
        {
            var result = new List<Bitmap>();
            var evalResult = new List<float>();
            try
            {
                // Load the model.
                Function modelFunc = Function.LoadModel(modelPath, device);

                // Get input variable. The model has only one single input.
                // The same way described above for output variable can be used here to get input variable by name.
                Variable inputVar = modelFunc.Arguments.Single();

                // Get shape data for the input variable
                NDShape inputShape = inputVar.Shape;
                int imageWidth = inputShape[0];
                int imageHeight = inputShape[1];
                int imageChannels = inputShape[2];
                int imageSize = inputShape.TotalSize;

                // The model has only one output.
                // If the model have more than one output, use the following way to get output variable by name.
                // Variable outputVar = modelFunc.Outputs.Where(variable => string.Equals(variable.Name, outputName)).Single();
                Variable outputVar = modelFunc.Output;

                var inputDataMap = new Dictionary<Variable, Value>();
                var outputDataMap = new Dictionary<Variable, Value>();

                Bitmap bmp, resized;
                List<float> resizedCHW;
                var seqData = new List<float>();

                for (int sampleIndex = 0; sampleIndex < images.Count; sampleIndex++)
                {
                    bmp = images[sampleIndex];
                    resized = bmp.Resize((int)imageWidth, (int)imageHeight, true);
                    resizedCHW = resized.ParallelExtractCHW();
                    // Aadd this sample to the data buffer.
                    seqData.AddRange(resizedCHW);
                }

                // Create Value for the batch data.
                var inputVal = Value.CreateBatch(inputVar.Shape, seqData, device);
                // Create input data map.
                inputDataMap.Add(inputVar, inputVal);

                // Create ouput data map. Using null as Value to indicate using system allocated memory.
                // Alternatively, create a Value object and add it to the data map.
                outputDataMap.Add(outputVar, null);

                // Evaluate the model against the batch input
                modelFunc.Evaluate(inputDataMap, outputDataMap, device);

                // Retrieve the evaluation result.
                var outputVal = outputDataMap[outputVar];
                var outputData = outputVal.GetDenseData<float>(outputVar);

                // Output result
                int resultCount = 0;
                foreach (var seq in outputData)
                {
                    foreach (var element in seq)
                    {
                        resultCount++;
                        evalResult.Add(element);
                    }
                }

                for (int i = 0; i < evalResult.Count; i += 2)
                {
                    if (evalResult[i].CompareTo(0.0f) > 0)
                    {
                        result.Add(images[i / 2]);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}\nCallStack: {1}\n Inner Exception: {2}", ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : "No Inner Exception");
                throw ex;
            }
            return result;
        }

        public static Bitmap BooleanEdgeDetectionFilter(
                                this Bitmap sourceBitmap,
                                BooleanFilterType filterType,
                                float redFactor = 1.0f,
                                float greenFactor = 1.0f,
                                float blueFactor = 1.0f,
                                byte threshold = 0)
        {
            BitmapData sourceData =
                       sourceBitmap.LockBits(new Rectangle(0, 0,
                       sourceBitmap.Width, sourceBitmap.Height),
                       ImageLockMode.ReadOnly,
                       PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[sourceData.Stride *
                                          sourceData.Height];

            byte[] resultBuffer = new byte[sourceData.Stride *
                                           sourceData.Height];


            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0,
                                       pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);


            List<string> edgeMasks = new List<string>();


            edgeMasks.Add("011011011");
            edgeMasks.Add("000111111");
            edgeMasks.Add("110110110");
            edgeMasks.Add("111111000");
            edgeMasks.Add("011011001");
            edgeMasks.Add("100110110");
            edgeMasks.Add("111011000");
            edgeMasks.Add("111110000");
            edgeMasks.Add("111011001");
            edgeMasks.Add("100110111");
            edgeMasks.Add("001011111");
            edgeMasks.Add("111110100");
            edgeMasks.Add("000011111");
            edgeMasks.Add("000110111");
            edgeMasks.Add("001011011");
            edgeMasks.Add("001011011");
            edgeMasks.Add("110110100");


            int filterOffset = 1;
            int calcOffset = 0;


            int byteOffset = 0;
            int matrixMean = 0;
            int matrixTotal = 0;
            double matrixVariance = 0;


            double blueValue = 0;
            double greenValue = 0;
            double redValue = 0;


            string matrixPatern = String.Empty;


            for (int offsetY = filterOffset; offsetY <
                sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    sourceBitmap.Width - filterOffset; offsetX++)
                {
                    byteOffset = offsetY *
                                 sourceData.Stride +
                                 offsetX * 4;


                    matrixMean = 0;
                    matrixTotal = 0;
                    matrixVariance = 0;


                    matrixPatern = String.Empty;


                    //Step 1: Calculate local matrix  
                    for (int filterY = -filterOffset;
                        filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset;
                            filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset +
                                         (filterX * 4) +
                            (filterY * sourceData.Stride);


                            matrixMean += pixelBuffer[calcOffset];
                            matrixMean += pixelBuffer[calcOffset + 1];
                            matrixMean += pixelBuffer[calcOffset + 2];
                        }
                    }


                    matrixMean = matrixMean / 9;


                    //Step 4: Calculate Variance  
                    for (int filterY = -filterOffset;
                        filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset;
                            filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset +
                                         (filterX * 4) +
                            (filterY * sourceData.Stride);


                            matrixTotal = pixelBuffer[calcOffset];
                            matrixTotal += pixelBuffer[calcOffset + 1];
                            matrixTotal += pixelBuffer[calcOffset + 2];


                            matrixPatern += (matrixTotal > matrixMean
                                                         ? "1" : "0");


                            matrixVariance +=
                                Math.Pow(matrixMean -
                                (pixelBuffer[calcOffset] +
                                pixelBuffer[calcOffset + 1] +
                                pixelBuffer[calcOffset + 2]), 2);
                        }
                    }


                    matrixVariance = matrixVariance / 9;


                    if (filterType == BooleanFilterType.Sharpen)
                    {
                        blueValue = pixelBuffer[byteOffset];
                        greenValue = pixelBuffer[byteOffset + 1];
                        redValue = pixelBuffer[byteOffset + 2];


                        //Step 4: Exlclude noise using global  
                        // threshold  
                        if (matrixVariance > threshold)
                        {   //Step 2: Compare newly calculated  
                            // matrix and image masks  
                            if (edgeMasks.Contains(matrixPatern))
                            {
                                blueValue = (blueValue * blueFactor);
                                greenValue = (greenValue * greenFactor);
                                redValue = (redValue * redFactor);


                                blueValue = (blueValue > 255 ? 255 :
                                            (blueValue < 0 ? 0 :
                                             blueValue));


                                greenValue = (greenValue > 255 ? 255 :
                                             (greenValue < 0 ? 0 :
                                              greenValue));


                                redValue = (redValue > 255 ? 255 :
                                           (redValue < 0 ? 0 :
                                            redValue));
                            }
                        }
                    }    //Step 4: Exlclude noise using global  
                         // threshold  
                         //Step 2: Compare newly calculated  
                         // matrix and image masks  
                    else if (matrixVariance > threshold &&
                            edgeMasks.Contains(matrixPatern))
                    {
                        blueValue = 255;
                        greenValue = 255;
                        redValue = 255;
                    }
                    else
                    {
                        blueValue = 0;
                        greenValue = 0;
                        redValue = 0;
                    }


                    resultBuffer[byteOffset] = (byte)blueValue;
                    resultBuffer[byteOffset + 1] = (byte)greenValue;
                    resultBuffer[byteOffset + 2] = (byte)redValue;
                    resultBuffer[byteOffset + 3] = 255;
                }
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width,
                                             sourceBitmap.Height);


            BitmapData resultData =
                       resultBitmap.LockBits(new Rectangle(0, 0,
                       resultBitmap.Width, resultBitmap.Height),
                       ImageLockMode.WriteOnly,
                       PixelFormat.Format32bppArgb);


            Marshal.Copy(resultBuffer, 0, resultData.Scan0,
                                       resultBuffer.Length);


            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }
        public static Bitmap Contrast(this Bitmap sourceBitmap, int threshold)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                                        sourceBitmap.Width, sourceBitmap.Height),
                                        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];


            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);


            double contrastLevel = Math.Pow((100.0 + threshold) / 100.0, 2);


            double blue = 0;
            double green = 0;
            double red = 0;


            for (int k = 0; k + 4 < pixelBuffer.Length; k += 4)
            {
                blue = ((((pixelBuffer[k] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;


                green = ((((pixelBuffer[k + 1] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;


                red = ((((pixelBuffer[k + 2] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;


                if (blue > 255)
                { blue = 255; }
                else if (blue < 0)
                { blue = 0; }


                if (green > 255)
                { green = 255; }
                else if (green < 0)
                { green = 0; }


                if (red > 255)
                { red = 255; }
                else if (red < 0)
                { red = 0; }


                pixelBuffer[k] = (byte)blue;
                pixelBuffer[k + 1] = (byte)green;
                pixelBuffer[k + 2] = (byte)red;
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);


            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0,
                                        resultBitmap.Width, resultBitmap.Height),
                                        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);


            Marshal.Copy(pixelBuffer, 0, resultData.Scan0, pixelBuffer.Length);
            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }
        public static Bitmap MarkForeground(this Bitmap sourceBitmap)
        {
            BitmapData sourceData =
                       sourceBitmap.LockBits(new Rectangle(0, 0,
                       sourceBitmap.Width, sourceBitmap.Height),
                       ImageLockMode.ReadOnly,
                       PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[sourceData.Stride *
                                          sourceData.Height];


            byte[] resultBuffer = new byte[sourceData.Stride *
                                           sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0,
                                      pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);

            //float rgb = 0;


            for (int k = 0; k < pixelBuffer.Length; k += 4)
            {
                if (pixelBuffer[k] > 5)
                {
                    resultBuffer[k] = 255;
                    resultBuffer[k + 1] = 255;
                    resultBuffer[k + 2] = 255;
                    resultBuffer[k + 3] = 255;
                }
                else
                {
                    resultBuffer[k] = 0;
                    resultBuffer[k + 1] = 0;
                    resultBuffer[k + 2] = 0;
                    resultBuffer[k + 3] = 255;
                }
            }
            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width,
                                            sourceBitmap.Height);


            BitmapData resultData =
                       resultBitmap.LockBits(new Rectangle(0, 0,
                       resultBitmap.Width, resultBitmap.Height),
                       ImageLockMode.WriteOnly,
                       PixelFormat.Format32bppArgb);


            Marshal.Copy(resultBuffer, 0, resultData.Scan0,
                                                   resultBuffer.Length);


            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }
        public static Bitmap MarkForeground(this Bitmap sourceBitmap,
    int matrixSize)
        {
            BitmapData sourceData =
                       sourceBitmap.LockBits(new Rectangle(0, 0,
                       sourceBitmap.Width, sourceBitmap.Height),
                       ImageLockMode.ReadOnly,
                       PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[sourceData.Stride *
                                          sourceData.Height];


            byte[] resultBuffer = new byte[sourceData.Stride *
                                           sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0,
                                      pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);

            float rgb = 0;


            for (int k = 0; k < pixelBuffer.Length; k += 4)
            {
                rgb = pixelBuffer[k] * 0.11f;
                rgb += pixelBuffer[k + 1] * 0.59f;
                rgb += pixelBuffer[k + 2] * 0.3f;


                pixelBuffer[k] = (byte)rgb;
                pixelBuffer[k + 1] = pixelBuffer[k];
                pixelBuffer[k + 2] = pixelBuffer[k];
                pixelBuffer[k + 3] = 255;

            }

            int filterOffset = (matrixSize - 1) / 2;
            int calcOffset = 0;


            int byteOffset = 0;

            List<int> neighbourPixels = new List<int>();
            byte[] currentPixel;

            for (int offsetY = filterOffset; offsetY <
               sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    sourceBitmap.Width - filterOffset; offsetX++)
                {
                    byteOffset = offsetY *
                                 sourceData.Stride +
                                 offsetX * 4;


                    neighbourPixels.Clear();

                    for (int filterY = -filterOffset;
                        filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset;
                            filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset +
                                         (filterX * 4) +
                                (filterY * sourceData.Stride);
                            neighbourPixels.Add(BitConverter.ToInt32(
                                         pixelBuffer, calcOffset));
                        }
                    }

                    byte[] avg = BitConverter.GetBytes(neighbourPixels.Average());


                    currentPixel = BitConverter.GetBytes(
                                       neighbourPixels[filterOffset]);

                    if (Math.Abs(avg[0] - currentPixel[0]) < 128)
                    {
                        resultBuffer[byteOffset] = 0;
                        resultBuffer[byteOffset + 1] = 0;
                        resultBuffer[byteOffset + 2] = 255;
                        resultBuffer[byteOffset + 3] = 255;
                    }
                    else
                    {

                        resultBuffer[byteOffset] = currentPixel[0];
                        resultBuffer[byteOffset + 1] = currentPixel[1];
                        resultBuffer[byteOffset + 2] = currentPixel[2];
                        resultBuffer[byteOffset + 3] = currentPixel[3];
                    }

                }
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width,
                                             sourceBitmap.Height);


            BitmapData resultData =
                       resultBitmap.LockBits(new Rectangle(0, 0,
                       resultBitmap.Width, resultBitmap.Height),
                       ImageLockMode.WriteOnly,
                       PixelFormat.Format32bppArgb);


            Marshal.Copy(resultBuffer, 0, resultData.Scan0,
                                                   resultBuffer.Length);


            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }

        public static Bitmap ConvolutionFilter(Bitmap sourceBitmap,
                                     double[,] filterMatrix,
                                          double factor = 1,
                                               int bias = 0,
                                     bool grayscale = false)
        {
            BitmapData sourceData =
                           sourceBitmap.LockBits(new Rectangle(0, 0,
                           sourceBitmap.Width, sourceBitmap.Height),
                                             ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[sourceData.Stride *
                                          sourceData.Height];


            byte[] resultBuffer = new byte[sourceData.Stride *
                                           sourceData.Height];


            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0,
                                       pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);


            if (grayscale == true)
            {
                float rgb = 0;


                for (int k = 0; k < pixelBuffer.Length; k += 4)
                {
                    rgb = pixelBuffer[k] * 0.11f;
                    rgb += pixelBuffer[k + 1] * 0.59f;
                    rgb += pixelBuffer[k + 2] * 0.3f;


                    pixelBuffer[k] = (byte)rgb;
                    pixelBuffer[k + 1] = pixelBuffer[k];
                    pixelBuffer[k + 2] = pixelBuffer[k];
                    pixelBuffer[k + 3] = 255;
                }
            }


            double blue = 0.0;
            double green = 0.0;
            double red = 0.0;


            int filterWidth = filterMatrix.GetLength(1);
            int filterHeight = filterMatrix.GetLength(0);


            int filterOffset = (filterWidth - 1) / 2;
            int calcOffset = 0;


            int byteOffset = 0;


            for (int offsetY = filterOffset; offsetY <
                sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    sourceBitmap.Width - filterOffset; offsetX++)
                {
                    blue = 0;
                    green = 0;
                    red = 0;


                    byteOffset = offsetY *
                                 sourceData.Stride +
                                 offsetX * 4;


                    for (int filterY = -filterOffset;
                        filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset;
                            filterX <= filterOffset; filterX++)
                        {


                            calcOffset = byteOffset +
                                         (filterX * 4) +
                                         (filterY * sourceData.Stride);


                            blue += (double)(pixelBuffer[calcOffset]) *
                                    filterMatrix[filterY + filterOffset,
                                                 filterX + filterOffset];


                            green += (double)(pixelBuffer[calcOffset + 1]) *
                                     filterMatrix[filterY + filterOffset,
                                                  filterX + filterOffset];


                            red += (double)(pixelBuffer[calcOffset + 2]) *
                                   filterMatrix[filterY + filterOffset,
                                                filterX + filterOffset];
                        }
                    }


                    blue = factor * blue + bias;
                    green = factor * green + bias;
                    red = factor * red + bias;


                    if (blue > 255)
                    { blue = 255; }
                    else if (blue < 0)
                    { blue = 0; }


                    if (green > 255)
                    { green = 255; }
                    else if (green < 0)
                    { green = 0; }


                    if (red > 255)
                    { red = 255; }
                    else if (red < 0)
                    { red = 0; }


                    resultBuffer[byteOffset] = (byte)(blue);
                    resultBuffer[byteOffset + 1] = (byte)(green);
                    resultBuffer[byteOffset + 2] = (byte)(red);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width,
                                            sourceBitmap.Height);


            BitmapData resultData =
                       resultBitmap.LockBits(new Rectangle(0, 0,
                       resultBitmap.Width, resultBitmap.Height),
                                        ImageLockMode.WriteOnly,
                                    PixelFormat.Format32bppArgb);


            Marshal.Copy(resultBuffer, 0, resultData.Scan0,
                                       resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }
        public static Bitmap ConvolutionFilter(this Bitmap sourceBitmap,
                                        double[,] xFilterMatrix,
                                        double[,] yFilterMatrix,
                                              double factor = 1,
                                                   int bias = 0,
                                         bool grayscale = false)
        {
            BitmapData sourceData =
                           sourceBitmap.LockBits(new Rectangle(0, 0,
                           sourceBitmap.Width, sourceBitmap.Height),
                                             ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[sourceData.Stride *
                                          sourceData.Height];


            byte[] resultBuffer = new byte[sourceData.Stride *
                                           sourceData.Height];


            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0,
                                       pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);


            if (grayscale == true)
            {
                float rgb = 0;


                for (int k = 0; k < pixelBuffer.Length; k += 4)
                {
                    rgb = pixelBuffer[k] * 0.11f;
                    rgb += pixelBuffer[k + 1] * 0.59f;
                    rgb += pixelBuffer[k + 2] * 0.3f;


                    pixelBuffer[k] = (byte)rgb;
                    pixelBuffer[k + 1] = pixelBuffer[k];
                    pixelBuffer[k + 2] = pixelBuffer[k];
                    pixelBuffer[k + 3] = 255;
                }
            }


            double blueX = 0.0;
            double greenX = 0.0;
            double redX = 0.0;


            double blueY = 0.0;
            double greenY = 0.0;
            double redY = 0.0;


            double blueTotal = 0.0;
            double greenTotal = 0.0;
            double redTotal = 0.0;


            int filterOffset = 1;
            int calcOffset = 0;


            int byteOffset = 0;


            for (int offsetY = filterOffset; offsetY <
                sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    sourceBitmap.Width - filterOffset; offsetX++)
                {
                    blueX = greenX = redX = 0;
                    blueY = greenY = redY = 0;


                    blueTotal = greenTotal = redTotal = 0.0;


                    byteOffset = offsetY *
                                 sourceData.Stride +
                                 offsetX * 4;


                    for (int filterY = -filterOffset;
                        filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset;
                            filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset +
                                         (filterX * 4) +
                                         (filterY * sourceData.Stride);


                            blueX += (double)
                                      (pixelBuffer[calcOffset]) *
                                      xFilterMatrix[filterY +
                                                    filterOffset,
                                                    filterX +
                                                    filterOffset];


                            greenX += (double)
                                  (pixelBuffer[calcOffset + 1]) *
                                      xFilterMatrix[filterY +
                                                    filterOffset,
                                                    filterX +
                                                    filterOffset];


                            redX += (double)
                                  (pixelBuffer[calcOffset + 2]) *
                                      xFilterMatrix[filterY +
                                                    filterOffset,
                                                    filterX +
                                                    filterOffset];


                            blueY += (double)
                                      (pixelBuffer[calcOffset]) *
                                      yFilterMatrix[filterY +
                                                    filterOffset,
                                                    filterX +
                                                    filterOffset];


                            greenY += (double)
                                  (pixelBuffer[calcOffset + 1]) *
                                      yFilterMatrix[filterY +
                                                    filterOffset,
                                                    filterX +
                                                    filterOffset];


                            redY += (double)
                                  (pixelBuffer[calcOffset + 2]) *
                                      yFilterMatrix[filterY +
                                                    filterOffset,
                                                    filterX +
                                                    filterOffset];
                        }
                    }


                    blueTotal = Math.Sqrt((blueX * blueX) +
                                          (blueY * blueY));


                    greenTotal = Math.Sqrt((greenX * greenX) +
                                           (greenY * greenY));


                    redTotal = Math.Sqrt((redX * redX) +
                                         (redY * redY));


                    if (blueTotal > 255)
                    { blueTotal = 255; }
                    else if (blueTotal < 0)
                    { blueTotal = 0; }


                    if (greenTotal > 255)
                    { greenTotal = 255; }
                    else if (greenTotal < 0)
                    { greenTotal = 0; }


                    if (redTotal > 255)
                    { redTotal = 255; }
                    else if (redTotal < 0)
                    { redTotal = 0; }


                    resultBuffer[byteOffset] = (byte)(blueTotal);
                    resultBuffer[byteOffset + 1] = (byte)(greenTotal);
                    resultBuffer[byteOffset + 2] = (byte)(redTotal);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width,
                                             sourceBitmap.Height);


            BitmapData resultData =
                       resultBitmap.LockBits(new Rectangle(0, 0,
                       resultBitmap.Width, resultBitmap.Height),
                                        ImageLockMode.WriteOnly,
                                    PixelFormat.Format32bppArgb);


            Marshal.Copy(resultBuffer, 0, resultData.Scan0,
                                       resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }

        public static Bitmap PrewittFilter(this Bitmap sourceBitmap,
                 bool grayscale = true)
        {
            Bitmap resultBitmap =
                   ConvolutionFilter(sourceBitmap,
                                Prewitt3x3Horizontal,
                                  Prewitt3x3Vertical,
                                          1.0, 0, grayscale);


            return resultBitmap;
        }

        public static double[,] Prewitt3x3Horizontal
        {
            get
            {
                return new double[,]
                { { -1,  0,  1, },
                { -1,  0,  1, },
                { -1,  0,  1, }, };
            }
        }
        public static double[,] Prewitt3x3Vertical
        {
            get
            {
                return new double[,]
                { {  1,  1,  1, },
         {  0,  0,  0, },
         { -1, -1, -1, }, };
            }
        }

        public static double[,] Gaussian3x3
        {
            get
            {
                return new double[,]
                { { 1, 2, 1, },
              { 2, 4, 2, },
              { 1, 2, 1, }, };
            }
        }


        public static double[,] Gaussian5x5Type1
        {
            get
            {
                return new double[,]
                 { { 2, 04, 05, 04, 2  },
               { 4, 09, 12, 09, 4  },
               { 5, 12, 15, 12, 5  },
               { 4, 09, 12, 09, 4  },
               { 2, 04, 05, 04, 2  }, };
            }
        }


        public static double[,] Gaussian5x5Type2
        {
            get
            {
                return new double[,]
                 { {  1,   4,  6,  4,  1  },
               {  4,  16, 24, 16,  4  },
               {  6,  24, 36, 24,  6  },
               {  4,  16, 24, 16,  4  },
               {  1,   4,  6,  4,  1  }, };
            }
        }


        public static Bitmap GaussianFilter(this Bitmap sourceBitmap,
                 bool grayscale = true)
        {
            Bitmap resultBitmap =
                   ConvolutionFilter(sourceBitmap,
                                Gaussian3x3, 0.0625, 0);
            return resultBitmap;
        }

        public static Bitmap DilateAndErodeFilter(
                           this Bitmap sourceBitmap,
                           int matrixSize,
                           //MorphologyType morphType,
                           bool applyBlue = true,
                           bool applyGreen = true,
                           bool applyRed = true)
        {
            BitmapData sourceData =
                       sourceBitmap.LockBits(new Rectangle(0, 0,
                       sourceBitmap.Width, sourceBitmap.Height),
                       ImageLockMode.ReadOnly,
                       PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[sourceData.Stride *
                                          sourceData.Height];


            byte[] resultBuffer = new byte[sourceData.Stride *
                                           sourceData.Height];


            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0,
                                       pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);


            int filterOffset = (matrixSize - 1) / 2;
            int calcOffset = 0;


            int byteOffset = 0;


            byte blue = 0;
            byte green = 0;
            byte red = 0;

            byte morphResetValue = 0;


            //if (morphType == MorphologyType.Erosion)
            //{
            morphResetValue = 255;
            //}


            for (int offsetY = filterOffset; offsetY <
                sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    sourceBitmap.Width - filterOffset; offsetX++)
                {
                    byteOffset = offsetY *
                                 sourceData.Stride +
                                 offsetX * 4;


                    blue = morphResetValue;
                    green = morphResetValue;
                    red = morphResetValue;


                    //if (morphType == MorphologyType.Dilation)
                    //{
                    //    for (int filterY = -filterOffset;
                    //        filterY <= filterOffset; filterY++)
                    //    {
                    //        for (int filterX = -filterOffset;
                    //            filterX <= filterOffset; filterX++)
                    //        {
                    //            calcOffset = byteOffset +
                    //                         (filterX * 4) +
                    //            (filterY * sourceData.Stride);


                    //            if (pixelBuffer[calcOffset] > blue)
                    //            {
                    //                blue = pixelBuffer[calcOffset];
                    //            }


                    //            if (pixelBuffer[calcOffset + 1] > green)
                    //            {
                    //                green = pixelBuffer[calcOffset + 1];
                    //            }


                    //            if (pixelBuffer[calcOffset + 2] > red)
                    //            {
                    //                red = pixelBuffer[calcOffset + 2];
                    //            }
                    //        }
                    //    }
                    //}
                    //else if (morphType == MorphologyType.Erosion)
                    //{


                    for (int filterY = -filterOffset;
                        filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset;
                            filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset +
                                         (filterX * 4) +
                            (filterY * sourceData.Stride);


                            if (pixelBuffer[calcOffset] < blue)
                            {
                                blue = pixelBuffer[calcOffset];
                            }


                            if (pixelBuffer[calcOffset + 1] < green)
                            {
                                green = pixelBuffer[calcOffset + 1];
                            }


                            if (pixelBuffer[calcOffset + 2] < red)
                            {
                                red = pixelBuffer[calcOffset + 2];
                            }
                        }
                    }
                    //}


                    if (applyBlue == false)
                    {
                        blue = pixelBuffer[byteOffset];
                    }


                    if (applyGreen == false)
                    {
                        green = pixelBuffer[byteOffset + 1];
                    }


                    if (applyRed == false)
                    {
                        red = pixelBuffer[byteOffset + 2];
                    }

                    //if (blue != pixelBuffer[byteOffset] || green != pixelBuffer[byteOffset + 1] || red != pixelBuffer[byteOffset + 2])
                    //{
                    //    blue = 0;
                    //    green = 0;
                    //    red = 255;
                    //}

                    //blue = blue > avg ? (byte)255 : (byte)0;
                    //green = green > avg ? (byte)255 : (byte)0;
                    //red = red > avg ? (byte)255 : (byte)0;
                    resultBuffer[byteOffset] = blue;
                    resultBuffer[byteOffset + 1] = green;
                    resultBuffer[byteOffset + 2] = red;
                    resultBuffer[byteOffset + 3] = 255;
                }
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width,
                                             sourceBitmap.Height);


            BitmapData resultData =
                       resultBitmap.LockBits(new Rectangle(0, 0,
                       resultBitmap.Width, resultBitmap.Height),
                       ImageLockMode.WriteOnly,
                       PixelFormat.Format32bppArgb);


            Marshal.Copy(resultBuffer, 0, resultData.Scan0,
                                       resultBuffer.Length);


            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }

        public static Bitmap MedianFilter(this Bitmap sourceBitmap,
                                            int matrixSize,
                                              int bias = 0,
                                    bool grayscale = false)
        {
            BitmapData sourceData =
                       sourceBitmap.LockBits(new Rectangle(0, 0,
                       sourceBitmap.Width, sourceBitmap.Height),
                       ImageLockMode.ReadOnly,
                       PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[sourceData.Stride *
                                          sourceData.Height];


            byte[] resultBuffer = new byte[sourceData.Stride *
                                           sourceData.Height];


            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0,
                                       pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);


            if (grayscale == true)
            {
                float rgb = 0;


                for (int k = 0; k < pixelBuffer.Length; k += 4)
                {
                    rgb = pixelBuffer[k] * 0.11f;
                    rgb += pixelBuffer[k + 1] * 0.59f;
                    rgb += pixelBuffer[k + 2] * 0.3f;


                    pixelBuffer[k] = (byte)rgb;
                    pixelBuffer[k + 1] = pixelBuffer[k];
                    pixelBuffer[k + 2] = pixelBuffer[k];
                    pixelBuffer[k + 3] = 255;
                }
            }


            int filterOffset = (matrixSize - 1) / 2;
            int calcOffset = 0;


            int byteOffset = 0;

            List<int> neighbourPixels = new List<int>();
            byte[] middlePixel;


            for (int offsetY = filterOffset; offsetY <
                sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    sourceBitmap.Width - filterOffset; offsetX++)
                {
                    byteOffset = offsetY *
                                 sourceData.Stride +
                                 offsetX * 4;


                    neighbourPixels.Clear();


                    for (int filterY = -filterOffset;
                        filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset;
                            filterX <= filterOffset; filterX++)
                        {


                            calcOffset = byteOffset +
                                         (filterX * 4) +
                                (filterY * sourceData.Stride);


                            neighbourPixels.Add(BitConverter.ToInt32(
                                             pixelBuffer, calcOffset));
                        }
                    }


                    neighbourPixels.Sort();

                    middlePixel = BitConverter.GetBytes(
                                       neighbourPixels[filterOffset]);


                    resultBuffer[byteOffset] = middlePixel[0];
                    resultBuffer[byteOffset + 1] = middlePixel[1];
                    resultBuffer[byteOffset + 2] = middlePixel[2];
                    resultBuffer[byteOffset + 3] = middlePixel[3];
                }
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width,
                                             sourceBitmap.Height);


            BitmapData resultData =
                       resultBitmap.LockBits(new Rectangle(0, 0,
                       resultBitmap.Width, resultBitmap.Height),
                       ImageLockMode.WriteOnly,
                       PixelFormat.Format32bppArgb);


            Marshal.Copy(resultBuffer, 0, resultData.Scan0,
                                       resultBuffer.Length);


            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }

        public static Bitmap GradientBasedEdgeDetectionFilter(
                                this Bitmap sourceBitmap,
                                EdgeFilterType filterType,
                                int derivativeLevel,
                                float redFactor = 1.0f,
                                float greenFactor = 1.0f,
                                float blueFactor = 1.0f,
                                byte threshold = 0)
        {
            BitmapData sourceData =
                       sourceBitmap.LockBits(new Rectangle(0, 0,
                       sourceBitmap.Width, sourceBitmap.Height),
                       ImageLockMode.ReadOnly,
                       PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[sourceData.Stride *
                                          sourceData.Height];


            byte[] resultBuffer = new byte[sourceData.Stride *
                                           sourceData.Height];


            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0,
                                       pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);


            int derivative = derivativeLevel;
            int byteOffset = 0;
            int blueGradient, greenGradient, redGradient = 0;
            double blue = 0, green = 0, red = 0;


            bool exceedsThreshold = false;


            for (int offsetY = 1; offsetY < sourceBitmap.Height - 1; offsetY++)
            {
                for (int offsetX = 1; offsetX <
                    sourceBitmap.Width - 1; offsetX++)
                {
                    byteOffset = offsetY * sourceData.Stride +
                                 offsetX * 4;


                    blueGradient =
                    Math.Abs(pixelBuffer[byteOffset - 4] -
                    pixelBuffer[byteOffset + 4]) / derivative;


                    blueGradient +=
                    Math.Abs(pixelBuffer[byteOffset - sourceData.Stride] -
                    pixelBuffer[byteOffset + sourceData.Stride]) / derivative;


                    byteOffset++;


                    greenGradient =
                    Math.Abs(pixelBuffer[byteOffset - 4] -
                    pixelBuffer[byteOffset + 4]) / derivative;


                    greenGradient +=
                    Math.Abs(pixelBuffer[byteOffset - sourceData.Stride] -
                    pixelBuffer[byteOffset + sourceData.Stride]) / derivative;


                    byteOffset++;


                    redGradient =
                    Math.Abs(pixelBuffer[byteOffset - 4] -
                    pixelBuffer[byteOffset + 4]) / derivative;


                    redGradient +=
                    Math.Abs(pixelBuffer[byteOffset - sourceData.Stride] -
                    pixelBuffer[byteOffset + sourceData.Stride]) / derivative;


                    if (blueGradient + greenGradient + redGradient > threshold)
                    { exceedsThreshold = true; }
                    else
                    {
                        byteOffset -= 2;


                        blueGradient = Math.Abs(pixelBuffer[byteOffset - 4] -
                                                pixelBuffer[byteOffset + 4]);
                        byteOffset++;


                        greenGradient = Math.Abs(pixelBuffer[byteOffset - 4] -
                                                 pixelBuffer[byteOffset + 4]);
                        byteOffset++;


                        redGradient = Math.Abs(pixelBuffer[byteOffset - 4] -
                                               pixelBuffer[byteOffset + 4]);


                        if (blueGradient + greenGradient + redGradient > threshold)
                        { exceedsThreshold = true; }
                        else
                        {
                            byteOffset -= 2;


                            blueGradient =
                            Math.Abs(pixelBuffer[byteOffset - sourceData.Stride] -
                            pixelBuffer[byteOffset + sourceData.Stride]);


                            byteOffset++;


                            greenGradient =
                            Math.Abs(pixelBuffer[byteOffset - sourceData.Stride] -
                            pixelBuffer[byteOffset + sourceData.Stride]);


                            byteOffset++;


                            redGradient =
                            Math.Abs(pixelBuffer[byteOffset - sourceData.Stride] -
                            pixelBuffer[byteOffset + sourceData.Stride]);


                            if (blueGradient + greenGradient +
                                      redGradient > threshold)
                            { exceedsThreshold = true; }
                            else
                            {
                                byteOffset -= 2;


                                blueGradient =
                                Math.Abs(pixelBuffer[byteOffset - 4 - sourceData.Stride] -
                                pixelBuffer[byteOffset + 4 + sourceData.Stride]) / derivative;


                                blueGradient +=
                                Math.Abs(pixelBuffer[byteOffset - sourceData.Stride + 4] -
                                pixelBuffer[byteOffset + sourceData.Stride - 4]) / derivative;


                                byteOffset++;


                                greenGradient =
                                Math.Abs(pixelBuffer[byteOffset - 4 - sourceData.Stride] -
                                pixelBuffer[byteOffset + 4 + sourceData.Stride]) / derivative;


                                greenGradient +=
                                Math.Abs(pixelBuffer[byteOffset - sourceData.Stride + 4] -
                                pixelBuffer[byteOffset + sourceData.Stride - 4]) / derivative;


                                byteOffset++;


                                redGradient =
                                Math.Abs(pixelBuffer[byteOffset - 4 - sourceData.Stride] -
                                pixelBuffer[byteOffset + 4 + sourceData.Stride]) / derivative;


                                redGradient +=
                                Math.Abs(pixelBuffer[byteOffset - sourceData.Stride + 4] -
                                pixelBuffer[byteOffset + sourceData.Stride - 4]) / derivative;


                                if (blueGradient + greenGradient + redGradient > threshold)
                                { exceedsThreshold = true; }
                                else
                                { exceedsThreshold = false; }
                            }
                        }
                    }


                    byteOffset -= 2;


                    if (exceedsThreshold)
                    {
                        if (filterType == EdgeFilterType.EdgeDetectMono)
                        {
                            blue = green = red = 255;
                        }
                        else if (filterType == EdgeFilterType.EdgeDetectGradient)
                        {
                            blue = blueGradient * blueFactor;
                            green = greenGradient * greenFactor;
                            red = redGradient * redFactor;
                        }
                        else if (filterType == EdgeFilterType.Sharpen)
                        {
                            blue = pixelBuffer[byteOffset] * blueFactor;
                            green = pixelBuffer[byteOffset + 1] * greenFactor;
                            red = pixelBuffer[byteOffset + 2] * redFactor;
                        }
                        else if (filterType == EdgeFilterType.SharpenGradient)
                        {
                            blue = pixelBuffer[byteOffset] + blueGradient * blueFactor;
                            green = pixelBuffer[byteOffset + 1] + greenGradient * greenFactor;
                            red = pixelBuffer[byteOffset + 2] + redGradient * redFactor;
                        }
                    }
                    else
                    {
                        if (filterType == EdgeFilterType.EdgeDetectMono ||
                            filterType == EdgeFilterType.EdgeDetectGradient)
                        {
                            blue = green = red = 0;
                        }
                        else if (filterType == EdgeFilterType.Sharpen ||
                                 filterType == EdgeFilterType.SharpenGradient)
                        {
                            blue = pixelBuffer[byteOffset];
                            green = pixelBuffer[byteOffset + 1];
                            red = pixelBuffer[byteOffset + 2];
                        }
                    }


                    blue = (blue > 255 ? 255 :
                           (blue < 0 ? 0 :
                            blue));


                    green = (green > 255 ? 255 :
                            (green < 0 ? 0 :
                             green));


                    red = (red > 255 ? 255 :
                          (red < 0 ? 0 :
                           red));


                    resultBuffer[byteOffset] = (byte)blue;
                    resultBuffer[byteOffset + 1] = (byte)green;
                    resultBuffer[byteOffset + 2] = (byte)red;
                    resultBuffer[byteOffset + 3] = 255;
                }
            }

            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width,
                                             sourceBitmap.Height);


            BitmapData resultData =
                       resultBitmap.LockBits(new Rectangle(0, 0,
                       resultBitmap.Width, resultBitmap.Height),
                       ImageLockMode.WriteOnly,
                       PixelFormat.Format32bppArgb);


            Marshal.Copy(resultBuffer, 0, resultData.Scan0,
                                       resultBuffer.Length);


            resultBitmap.UnlockBits(resultData);


            return resultBitmap;
        }

        public enum EdgeFilterType
        {
            EdgeDetectMono, EdgeDetectGradient, Sharpen, SharpenGradient
        }

        public enum BooleanFilterType
        {
            Sharpen, EdgeDetection
        }
    }
}
