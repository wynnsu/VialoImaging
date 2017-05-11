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
            string path = @"C:\Users\Wynn\Documents\visual studio 2017\Projects\VialoImaging\Vialo.TestEval\results\";
            VialoImage vImage = new VialoImage();
            vImage.Init(@"C:\Users\Wynn\Documents\visual studio 2017\Projects\VialoImaging\Vialo.TestEval\T1_220 microns_flash.bmp");
            Console.WriteLine("Good Init");
            var result = vImage.Evaluate(@"C:\Users\Wynn\Documents\visual studio 2017\Projects\VialoImaging\Vialo.TestEval\ConvNet_CIFAR10_DataAug_14.dnn");
            var count = Directory.GetFiles(path).Length;
            foreach (var img in result)
            {
                img.Save(path + (count++) + ".bmp");
            }
            Application.Current.Shutdown();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
