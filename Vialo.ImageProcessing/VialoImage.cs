using System.Collections.Generic;
using System.Drawing;
using CNTK;

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
            var w = bmp.Width;
            var h = bmp.Height;
            var hMargin = w % size / 2;
            var vMargin = h % size / 2;

            for (var i = vMargin; i + size <= h - vMargin; i += size)
            {
                for (var j = hMargin; j + size <= w - hMargin; j += size)
                {
                    list.Add(bmp.Clone(new Rectangle(j, i, size, size), bmp.PixelFormat));
                }
            }
            return list;
        }
    }

}
