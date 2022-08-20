using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Windows.Media.Control;

namespace MusicLibrary
{
   public delegate void UpdateContent(string track, string singer,bool error);
   public delegate void UpdateImage(byte[] image);
    public delegate void UpdatePlay(string type);
    public class MusicControls
    {
        private GlobalSystemMediaTransportControlsSessionManager gsmtcsm;
        private GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties;
        private DispatcherTimer Timer;
        UpdateContent updateContent;
        UpdateImage updateImage;
        UpdatePlay updatePlay;
        private MusicInfo MusicInfo;
        int i = 0;
        public MusicControls(UpdateContent updateContent, UpdateImage updateImage,UpdatePlay updatePlay)
        {
            this.updateContent = updateContent;
            this.updateImage= updateImage;
            this.updatePlay=updatePlay;
            MusicInfo= new MusicInfo(); 
                var timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 500) };
                timer.Tick += (o, O) => UpdatePlayback();
                timer.Start();
                GC.Collect();


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
                    updateContent(null, null, true);
                    updatePlay("Paused");
                    GC.Collect();
                    return;
                }
                if (gsmtcsm==null)
                    return;
                var CurrSession = gsmtcsm.GetCurrentSession();
                var play_back = CurrSession.GetPlaybackInfo();
                updatePlay(play_back.PlaybackStatus.ToString());
                MusicInfo musicInfo=new MusicInfo(mediaProperties.Title, mediaProperties.Artist);
                if (musicInfo.ToString()==MusicInfo.ToString())
                {
                    if (i<5)
                    {
                        GetImage();
                        i++;
                    }
                        

                    return;
                }
                i=0;
                MusicInfo=new MusicInfo(mediaProperties.Title, mediaProperties.Artist);
                updateContent(mediaProperties.Title, mediaProperties.Artist, false);

                GetImage();
                gsmtcsm=null;
                mediaProperties=null;
                GC.Collect();
            }
        }
        private async void GetImage()
        {
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
                        if (MusicInfo.Image==bytes)
                            return;
                        updateImage(bytes);
                        MusicInfo.Image=bytes;
                    }
                }
            }

            catch
            {
                updateImage(null);
            }
        }

        private async Task<GlobalSystemMediaTransportControlsSessionManager> GetSystemMediaTransportControlsSessionManager() =>
          await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

        private async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaProperties(GlobalSystemMediaTransportControlsSession session)
     => await session.TryGetMediaPropertiesAsync();
        public  async void NextButton()
        {
            try
            {
                gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
                var CurrSession = gsmtcsm.GetCurrentSession();

                await CurrSession.TrySkipNextAsync();
                gsmtcsm=null;

            }
            catch 
            { 
            }
        }
        public async void BackButton()
        {
            try
            {
                gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
                var CurrSession = gsmtcsm.GetCurrentSession();

                await CurrSession.TrySkipPreviousAsync();
                gsmtcsm=null;

            }
            catch
            {
            }
        }
        public async void StartStop()
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
    }
    }

