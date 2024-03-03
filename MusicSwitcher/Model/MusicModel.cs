using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MusicSwitcher.Model;

public class MusicModel:ReactiveObject
{
    /// <summary>
    /// Название песни
    /// </summary>
    [Reactive]
    public string SingName { get; set; } = "";
    /// <summary>
    /// Название альбома
    /// </summary>

    [Reactive] public string AlbumName { get; set; } = "";
    /// <summary>
    /// Название исполнителя
    /// </summary>
    [Reactive] public string SingerName { get; set; } = "";

    [Reactive] public string Status { get; set; } = "Stoped";
}