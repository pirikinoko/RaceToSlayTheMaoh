using Unity.Properties;
using UnityEngine.UIElements;
using System;
using System.Runtime.CompilerServices;

public class TitleTextData : INotifyBindablePropertyChanged
{
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    // バッキングフィールド
    private string _localPlayButtonText;
    private string _matchmakingButtonText;

    // propertyChanged イベントを発行するメソッド
    private void Notify([CallerMemberName] string propertyName = null)
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(propertyName));
    }

    [CreateProperty]
    public string LocalPlayButtonText
    {
        get => _localPlayButtonText;
        set
        {
            if (_localPlayButtonText != value)
            {
                _localPlayButtonText = value;
                Notify();
            }
        }
    }

    [CreateProperty]
    public string MatchmakingButtonText
    {
        get => _matchmakingButtonText;
        set
        {
            if (_matchmakingButtonText != value)
            {
                _matchmakingButtonText = value;
                Notify();
            }
        }
    }
}
