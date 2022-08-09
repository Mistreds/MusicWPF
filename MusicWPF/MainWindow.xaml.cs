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
using Windows.Media;
using Windows.Media.Control;
using Windows.Media.Playback;
using static System.Net.Mime.MediaTypeNames;

namespace MusicWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private    GlobalSystemMediaTransportControlsSessionManager gsmtcsm;
        private GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties;
        private GlobalSystemMediaTransportControlsSession CurrMusSession;
        public MainWindow()
        {
            InitializeComponent();
            //ss

            var primaryMonitorArea = SystemParameters.WorkArea;
            Left = primaryMonitorArea.Right - Width-5;
            Top = primaryMonitorArea.Bottom - Height-5 ;
            var image_next = new System.Windows.Controls.Image();
            image_next.Source=ToImage(Properties.Resources.music_next);
            Next.Content=image_next;
            var image_back = new System.Windows.Controls.Image();
            image_back.Source=ToImage(Properties.Resources.music_back);
            Back.Content=image_back;
            Tray.Icon=Properties.Resources.play_button1;
            _ = Maisn();
            Hide();
        }
        public  async Task Maisn()
        {
           
            var timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0,0,500) };
            timer.Tick += async (o,O) => UpdatePlayback();
            timer.Start();
           
            GC.Collect();

        }
            private void UpdatePlayPause(string type)
            {
                if (type=="Paused")
                {
                    var image = new System.Windows.Controls.Image();
                    image.Source=ToImage(Properties.Resources.play_button);
                    Tray.Icon=Properties.Resources.play_button1;
                    StartStop.Content=image;
              


                }
                else
                {
                    var image = new System.Windows.Controls.Image();
                    image.Source=ToImage(Properties.Resources.video_pause_button);
                    Tray.Icon=Properties.Resources.video_pause_button1;
                    StartStop.Content=image;
              

                }
            }
        private async void UpdatePlayback()
        {
            using (null)
            {

           
                try
            {
               
                gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
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
                    GC.Collect();
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
            gsmtcsm=null;
            mediaProperties=null;
                GC.Collect();
            }

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
    
    private  async Task<GlobalSystemMediaTransportControlsSessionManager> GetSystemMediaTransportControlsSessionManager() =>
            await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

        private  async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaProperties(GlobalSystemMediaTransportControlsSession session)
     => await session.TryGetMediaPropertiesAsync();
             
            
        

        private async Task AwaitMedia()
        {
            await Task.Run(async() => {
                while (true)
                {
                    try
                    {
                        var session= await GetSystemMediaTransportControlsSessionManager();
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
                gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
                var CurrSession = gsmtcsm.GetCurrentSession();

                await CurrSession.TrySkipPreviousAsync();
                gsmtcsm=null;
                UpdatePlayback();
            }
            catch { }

        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
                var CurrSession = gsmtcsm.GetCurrentSession();

                await CurrSession.TrySkipNextAsync();
                gsmtcsm=null;
                UpdatePlayback();
            }
            catch { }
        }

        private async void StartStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
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
                gsmtcsm=null;
            }
            catch { }
        }

        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (this.IsVisible)
            {
             
                this.Hide();
            }
                
            else
            {

                this.Show();
                HideFromAltTab(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            }
                
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private async  void Tray_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
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
    }
}
