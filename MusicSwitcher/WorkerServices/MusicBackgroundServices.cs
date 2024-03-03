using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MusicSwitcher.Model;
using Windows.Media.Control;
namespace MusicSwitcher.WorkerServices
{
    public class MusicBackgroundServices:BackgroundService
    {

        private MusicModel _musicModel { get; set; }
        private GlobalSystemMediaTransportControlsSessionManager gsmtcsm;
        private GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties;

        public MusicBackgroundServices(MusicModel _musicModel)
        {
            GetGSMT();
            this._musicModel = _musicModel;
        }
        private async void GetGSMT() => gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    gsmtcsm = await GetSystemMediaTransportControlsSessionManager();
                    if (gsmtcsm.GetCurrentSession() == null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }
                    mediaProperties = await GetMediaProperties(gsmtcsm.GetCurrentSession());
                    
                    var CurrSession = gsmtcsm.GetCurrentSession();
                    var play_back = CurrSession.GetPlaybackInfo();
                    Console.WriteLine(play_back.PlaybackStatus.ToString());
                    _musicModel.AlbumName = mediaProperties.AlbumTitle;
                    _musicModel.SingName = mediaProperties.Title;
                    _musicModel.SingerName = mediaProperties.Artist;
                    _musicModel.Status = play_back.PlaybackStatus.ToString();
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                catch (Exception e)
                {
                 
                }
              
            }
           
        }
        private async Task<GlobalSystemMediaTransportControlsSessionManager> GetSystemMediaTransportControlsSessionManager() =>
            await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

        private async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaProperties(GlobalSystemMediaTransportControlsSession session)
            => await session.TryGetMediaPropertiesAsync();
    }
}
