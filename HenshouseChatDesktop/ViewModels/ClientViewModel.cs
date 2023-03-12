using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using HenshouseChat;

namespace HenshouseChatDesktop.ViewModels;

public class ClientViewModel : INotifyPropertyChanged
{
    private Client? _client;

    public Client? Client {
        get => _client;
        set {
            SetField(ref _client, value);
            OnPropertyChanged(nameof(IsConnected));
            Task.Run(async () => {
                if (_client is null)
                    return;

                if (_nickname is not null)
                    await _client?.SetNickname(_nickname)!;

                await Client!.ListenAsync(
                    msg => {
                        OnPropertyChanged(nameof(Nickname));
                        Application.Current.Dispatcher.BeginInvoke(() => {
                            Messages.Add(msg);
                            OnPropertyChanged(nameof(Messages));
                            OnMessage?.Invoke(msg);
                        });
                    }, OnError, OnNormalClose);
            });
        }
    }

    public ObservableCollection<ServerMessage> Messages { get; } = new();

    public bool IsConnected => _client != null;

    public void Disconnect() {
        Client?.Disconnect();
        Client = null;
        MessageBox.Show("Disconnected");
    }

    public Action<ServerMessage>? OnMessage;
    public Action<Exception?>? OnError;
    public Action? OnNormalClose;

    private string? _nickname;

    public string? Nickname {
        get => _client?.Nickname ?? _nickname;
        set {
            if (value is null) return;
            _nickname = value;

            if (_client is null) return;
            _client?.SetNickname(value);
            OnPropertyChanged();
        }
    }

    private string _currentMessage = "";

    public string CurrentMessage {
        get => _currentMessage;
        set => SetField(ref _currentMessage, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}