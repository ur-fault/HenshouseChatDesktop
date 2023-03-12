using System;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HenshouseChat;
using HenshouseChatDesktop.ViewModels;

namespace HenshouseChatDesktop;

public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error,
}

public enum SendButtonState
{
    Normal,
    Disabled,
}

public partial class MainWindow : Window
{
    #region DP

    public ConnectionStatus Status =>
        (ConnectionStatus)GetValue(StatusProperty.DependencyProperty);

    internal static readonly DependencyPropertyKey StatusProperty =
        DependencyProperty.RegisterReadOnly(nameof(Status), typeof(ConnectionStatus),
            typeof(MainWindow),
            new PropertyMetadata(ConnectionStatus.Disconnected,
                (o, args) => o.SetValue(SendButtonStateProperty,
                    (ConnectionStatus)args.NewValue == ConnectionStatus.Connected
                        ? SendButtonState.Normal
                        : SendButtonState.Disabled)));

    public Visibility WatermarkVisibility =>
        (Visibility)GetValue(WatermarkVisibilityProperty.DependencyProperty);

    public static readonly DependencyPropertyKey WatermarkVisibilityProperty =
        DependencyProperty.RegisterReadOnly(nameof(WatermarkVisibility), typeof(Visibility),
            typeof(MainWindow),
            new PropertyMetadata(Visibility.Visible));


    public SendButtonState SendButtonState {
        get => (SendButtonState)GetValue(SendButtonStateProperty);
        set => SetValue(SendButtonStateProperty, value);
    }

    public static readonly DependencyProperty SendButtonStateProperty =
        DependencyProperty.Register(nameof(SendButtonState), typeof(SendButtonState), typeof(MainWindow),
            new PropertyMetadata(SendButtonState.Disabled));

    #endregion

    private static readonly Regex DomainNameRegex =
        new(@"^((?!-)[A-Za-z0-9-]{1,63}(?<!-)\.)+[A-Za-z]{2,6}$");

    private static readonly Regex Ipv4Regex =
        new(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$");

    #region VM

    private ServerConnectionInfoViewModel ConnectionInfo =>
        (ServerConnectionInfoViewModel)ConnectionInfoView.DataContext;

    public ClientViewModel ClientViewModel => (ClientViewModel)DataContext;
    private readonly CancellationTokenSource _connectionCancel = new();

    #endregion

    private bool _scrollToEnd = true;

    public MainWindow() {
        InitializeComponent();

        DomainTextBox.ValidationFunction =
            (tb, s) => DomainNameRegex.IsMatch(s) || Ipv4Regex.IsMatch(s);

        PortTextBox.ValidationFunction =
            (tb, s) => short.TryParse(s, out var _);

        ClientViewModel.PropertyChanged += (_, args) => {
            if (args.PropertyName == nameof(ClientViewModel.CurrentMessage))
                SetValue(WatermarkVisibilityProperty,
                    string.IsNullOrEmpty(ClientViewModel.CurrentMessage)
                        ? Visibility.Visible
                        : Visibility.Hidden);
        };

        ClientViewModel.OnMessage = message =>
            Dispatcher.BeginInvoke(() => {
                if (!_scrollToEnd) return;

                var viewer = GetScrollViewer(MessagesListView) ??
                             throw new InvalidOperationException("Cannot find ScrollViewer");
                viewer.ScrollToEnd();
            });

        ClientViewModel.OnError = error =>
            Dispatcher.BeginInvoke(() => {
                    ClientViewModel.Disconnect();
                    SetValue(StatusProperty, ConnectionStatus.Error);
                    MessageBox.Show(error?.Message);
                }
            );

        DomainTextBox.FocusInput();
        NicknameTextBox.OnEnterPressedAction = () => MessageTextBox.Focus();

        // FIXME: don't scroll to the end on new message when user was not already there
        //MessagesListView.LayoutUpdated += (_, _) => GetScrollViewer(MessagesListView).ScrollChanged +=
        //    (sender, e) => {
        //        _scrollToEnd = e.ExtentHeight + e.VerticalOffset > e.ViewportHeight;
        //    };
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

    private async void SendButton_OnClick(object sender, RoutedEventArgs _) {
        var msgText = MessageTextBox.Text;
        if (string.IsNullOrWhiteSpace(msgText))
            return;

        var msg = msgText.StartsWith('/')
            ? Message.NewCommand(msgText[1..].Split(' ')[0], string.Join(' ', msgText.Split(' ')[1..]))
            : Message.NewMessage(msgText);

        MessageTextBox.Text = "";
        MessageTextBox.Focus();
        if (ClientViewModel.Client?.SendData(msg) is { } t)
            await t;
    }

    private void MessageTextBox_OnKeyDown(object sender, KeyEventArgs e) {
        if (e.Key == Key.Enter)
            SendButton_OnClick(sender, new RoutedEventArgs());
    }

    // From ChatGPT
    private static ScrollViewer? GetScrollViewer(DependencyObject element) {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++) {
            var child = VisualTreeHelper.GetChild(element, i);
            if (child is ScrollViewer scrollViewer)
                return scrollViewer;

            var result = GetScrollViewer(child);
            if (result != null)
                return result;
        }

        return null;
    }

    // From ChatGPT
    private static bool IsAtListViewEnd(DependencyObject listView) {
        var scrollViewer = GetScrollViewer(listView) ??
                           throw new InvalidOperationException("Cannot find ScrollViewer");

        var offset = scrollViewer.VerticalOffset;
        var extent = scrollViewer.ExtentHeight;
        var viewport = scrollViewer.ViewportHeight;

        return offset + viewport >= extent;
    }
}