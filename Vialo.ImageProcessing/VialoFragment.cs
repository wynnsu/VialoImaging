using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vialo.ImageProcessing
{
    public class VialoFragment
    {
        public Bitmap Bmp;
        public bool IsChecked;
        public VialoFragment(Bitmap bmp, bool isChecked)
        {
            Bmp = bmp;
            IsChecked = isChecked;
        }
    }
}
