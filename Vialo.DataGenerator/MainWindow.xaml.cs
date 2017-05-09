using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Vialo.ImageProcessing;

namespace Vialo.DataGenerator
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

        string path = "";
        VialoImage vImage;
        int w, h;
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = "bmp";
            if (ofd.ShowDialog() == true)
            {
                path = ofd.FileName;
                vImage = new VialoImage();
                vImage.Init(path);
                BitmapImage bmpSource = new BitmapImage(new Uri(path));
                imgTarget.Source = bmpSource;
                w = bmpSource.PixelWidth;
                h = bmpSource.PixelHeight;
                InitGrid(32);
                imgTarget.Visibility = Visibility.Hidden;
            }
        }

        List<Bitmap> fragList = new List<Bitmap>();

        private void InitGrid(int size)
        {
            fragList = vImage.Fragment(size);
            StackPanel innerStack;
            ScaleTransform scale = new ScaleTransform(2.0, 2.0);
            for (int i = 0; i < h / size; i++)
            {
                innerStack = new StackPanel { Orientation = Orientation.Horizontal };
                for (int j = 0; j < w / size; j++)
                {
                    Bitmap b = fragList[(size - 1) * i + j];
                    CheckBox cb = new CheckBox();
                    cb.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                    cb.RenderTransform = scale;
                    cb.Margin = new Thickness(6);
                    ImageBrush brush = new ImageBrush();
                    MemoryStream ms = new MemoryStream();
                    b.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    ms.Seek(0, SeekOrigin.Begin);
                    image.StreamSource = ms;
                    image.EndInit();
                    brush.ImageSource = image;
                    cb.Background = brush;
                    innerStack.Children.Add(cb);
                }
                container.Children.Add(innerStack);
            }
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(path))
            {
                string noisePath = System.IO.Path.GetDirectoryName(path) + @"/0/";
                string backgroundPath = System.IO.Path.GetDirectoryName(path) + @"/1/";
                if (!Directory.Exists(noisePath))
                {
                    Directory.CreateDirectory(noisePath);
                }
                if (!Directory.Exists(backgroundPath))
                {
                    Directory.CreateDirectory(backgroundPath);
                }
                int countNoise = Directory.GetFiles(noisePath).Length;
                int countBackground = Directory.GetFiles(backgroundPath).Length;
                int idx = 0;
                foreach (StackPanel panel in container.Children)
                {
                    foreach (CheckBox cb in panel.Children)
                    {
                        if (cb.IsChecked == true)
                        {
                            fragList[idx].Save(noisePath + countNoise + ".bmp");
                            countNoise++;
                        }
                        else
                        {
                            fragList[idx].Save(backgroundPath + countBackground + ".bmp");
                            countBackground++;
                        }
                        idx++;
                    }
                }

                Application.Current.Shutdown();
            }
        }
    }
}
