using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Vialo.ImageProcessing;

namespace Vialo.TestEval
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string basePath;
        string resultPath = @"results";
        string imagePath = @"ss192d.bmp";
        string modelPath = @"ConvNet_CIFAR10_DataAug_14.dnn";

        public MainWindow()
        {
            InitializeComponent();

            Func<string, string> toAbsolute = relative => Path.Combine(basePath, relative);
            basePath = Directory.GetCurrentDirectory();
            resultPath = toAbsolute(resultPath);
            imagePath = toAbsolute(imagePath);
            modelPath = toAbsolute(modelPath);
            container.Children.Add(new System.Windows.Controls.Image()
            {
                Source = new BitmapImage(new Uri(imagePath))
            });
        }

        List<Bitmap> imageBuffer = new List<Bitmap>();

        private void Eval_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(resultPath))
                Directory.CreateDirectory(resultPath);

            var result = new VialoImage().Init(imagePath).Evaluate(modelPath);
            Func<Bitmap, BitmapImage> toImageSource = bmp =>
              {
                  using (MemoryStream memory = new MemoryStream())
                  {
                      bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                      memory.Position = 0;
                      BitmapImage bitmapimage = new BitmapImage();
                      bitmapimage.BeginInit();
                      bitmapimage.StreamSource = memory;
                      bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                      bitmapimage.EndInit();

                      return bitmapimage;
                  }
              };

            result.ForEach(
                i => result_list.Children.Add(new System.Windows.Controls.Image()
                {
                    Source = toImageSource(i),
                    Width = 32,
                    Height = 32,
                    Margin = new Thickness(3)
                })
            );
            imageBuffer.AddRange(result);
            MessageBox.Show("Eval Done!", "", MessageBoxButton.OK);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
