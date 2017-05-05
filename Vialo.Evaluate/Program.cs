using CNTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vialo.Evaluate
{
    class Program
    {
        static void Main(string[] args)
        {
            Evaluator.EvaluationBatchOfImages(DeviceDescriptor.CPUDevice);
        }
    }
}
