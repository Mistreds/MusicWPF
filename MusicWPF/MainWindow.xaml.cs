﻿using System;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Media;
using Windows.Media.Control;
using Windows.Media.Playback;
using static System.Net.Mime.MediaTypeNames;
using MusicLibrary;
namespace MusicWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        
        private GlobalSystemMediaTransportControlsSessionManager gsmtcsm;
        private GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties;
        private readonly DoubleAnimation _oa;
        private readonly DoubleAnimation _oa1;
        private UpdateContent updateContent;
        private UpdateImage updateImage;
        private UpdatePlay updatePlay;
        private MusicControls musicControls;
        public MainWindow()
        {
            var primaryMonitorArea = SystemParameters.WorkArea;
            InitializeComponent();

            //ss
            updateContent=UpdateContentWin;
            updateImage=UpdateImageWin;
            updatePlay=UpdatePlayPause;
            _oa = new DoubleAnimation();
            _oa.From = primaryMonitorArea.Bottom;
            _oa.To = primaryMonitorArea.Bottom-Height-5;
            _oa.AutoReverse = false;
            _oa.Duration = new Duration(TimeSpan.FromMilliseconds(300d));
            _oa.Completed+=_oa_Completed;
            _oa1 = new DoubleAnimation();
            _oa1.From = primaryMonitorArea.Bottom-Height-5;
            _oa1.To =  primaryMonitorArea.Bottom;
            _oa1.AutoReverse = false;
            _oa1.Completed+=_oa1_Completed;
            _oa1.Duration = new Duration(TimeSpan.FromMilliseconds(300d));
            Left = primaryMonitorArea.Right - Width-5; 
            Tray.Icon=Properties.Resources.play_button1;
            // _ = Maisn();
            musicControls=new MusicControls(updateContent,updateImage,updatePlay);
            this.Loaded+=MainWindow_Loaded;
            Hide();
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }
        private void UpdateContentWin(string track, string singer, bool error)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
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
                    Singer.Text=singer;
                }
            });
        }
        private void UpdateImageWin(byte[] _image)
        {
            if (_image==null)
                return;
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Image.Source = ToImage(_image);
            });
        }
        private void _oa_Completed(object sender, EventArgs e)
        {
            this.Topmost=true;
        }

        private void _oa1_Completed(object sender, EventArgs e)
        {
            this.Hide();
        }

     
        private void UpdatePlayPause(string type)
            {
                if (type=="Paused")
                {
                   
                    Tray.Icon=Properties.Resources.play_button1;
                StartStop.Content=this.FindResource("Start");



            }
                else
                {
                    
                    Tray.Icon=Properties.Resources.video_pause_button1;
                    StartStop.Content=this.FindResource("Stop");


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
                      
        private  void Back_Click(object sender, RoutedEventArgs e) => musicControls.BackButton();
        private  void Next_Click(object sender, RoutedEventArgs e) => musicControls.NextButton();

        private  void StartStop_Click(object sender, RoutedEventArgs e) => musicControls.StartStop();
        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            Activate();
            Console.WriteLine(this.IsVisible);
            if (this.IsVisible)
            {
                Topmost=false;
                BeginAnimation(TopProperty, _oa1);
            }
                
            else
            {
                Topmost=false;
                this.Show();
                BeginAnimation(TopProperty, _oa);
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
