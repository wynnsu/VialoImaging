using CNTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vialo.Evaluate
{
    class Program
    {
        static void Main(string[] args)
        {
            var pathList = new List<string>() { "125.bmp", "13.bmp", "6672.bmp" };
            var images = new List<Bitmap>();
            foreach (var path in pathList)
            {
                images.Add(new Bitmap(path));
            }
            string modelFilePath = "ConvNet_CIFAR10_DataAug_14.dnn";
            Evaluator.EvaluationBatchOfImages(DeviceDescriptor.CPUDevice, images, modelFilePath);
        }
    }
}
