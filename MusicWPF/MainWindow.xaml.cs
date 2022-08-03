using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
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
        public MainWindow()
        {
            InitializeComponent();
            var image_next = new System.Windows.Controls.Image();
            image_next.Source=ToImage(Properties.Resources.music_next);
            Next.Content=image_next;
            var image_back = new System.Windows.Controls.Image();
            image_back.Source=ToImage(Properties.Resources.music_back);
            Back.Content=image_back;
            _ = Maisn();
            Hide();
        }
        public  async Task Maisn()
        {
            gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
            mediaProperties = await GetMediaProperties(gsmtcsm.GetCurrentSession());
          

            var CurrMusSession = gsmtcsm.GetCurrentSession();
            CurrMusSession.PlaybackInfoChanged += Ss_PlaybackInfoChanged;
           await UpdatePlayback();
        }
        private void UpdatePlayPause(string type)
        {
            if (type=="Paused")
            {
                var image = new System.Windows.Controls.Image();
                image.Source=ToImage(Properties.Resources.play_button);

                StartStop.Content=image;
                //Tray.IconSource=(ImageSource)ToImage(Properties.Resources.play_button);


            }
            else
            {
                var image = new System.Windows.Controls.Image();
                image.Source=ToImage(Properties.Resources.video_pause_button);
                 StartStop.Content=image;
               // Tray.IconSource=(ImageSource)ToImage(Properties.Resources.video_pause_button);

            }
        }

        private  async Task UpdatePlayback()
        {
            await Task.Run(async() =>
            {
                gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
                mediaProperties = await GetMediaProperties(gsmtcsm.GetCurrentSession());
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    Track.Text=mediaProperties.Title;
                    Singer.Text=mediaProperties.Artist;
                });
                var ssss = await mediaProperties.Thumbnail.OpenReadAsync();
                var CurrSession = gsmtcsm.GetCurrentSession();
               
                var play_back = CurrSession.GetPlaybackInfo();
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    UpdatePlayPause(play_back.PlaybackStatus.ToString());
                });
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
            });
           
        }
        private async  void Ss_PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
  
            await UpdatePlayback();
           
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

        private  async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaProperties(GlobalSystemMediaTransportControlsSession session) =>
            await session.TryGetMediaPropertiesAsync();

        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            var CurrSession = gsmtcsm.GetCurrentSession();
           
            await CurrSession.TrySkipPreviousAsync();

        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            var CurrSession = gsmtcsm.GetCurrentSession();
           
            await CurrSession.TrySkipNextAsync();
        }

        private async void StartStop_Click(object sender, RoutedEventArgs e)
        {
            var CurrSession = gsmtcsm.GetCurrentSession();
           
            var play_back = CurrSession.GetPlaybackInfo();
            if(play_back.PlaybackStatus.ToString()=="Paused")
            {
                await CurrSession.TryPlayAsync();
            }
            else
            {
                await CurrSession.TryPauseAsync();
            }
        }

        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (this.IsVisible)
            {
             
                this.Hide();
            }
                
            else
            {
                var primaryMonitorArea = SystemParameters.WorkArea;
                Left = primaryMonitorArea.Right - Width - 10;
                Top = primaryMonitorArea.Bottom - Height - 10;
                this.Show();
            }
                
        }
    }
}
