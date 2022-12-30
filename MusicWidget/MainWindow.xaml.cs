using Microsoft.Win32;
using MusicLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Media.Control;

namespace MusicWidget
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UpdateContent updateContent;
        private UpdateImage updateImage;
        private UpdatePlay updatePlay;
        private MusicControls musicControls;
        private Animation Animation;
        private RegistryKey currApp;
        private bool IsBlockMove;
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hP, IntPtr hC, string sC,
    string sW);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumedWindow lpEnumFunc, ArrayList
        lParam);

        public delegate bool EnumedWindow(IntPtr handleWindow, ArrayList handles);

        public static bool GetWindowHandle(IntPtr windowHandle, ArrayList
        windowHandles)
        {
            windowHandles.Add(windowHandle);
            return true;
        }

        private void SetAsDesktopChild()
        {
            ArrayList windowHandles = new ArrayList();
            EnumedWindow callBackPtr = GetWindowHandle;
            EnumWindows(callBackPtr, windowHandles);

            foreach (IntPtr windowHandle in windowHandles)
            {
                IntPtr hNextWin = FindWindowEx(windowHandle, IntPtr.Zero,
                "SHELLDLL_DefView", null);
                if (hNextWin != IntPtr.Zero)
                {
                    var interop = new WindowInteropHelper(this);
                    interop.EnsureHandle();
                    interop.Owner = hNextWin;
                }
            }
        }

        public MainWindow()
        {
            var primaryMonitorArea = SystemParameters.WorkArea;
            InitializeComponent();

            SetAsDesktopChild();
            Loaded+=MainWindow_Loaded;
            IsBlockMove=true;
            updateContent=UpdateContentWin;
            updateImage=UpdateImageWin;
            updatePlay= UpdatePlayPause;
         
            currApp=GetAppKey();
            UpdateLeftTop();

        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
            WinApiLib.ShoveToBackground((int)new System.Windows.Interop.WindowInteropHelper(this).Handle);
            Animation =new Animation(this);
            musicControls=new MusicControls(updateContent, updateImage, updatePlay);
            WinApiLib.HideFromAltTab(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            

        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
           
        }
      
      
        private void UpdateLeftTop()
        {
            this.Left =Convert.ToDouble(currApp.GetValue("left"));
            this.Top =Convert.ToDouble(currApp.GetValue("top"));
            this.Width=Convert.ToDouble(currApp.GetValue("width"));
            this.Height=Convert.ToDouble(currApp.GetValue("height"));
        }
        private void UpdateReg()
        {
            currApp.SetValue("left", Left.ToString());
            currApp.SetValue("top", Top.ToString());
            currApp.SetValue("width", ActualWidth);
            currApp.SetValue("height", ActualHeight);
        }
        private RegistryKey GetAppKey()
        {
            RegistryKey currentUserKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
            Console.WriteLine(currentUserKey);
            RegistryKey currenApp = currentUserKey.OpenSubKey("MusicWidget", true);
            if (currenApp==null)
            {
                currentUserKey.CreateSubKey("MusicWidget");
                currenApp = currentUserKey.OpenSubKey("MusicWidget", true);
            }
            if (currenApp.GetValue("left")==null)
            { currenApp.SetValue("left", "200"); }
            if (currenApp.GetValue("top")==null)
            { currenApp.SetValue("top", "200"); }
            if (currenApp.GetValue("height")==null)
            { currenApp.SetValue("height", "400"); }
            if (currenApp.GetValue("width")==null)
            { currenApp.SetValue("width", "400"); }
            return currenApp;
        }
        private void UpdatePlayPause(string type)
        {
            if (type=="Paused")
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StartStop.Content=this.FindResource("Start");
                });
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StartStop.Content=this.FindResource("Stop");
                });
            }
        }
        private void UpdateContentWin(string track, string singer, bool error)
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                if (error)
                {
                    Image.Source=null;
                    Track.Text=String.Empty;
                    Singer.Text=String.Empty;
                }
                else
                {
                    Track.Text=track;
                    Track1.Text=track;
                    Singer.Text=singer;

                }
            });

        }
        BitmapImage image_ = new BitmapImage();
        private async void UpdateImageWin(byte[] _image)
        {
            if (_image==null)
                return;
           
         await App.Current.Dispatcher.Invoke(async delegate
            {
                var image = ConvertImage.ToBitmapImage(_image);
                if (image==image_)
                    return;
                image_=image;
                Image.Source = image;
                var ss=await ConvertImage.GetColor(image);
                Border.Background=await ConvertImage.GetColor(image);
            });
        }
        private void StartTrackAnimation()
        {
                //Dispatcher.Stop();
           
                //Dispatcher = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0,1) };
                //Dispatcher.Tick += (o, O) => Timers();
                //Dispatcher.Start();
              
            
            if (Animation==null)
                return;
            Track.BeginAnimation(Canvas.LeftProperty, null);
            Track1.BeginAnimation(Canvas.LeftProperty, null);
            if (Track.ActualWidth > this.ActualWidth-60)
            {
                Animation.UpdateTrackAnimation();
                Track.BeginAnimation(Canvas.LeftProperty, Animation.StartFirstTrack);
                Track1.BeginAnimation(Canvas.LeftProperty, Animation.StartSecondTrack);

            }
        }
        private void Back_Click(object sender, RoutedEventArgs e) => musicControls.BackButton();
        private void Next_Click(object sender, RoutedEventArgs e) => musicControls.NextButton();
        private void StartStop_Click(object sender, RoutedEventArgs e) => musicControls.StartStop();
      
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        private void Track_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetLeft(Track, 0);
            
            StartTrackAnimation();
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("MusicWPF.exe");
            Process.GetCurrentProcess().Kill();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RectangleGeometry va = new RectangleGeometry();
            va.Rect=new Rect(0, 0, 230, 30);
            StartTrackAnimation();
            UpdateReg();

        }
        #region Move

        private bool winDragged = false;
        private Point lmAbs = new Point();

        void Window_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            winDragged = true;
            this.lmAbs = e.GetPosition(this);
            this.lmAbs.Y = Convert.ToInt16(this.Top) + this.lmAbs.Y;
            this.lmAbs.X = Convert.ToInt16(this.Left) + this.lmAbs.X;
            Mouse.Capture(this);
        }

        void Window_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {   
            winDragged = false;
            Mouse.Capture(null);
            WinApiLib.ShoveToBackground((int)new System.Windows.Interop.WindowInteropHelper(this).Handle);
        }

        void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsBlockMove)
                return;
            if (winDragged)
            {
                Point MousePosition = e.GetPosition(this);
                Point MousePositionAbs = new Point();
                MousePositionAbs.X = Convert.ToInt16(this.Left) + MousePosition.X;
                MousePositionAbs.Y = Convert.ToInt16(this.Top) + MousePosition.Y;
                this.Left = this.Left + (MousePositionAbs.X - this.lmAbs.X);
                this.Top = this.Top + (MousePositionAbs.Y - this.lmAbs.Y);
                this.lmAbs = MousePositionAbs;
                UpdateReg();

            }
        }
        
        #endregion
        protected override void OnDeactivated(EventArgs e)
        {

            this.Show();
            WindowState = System.Windows.WindowState.Normal;
            WinApiLib.ShoveToBackground((int)new System.Windows.Interop.WindowInteropHelper(this).Handle);
        }
        private void MoveClick_Click(object sender, RoutedEventArgs e)
        {
            IsBlockMove=!IsBlockMove;
            if (IsBlockMove)
                this.ResizeMode=ResizeMode.NoResize;
            else
                this.ResizeMode=ResizeMode.CanResizeWithGrip;
            WinApiLib.ShoveToBackground((int)new System.Windows.Interop.WindowInteropHelper(this).Handle);
        }

     
    }
}
