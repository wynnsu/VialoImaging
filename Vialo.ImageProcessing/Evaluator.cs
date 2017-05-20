using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CNTK;

namespace Vialo.ImageProcessing
{
    /// <summary>
    /// Provide method for image evaluation with pre-trained models
    /// </summary>
    public class Evaluator
    {
        /// <summary>
        /// Evaluate batch of images using pre-trained models
        /// </summary>
        /// <param name="device">Device type</param>
        /// <param name="images">List of 32x32 Bitmap objects</param>
        /// <param name="modelPath">File path to the .dnn pre-trained model file</param>
        /// <returns></returns>
        public static List<Bitmap> EvaluationBatchOfImages(DeviceDescriptor device, List<Bitmap> images, string modelPath)
        {
            var result = new List<Bitmap>();
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
                foreach (var seq in outputData)
                {
                    if (seq.Count == 2)
                    {
                        if (seq[0].CompareTo(0.0f) > 0)
                        {
                            result.Add(images[outputData.IndexOf(seq)]);
                        }
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
    }
}
