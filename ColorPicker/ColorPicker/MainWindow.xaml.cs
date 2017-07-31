using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace ColorPicker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte colorAlpha = 255;
        private byte colorRed = 255;
        private byte colorGreen = 255;
        private byte colorBlue = 255;
        private DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.cboKnownColor.ItemsSource = typeof(Colors).GetProperties();
            this.cboKnownColor.SelectedItem = typeof(Colors).GetProperty("Bisque");

            UpdateSlider();
            ShowColor();

            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += new EventHandler(timer_Tick);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            POINT pt;
            GetCursorPos(out pt);

            Point point = new Point(pt.X, pt.Y);
            Color color = GetPixelColor(point);
            colorAlpha = color.A;
            colorRed = color.R;
            colorGreen = color.G;
            colorBlue = color.B;
            UpdateSlider();
        }

        private void UpdateSlider()
        {
            this.sliderA.Value = colorAlpha;
            this.sliderR.Value = colorRed;
            this.sliderG.Value = colorGreen;
            this.sliderB.Value = colorBlue;
        }

        private void ShowColor()
        {
            Color color = Color.FromArgb(colorAlpha, colorRed, colorGreen, colorBlue);
            this.rectColor.Fill = new SolidColorBrush(color);
            this.txtARGB.Text = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", colorAlpha, colorRed, colorGreen, colorBlue);
        }

        private void cboKnownColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Color color = (Color)(cboKnownColor.SelectedItem as PropertyInfo).GetValue(null, null);
                colorAlpha = color.A;
                colorRed = color.R;
                colorGreen = color.G;
                colorBlue = color.B;
                UpdateSlider();
            }
            catch (Exception)
            {
            }
        }

        private void sliderA_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                colorAlpha = (byte)this.sliderA.Value;
                ShowColor();
            }
        }

        private void sliderR_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                colorRed = (byte)this.sliderR.Value;
                ShowColor();
            }
        }

        private void sliderG_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                colorGreen = (byte)this.sliderG.Value;
                ShowColor();
            }
        }

        private void sliderB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                colorBlue = (byte)this.sliderB.Value;
                ShowColor();
            }
        }

        #region 获取鼠标坐标
        private Color GetPixelColor(Point point)
        {
            int lDC = GetWindowDC(0);
            int intColor = GetPixel(lDC, (int)point.X, (int)point.Y);
            ReleaseDC(0, lDC);
            byte b = (byte)((intColor >> 0x10) & 0xffL);
            byte g = (byte)((intColor >> 8) & 0xffL);
            byte r = (byte)(intColor & 0xffL);
            Color color = Color.FromRgb(r, g, b);
            return color;
        }

        [DllImport("gdi32")]
        private static extern int GetPixel(int hdc, int nXPos, int nYPos);
        [DllImport("user32")]
        private static extern int GetWindowDC(int hwnd);
        [DllImport("user32")]
        private static extern int ReleaseDC(int hWnd, int hDC);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out POINT pt);
        #endregion

        private void gridPick_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string imgPath = "/Images/ImgNorml.png";
            this.imgPick.Source = new BitmapImage(new Uri(imgPath, UriKind.Relative));
            this.Cursor = new Cursor(new MemoryStream(Properties.Resources.GetColor));
            ((UIElement)e.Source).CaptureMouse();
            timer.Start();
        }

        private void gridPick_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string imgPath = "/Images/ImgColor.png";
            this.imgPick.Source = new BitmapImage(new Uri(imgPath, UriKind.Relative));
            this.Cursor = Cursors.Arrow;
            ((UIElement)e.Source).ReleaseMouseCapture();
            timer.Stop();
        }
    }
}
