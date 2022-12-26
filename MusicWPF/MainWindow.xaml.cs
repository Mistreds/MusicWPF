using MusicLibrary;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Windows.Media.Control;
using static System.Net.Mime.MediaTypeNames;
namespace MusicWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GlobalSystemMediaTransportControlsSessionManager gsmtcsm;
        private UpdateContent updateContent;
        private UpdateImage updateImage;
        private UpdatePlay updatePlay;
        private MusicControls musicControls;
        private Animation Animation;
        public MainWindow()
        {
            var primaryMonitorArea = SystemParameters.WorkArea;
            InitializeComponent();
            Loaded+=MainWindow_Loaded;
            updateContent=UpdateContentWin;
            updateImage=UpdateImageWin;
            updatePlay=UpdatePlayPause;
            Left = primaryMonitorArea.Right - Width-5;
            Tray.Icon=Properties.Resources.play_button;

        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            Animation=new Animation(this);
            musicControls=new MusicControls(updateContent, updateImage, updatePlay);
            Hide();
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
        private void UpdateImageWin(byte[] _image)
        {
            if (_image==null)
                return;
            App.Current.Dispatcher.Invoke(delegate
            {
                var image = ConvertImage.ToBitmapImage(_image);
                Image.Source = image;
                Border.Background=ConvertImage.GetColor(image);
            });
        }
        private void StartTrackAnimation()
        {
            Track.BeginAnimation(Canvas.LeftProperty, null);
            Track1.BeginAnimation(Canvas.LeftProperty, null);
            if (Track.ActualWidth > 230)
            {
                Animation.UpdateTrackAnimation();
                Track.BeginAnimation(Canvas.LeftProperty, Animation.StartFirstTrack);
                Track1.BeginAnimation(Canvas.LeftProperty, Animation.StartSecondTrack);

            }
        }
        private void UpdatePlayPause(string type)
        {
            if (type=="Paused")
            {
                Tray.Icon=Properties.Resources.play_button;
                StartStop.Content=this.FindResource("Start");
            }
            else
            {
                Tray.Icon=Properties.Resources.video_pause_button1;
                StartStop.Content=this.FindResource("Stop");
            }
        }
        private void Back_Click(object sender, RoutedEventArgs e) => musicControls.BackButton();
        private void Next_Click(object sender, RoutedEventArgs e) => musicControls.NextButton();
        private void StartStop_Click(object sender, RoutedEventArgs e) => musicControls.StartStop();
        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            Activate();
            if (this.IsVisible)
            {
                Topmost=false;
                BeginAnimation(TopProperty, Animation.OnHide);
            }
            else
            {
                Topmost=false;
                this.Show();
                BeginAnimation(TopProperty, Animation.OnShow);
                WinApiLib.HideFromAltTab(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            }

        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        private void Track_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetLeft(Track, 0);
            Canvas.SetLeft(Track1, Track1.ActualWidth+360);
            StartTrackAnimation();
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("MusicWPF.exe");
            Process.GetCurrentProcess().Kill();
        }
    }
}
