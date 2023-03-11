using System;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using HenshouseChat;
using HenshouseChatDesktop.ViewModels;

namespace HenshouseChatDesktop;

public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error
}

public partial class MainWindow : Window
{
    public ConnectionStatus Status =>
        (ConnectionStatus)GetValue(StatusProperty.DependencyProperty);

    internal static readonly DependencyPropertyKey StatusProperty =
        DependencyProperty.RegisterReadOnly(nameof(Status), typeof(ConnectionStatus),
            typeof(MainWindow),
            new PropertyMetadata(ConnectionStatus.Disconnected));


    private static readonly Regex DomainNameRegex =
        new(@"^((?!-)[A-Za-z0-9-]{1,63}(?<!-)\.)+[A-Za-z]{2,6}$");

    private static readonly Regex Ipv4Regex =
        new(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$");

    private ServerConnectionInfoViewModel ConnectionInfo =>
        (ServerConnectionInfoViewModel)ConnectionInfoView.DataContext;

    public ClientViewModel ClientViewModel => (ClientViewModel)DataContext;
    private readonly CancellationTokenSource _connectionCancel = new();

    public MainWindow() {
        InitializeComponent();

        DomainTextBox.ValidationFunction =
            (tb, s) => DomainNameRegex.IsMatch(s) || Ipv4Regex.IsMatch(s);

        PortTextBox.ValidationFunction =
            (tb, s) => short.TryParse(s, out var _);

        //ClientViewModel.OnMessage =
        //    message => MessageBox.Show(
        //        (message.Type == MessageType.Message
        //            ? $"{message.Content}"
        //            : $"/{message.Command} {message.CommandArgs}") + $" by {message.Author}");
        ClientViewModel.OnMessage =
            message =>
                Dispatcher.BeginInvoke(() =>
                    MessageBox.Show(((message.Type == MessageType.Message)
                        ? $"{message.Content}"
                        : $"/{message.Command} {message.CommandArgs}") + $" by {message.Author}")
                );

        ClientViewModel.OnError =
            error =>
                Dispatcher.BeginInvoke(() => {
                        SetValue(StatusProperty, ConnectionStatus.Error);
                        MessageBox.Show(error?.Message);
                    }
                );

        //ClientViewModel.OnError =
        //    error => {
        //        SetValue(StatusProperty, ConnectionStatus.Error);
        //        Console.WriteLine(error);
        //        MessageBox.Show(error?.Message);
        //    };

        NicknameTextBox.OnEnterPressedAction = () => DomainTextBox.FocusInput();
    }

    private async void StatusButton_Click(object sender, RoutedEventArgs e) {
        switch (Status) {
            case ConnectionStatus.Connecting:
                _connectionCancel.Cancel();
                ClientViewModel.Disconnect();
                SetValue(StatusProperty, ConnectionStatus.Disconnected);
                break;
            case ConnectionStatus.Connected:
                ClientViewModel.Disconnect();
                SetValue(StatusProperty, ConnectionStatus.Disconnected);
                break;
            case ConnectionStatus.Disconnected:
            case ConnectionStatus.Error:
                try {
                    if (!DomainTextBox.IsValid) {
                        MessageBox.Show("Domain must be valid URI domain");
                        return;
                    }

                    if (!PortTextBox.IsValid) {
                        MessageBox.Show("Port number must be valid number between 1 and 65536");
                        return;
                    }

                    SetValue(StatusProperty, ConnectionStatus.Connecting);
                    var client = await Client.ConnectTo(ConnectionInfo.Domain,
                        int.Parse(ConnectionInfo.Port),
                        _connectionCancel.Token);
                    SetValue(StatusProperty, ConnectionStatus.Connected);
                    ClientViewModel.Client = client;
                }
                catch (WebSocketException ex) {
                    SetValue(StatusProperty, ConnectionStatus.Error);
                    MessageBox.Show($"Error connecting: {ex.Message}");
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}