using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Vialo.ImageProcessing;

namespace Vialo.TestEval
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string path = @"C:\Users\su153\Source\Repos\VialoImaging\Vialo.TestEval\Picture1.png";
            VialoImage vImage;
            //Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            //if (ofd.ShowDialog() != true) { return; }
            //path = ofd.FileName;

            vImage = new VialoImage();
            vImage.Init(path);

            //Microsoft.Win32.OpenFileDialog ofdModel = new Microsoft.Win32.OpenFileDialog();
            //ofdModel = new Microsoft.Win32.OpenFileDialog();
            //ofdModel.DefaultExt = "dnn";
            //if (ofdModel.ShowDialog() != true) { return; }
            //string modelPath = ofdModel.FileName;

            var images = vImage.Fragment(32);

            string modelPath = @"C:\Users\su153\Source\Repos\VialoImaging\Vialo.TestEval\ConvNet_CIFAR10_DataAug_14.dnn";

            var result = vImage.Evaluate(modelPath);
            System.Windows.Controls.Image image;
            foreach (var bmp in result)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    image = new System.Windows.Controls.Image();
                    bmp.Save(memory, ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    image.Source = bitmapImage;
                    container.Children.Add(image);
                }
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
