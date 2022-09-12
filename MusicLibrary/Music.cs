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
            GetGSMT();
            MusicInfo= new MusicInfo(); 
                Timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 500) };
                Timer.Tick += (o, O) => UpdatePlayback();
                Timer.Start();
                GC.Collect();


        }
        private async void GetGSMT() => gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
        private async void UpdatePlayback()
        {
            using (null)
            {
                try
                {
                  
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
                        bytes=null;
                    }
                }
            }

            catch
            {
                updateImage(null);
            }

            GC.Collect();
        }

        private async Task<GlobalSystemMediaTransportControlsSessionManager> GetSystemMediaTransportControlsSessionManager() =>
          await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

        private async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaProperties(GlobalSystemMediaTransportControlsSession session)
     => await session.TryGetMediaPropertiesAsync();
        public  async void NextButton()
        {
            try
            {
   
                var CurrSession = gsmtcsm.GetCurrentSession();

                _=  CurrSession.TrySkipNextAsync();
                
            }
            catch 
            {
                return;
            }
            GC.Collect();
        }
        public async void BackButton()
        {
            try
            {
                
                var CurrSession = gsmtcsm.GetCurrentSession();
                _=  CurrSession.TrySkipPreviousAsync();
                

            }
            catch
            {
                GC.Collect();
                return;
            }
            GC.Collect();
        }
        public async void StartStop()
        {
            try
            {
                
                var CurrSession = gsmtcsm.GetCurrentSession();
                var play_back = CurrSession.GetPlaybackInfo();
                if (play_back.PlaybackStatus.ToString()=="Paused")
                {
                   _= CurrSession.TryPlayAsync();
                }
                else
                {
                    _= CurrSession.TryPauseAsync();
                }
                
                GC.Collect();
            }
            catch { GC.Collect(); return; }
            GC.Collect();
        }
    }
    }

