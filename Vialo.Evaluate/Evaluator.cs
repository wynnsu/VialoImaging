using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CNTK;

namespace Vialo.Evaluate
{
    public class Evaluator
    {

        /// <summary>
        /// Print out the evalaution results.
        /// </summary>
        /// <typeparam name="T">The data value type</typeparam>
        /// <param name="sampleSize">The size of each sample.</param>
        /// <param name="outputBuffer">The evaluation result data.</param>
        internal static void PrintOutput<T>(int sampleSize, IList<IList<T>> outputBuffer)
        {
            Console.WriteLine("The number of sequences in the batch: " + outputBuffer.Count);
            int seqNo = 0;
            int outputSampleSize = sampleSize;
            foreach (var seq in outputBuffer)
            {
                if (seq.Count % outputSampleSize != 0)
                {
                    throw new ApplicationException("The number of elements in the sequence is not a multiple of sample size");
                }

                Console.WriteLine(String.Format("Sequence {0} contains {1} samples.", seqNo++, seq.Count / outputSampleSize));
                int i = 0;
                int sampleNo = 0;
                foreach (var element in seq)
                {
                    if (i++ % outputSampleSize == 0)
                    {
                        Console.Write(String.Format("    sample {0}: ", sampleNo));
                    }
                    Console.Write(element);
                    if (i % outputSampleSize == 0)
                    {
                        Console.WriteLine(".");
                        sampleNo++;
                    }
                    else
                    {
                        Console.Write(",");
                    }
                }
            }
        }
        /// <summary>
        /// Checks whether the file exists. If not, write the error message on the console and throw FileNotFoundException.
        /// </summary>
        /// <param name="filePath">The file to check.</param>
        /// <param name="errorMsg">The message to write on console if the file does not exist.</param>
        internal static void ThrowIfFileNotExist(string filePath, string errorMsg)
        {
            if (!File.Exists(filePath))
            {
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Console.WriteLine(errorMsg);
                }
                throw new FileNotFoundException(string.Format("File '{0}' not found.", filePath));
            }
        }

        private static Dictionary<string, int> buildVocabIndex(string filePath)
        {
            var vocab = new Dictionary<string, int>();

            string[] lines = File.ReadAllLines(filePath);
            for (int idx = 0; idx < lines.Count(); idx++)
                vocab.Add(lines[idx], idx);

            return vocab;
        }

        private static string[] buildSlotIndex(string filePath)
        {
            return File.ReadAllLines(filePath);
        }

        public static void EvaluationSingleImage(DeviceDescriptor device)
        {
            try
            {
                Console.WriteLine("\n===== Evaluate single image =====");

                // Load the model.
                // The model resnet20.dnn is trained by <CNTK>/Examples/Image/Classification/ResNet/Python/Models/TrainResNet_CIFAR10.py
                // Please see README.md in <CNTK>/Examples/Image/Classification/ResNet about how to train the model.
                string modelFilePath = "ConvNet_CIFAR10_DataAug_14.dnn";
                ThrowIfFileNotExist(modelFilePath, string.Format("Error: The model '{0}' does not exist. Please follow instructions in README.md in <CNTK>/Examples/Image/Classification/ResNet to create the model.", modelFilePath));
                Function modelFunc = Function.LoadModel(modelFilePath, device);

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

                // Image preprocessing to match input requirements of the model.
                // This program uses images from the CIFAR-10 dataset for evaluation.
                // Please see README.md in <CNTK>/Examples/Image/DataSets/CIFAR-10 about how to download the CIFAR-10 dataset.
                string sampleImage = "125.bmp";
                ThrowIfFileNotExist(sampleImage, string.Format("Error: The sample image '{0}' does not exist. Please see README.md in <CNTK>/Examples/Image/DataSets/CIFAR-10 about how to download the CIFAR-10 dataset.", sampleImage));
                Bitmap bmp = new Bitmap(Bitmap.FromFile(sampleImage));
                var resized = bmp.Resize((int)imageWidth, (int)imageHeight, true);
                List<float> resizedCHW = resized.ParallelExtractCHW();

                // Create input data map
                var inputVal = Value.CreateBatch(inputVar.Shape, resizedCHW, device);
                inputDataMap.Add(inputVar, inputVal);

                // Create ouput data map. Using null as Value to indicate using system allocated memory.
                // Alternatively, create a Value object and add it to the data map.
                outputDataMap.Add(outputVar, null);

                // Start evaluation on the device
                modelFunc.Evaluate(inputDataMap, outputDataMap, device);

                // Get evaluate result as dense output
                var outputVal = outputDataMap[outputVar];
                var outputData = outputVal.GetDenseData<float>(outputVar);

                PrintOutput(outputVar.Shape.TotalSize, outputData);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}\nCallStack: {1}\n Inner Exception: {2}", ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : "No Inner Exception");
                throw ex;
            }
        }

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
                PrintOutput(outputVar.Shape.TotalSize, outputData);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}\nCallStack: {1}\n Inner Exception: {2}", ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : "No Inner Exception");
                throw ex;
            }
            return result;
        }
    }
}
