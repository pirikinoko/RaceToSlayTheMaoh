using Unity.Properties;
using UnityEngine.UIElements;
using System;
using System.Runtime.CompilerServices;

public class TitleTextData : INotifyBindablePropertyChanged
{
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    // propertyChanged イベントを発行するメソッド
    private void Notify([CallerMemberName] string propertyName = null)
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(propertyName));
    }

    [CreateProperty]
    public string LocalPlayButtonText
    {
        get => LocalPlayButtonText;
        set { LocalPlayButtonText = value; Notify(); }
    }

    [CreateProperty]
    public string MatchmakingButtonText
    {
        get => MatchmakingButtonText;
        set { MatchmakingButtonText = value; Notify(); }
    }
}