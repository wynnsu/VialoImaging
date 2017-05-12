using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
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
            container.Children.Add(new Image()
            {
                Source = new BitmapImage(new Uri(@"C:\Users\Wynn\Documents\visual studio 2017\Projects\VialoImaging\Vialo.TestEval\T1_220 microns_flash.bmp"))
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string path = @"C:\Users\Wynn\Documents\visual studio 2017\Projects\VialoImaging\Vialo.TestEval\results\";
            VialoImage vImage = new VialoImage();
            vImage.Init(@"C:\Users\Wynn\Documents\visual studio 2017\Projects\VialoImaging\Vialo.TestEval\T1_220 microns_flash.bmp");
            //var result = vImage.Evaluate(Path.Combine(basePath, modelPath));
            var result = vImage.Evaluate(@"C:\Users\Wynn\Documents\visual studio 2017\Projects\VialoImaging\Vialo.TestEval\ConvNet_CIFAR10_DataAug_14.dnn");
            //var count = Directory.GetFiles(Path.Combine(basePath, resultPath)).Length;
            var count = 0;
            container.Visibility = Visibility.Hidden;
            foreach (var img in result)
            {
                string imgPath = Path.Combine(path, (count++) + ".bmp");
                img.Save(imgPath);
                Image newImage = new Image();
                newImage.Source = new BitmapImage(new Uri(imgPath));
                newImage.Width = 32;
                newImage.Height = 32;
                newImage.Margin = new Thickness(3);
                result_list.Children.Add(newImage);
            }
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBox.Show("Eval Done!", "", button);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
