using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using Windows.ApplicationModel.Store;
using Windows.Media;
using Windows.Media.Control;
using Windows.Media.Playback;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;


namespace MusicWidget
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GlobalSystemMediaTransportControlsSessionManager gsmtcsm;
        private GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties;
        private RegistryKey currApp;
        private bool IsBlockMove;
        public MainWindow()
        {
            InitializeComponent();
            DataContext=this;
            var primaryMonitorArea = SystemParameters.WorkArea;
            Left = primaryMonitorArea.Right - Width-5;
            Top = primaryMonitorArea.Bottom - Height-5;
            var image_next = new System.Windows.Controls.Image();
            image_next.Source=ToImage(Properties.Resources.music_next);
            Next.Content=image_next;
            var image_back = new System.Windows.Controls.Image();
            image_back.Source=ToImage(Properties.Resources.music_back);
            Back.Content=image_back;
            System.Drawing.Size resolution = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size;
            Console.WriteLine(resolution);
            currApp=GetAppKey();
            UpdateLeftTop();
            IsBlockMove=true;
            this.Loaded+=MainWindow_Loaded;
            _ = Maisn();
            
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HideFromAltTab(new System.Windows.Interop.WindowInteropHelper(this).Handle);
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
            if(currenApp==null)
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
        public async Task Maisn()
        {
            gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
            try
            {
                mediaProperties = await GetMediaProperties(gsmtcsm.GetCurrentSession());
            }
            catch
            {
                await AwaitMedia();
                gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
                mediaProperties = await GetMediaProperties(gsmtcsm.GetCurrentSession());

            }

            try
            {


                var timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 300) };
                timer.Tick += async (o, O) => await UpdatePlayback();
                timer.Start();
            }
            catch
            {
                MessageBox.Show("");
            }

        }
        private void UpdatePlayPause(string type)
        {
            if (type=="Paused")
            {
                var image = new System.Windows.Controls.Image();
                image.Source=ToImage(Properties.Resources.play_button);
          
                StartStop.Content=image;



            }
            else
            {
                var image = new System.Windows.Controls.Image();
                image.Source=ToImage(Properties.Resources.video_pause_button);
 
                StartStop.Content=image;


            }
        }
        private async Task UpdatePlayback()
        {

            await Task.Run(async () =>
            {
                gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
                try
                {
                    mediaProperties = await GetMediaProperties(gsmtcsm.GetCurrentSession());
                }
                catch
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Image.Source=null;
                        Track.Text=String.Empty;
                        Singer.Text=String.Empty;
                        UpdatePlayPause("Paused");
                    });
                    return;

                }
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    Track.Text=mediaProperties.Title;
                    Singer.Text=mediaProperties.Artist;
                });
                var CurrSession = gsmtcsm.GetCurrentSession();

                var play_back = CurrSession.GetPlaybackInfo();
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    UpdatePlayPause(play_back.PlaybackStatus.ToString());
                });
                try
                {
                   

                    var ssss = await mediaProperties.Thumbnail.OpenReadAsync();
                   

                   
                    using (StreamReader sr = new StreamReader(ssss.AsStreamForRead()))
                    {
                        var bytes = default(byte[]);
                        using (var memstream = new MemoryStream())
                        {
                            var buffer = new byte[512];
                            var bytesRead = default(int);
                            while ((bytesRead = sr.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                                memstream.Write(buffer, 0, bytesRead);
                            bytes = memstream.ToArray();
                            App.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Image.Source = ToImage(bytes);
                            });
                        }
                    }
                }
                catch
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Image.Source=null;
                    });
                }
            });

        }
        private BitmapImage ToImage(byte[] array)//Делаем из потока байтов картинку
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                try
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad; // here
                    image.StreamSource = ms;
                    image.Rotation = Rotation.Rotate0;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.EndInit();
                    return image;
                }
                catch
                {
                    return null;
                }
            }
        }
        private async Task<GlobalSystemMediaTransportControlsSessionManager> GetSystemMediaTransportControlsSessionManager() =>
                await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

        private async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaProperties(GlobalSystemMediaTransportControlsSession session)
     => await session.TryGetMediaPropertiesAsync();
        private async Task AwaitMedia()
        {
            await Task.Run(async () => {
                while (true)
                {
                    try
                    {
                        var session = await GetSystemMediaTransportControlsSessionManager();
                        var ss = await session.GetCurrentSession().TryGetMediaPropertiesAsync();
                        break;
                    }
                    catch
                    {

                        continue;
                    }
                }
            });
        }
        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var CurrSession = gsmtcsm.GetCurrentSession();

                await CurrSession.TrySkipPreviousAsync();
                _=UpdatePlayback();
            }
            catch { }

        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var CurrSession = gsmtcsm.GetCurrentSession();

                await CurrSession.TrySkipNextAsync();
                _=UpdatePlayback();
            }
            catch { }
        }

        private async void StartStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var CurrSession = gsmtcsm.GetCurrentSession();
                var play_back = CurrSession.GetPlaybackInfo();
                if (play_back.PlaybackStatus.ToString()=="Paused")
                {
                    await CurrSession.TryPlayAsync();
                }
                else
                {
                    await CurrSession.TryPauseAsync();
                }
            }
            catch { }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        #region Bottommost

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOACTIVATE = 0x0010;

        private void ToBack()
        {
            var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            SetWindowPos(handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            ToBack();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            ToBack();
        }

        #endregion

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

        private void MoveClick_Click(object sender, RoutedEventArgs e)
        {
            IsBlockMove=!IsBlockMove;
            if (IsBlockMove)
                this.ResizeMode=ResizeMode.NoResize;
            else
                this.ResizeMode=ResizeMode.CanResizeWithGrip;
            
        }
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr window, int index, int value);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr window, int index);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        public static void HideFromAltTab(IntPtr Handle)
        {
            SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle,
                GWL_EXSTYLE) | WS_EX_TOOLWINDOW);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateReg();
        }
    }
}

