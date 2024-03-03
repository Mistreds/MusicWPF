using MusicSwitcher.Model;
using ReactiveUI;
using System.Reactive;

namespace MusicSwitcher.ViewModel;

public class MainViewModel:ReactiveObject
{
    public MusicModel MusicModel { get; set; }
    private IAnimation animation;
    public MainViewModel(MusicModel _musicModel)
    {
       
        this.MusicModel = _musicModel;
        
    }

    public ReactiveCommand<Unit, Unit> CreateQuiz => ReactiveCommand.Create(() =>
    {

    });

}