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
        private DispatcherTimer timer = new DispatcherTimer();

        #region Propertys
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(MainWindow),
                new PropertyMetadata(Colors.Bisque, new PropertyChangedCallback(OnColorChanged)));

        public static readonly DependencyProperty AlphaProperty =
            DependencyProperty.Register("Alpha", typeof(byte), typeof(MainWindow),
                new PropertyMetadata(Colors.Bisque.A, new PropertyChangedCallback(OnARGBChanged)));
        public static readonly DependencyProperty RedProperty =
            DependencyProperty.Register("Red", typeof(byte), typeof(MainWindow),
                new PropertyMetadata(Colors.Bisque.R, new PropertyChangedCallback(OnARGBChanged)));
        public static readonly DependencyProperty GreenProperty =
            DependencyProperty.Register("Green", typeof(byte), typeof(MainWindow),
                new PropertyMetadata(Colors.Bisque.G, new PropertyChangedCallback(OnARGBChanged)));
        public static readonly DependencyProperty BlueProperty =
            DependencyProperty.Register("Blue", typeof(byte), typeof(MainWindow),
                new PropertyMetadata(Colors.Bisque.B, new PropertyChangedCallback(OnARGBChanged)));

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public byte Alpha
        {
            get { return (byte)GetValue(AlphaProperty); }
            set { SetValue(AlphaProperty, value); }
        }

        public byte Red
        {
            get { return (byte)GetValue(RedProperty); }
            set { SetValue(RedProperty, value); }
        }

        public byte Green
        {
            get { return (byte)GetValue(GreenProperty); }
            set { SetValue(GreenProperty, value); }
        }

        public byte Blue
        {
            get { return (byte)GetValue(BlueProperty); }
            set { SetValue(BlueProperty, value); }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.cboKnownColor.ItemsSource = typeof(Colors).GetProperties();
            this.cboKnownColor.SelectedItem = typeof(Colors).GetProperty("Bisque");

            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += new EventHandler(timer_Tick);
        }

        private static void OnColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MainWindow win = (MainWindow)sender;

            Color newColor = (Color)e.NewValue;
            win.Alpha = newColor.A;
            win.Red = newColor.R;
            win.Green = newColor.G;
            win.Blue = newColor.B;
        }

        private static void OnARGBChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MainWindow win = (MainWindow)sender;

            Color color = win.Color;
            if (e.Property == AlphaProperty)
                color.A = (byte)e.NewValue;
            else if (e.Property == RedProperty)
                color.R = (byte)e.NewValue;
            else if (e.Property == GreenProperty)
                color.G = (byte)e.NewValue;
            else if (e.Property == BlueProperty)
                color.B = (byte)e.NewValue;

            win.Color = color;
        }

        private void cboKnownColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Color = (Color)(cboKnownColor.SelectedItem as PropertyInfo).GetValue(null, null);
            }
            catch (Exception)
            { }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            POINT pt;
            GetCursorPos(out pt);

            Point point = new Point(pt.X, pt.Y);
            Color = GetPixelColor(point);
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
