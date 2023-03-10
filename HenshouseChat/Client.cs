using HenshouseChat.Extensions;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace HenshouseChat;

public class Client : IDisposable
{
    private readonly ClientWebSocket _ws;
    private readonly IAsymmetric _localAsymmetric;
    private readonly IAsymmetric _remoteAsymmetric;

    public ISymmetric Symmetric { get; }

    private string _nickname = "";
    public string Nickname => _nickname;

    private int _id;
    public int Id => _id;

    private CancellationTokenSource? _listenCancellationTokenSource;

    private Action<ServerMessage>? _onMessage;
    private Action<Exception>? _onError;
    private Action? _onNormalClose;


    private Client(ClientWebSocket ws, IAsymmetric localAsymmetric, IAsymmetric remoteAsymmetric,
        ISymmetric symmetric) {
        _ws = ws;
        _localAsymmetric = localAsymmetric;
        _remoteAsymmetric = remoteAsymmetric;
        Symmetric = symmetric;
    }

    ~Client() {
        Dispose();
    }

    public static async Task<Client> ConnectTo(string domain, int port = 25017,
        CancellationToken ct = default) {
        var ws = new ClientWebSocket();
        await ws.ConnectAsync(new Uri($"ws://{domain}:{port}"), ct);

        var remoteAsymmetric = new RSAOAEP(await ws.ReceiveSingleAsync(ct));
        var localAsymmetric = await Task.Run(() => new RSAOAEP(), ct);
        await ws.SendAsync(localAsymmetric.ExportPublic(), WebSocketMessageType.Binary, true, ct);

        var symmetric =
            new AESGCM(localAsymmetric.Decode(await ws.ReceiveSingleAsync(ct) ??
                                              throw new InvalidOperationException()));

        return new Client(ws, localAsymmetric, remoteAsymmetric, symmetric);
    }

    public async Task ListenAsync(Action<ServerMessage> onMessage, Action<Exception?>? onError = null,
        Action? onNormalClose = null, CancellationToken ct = default) {
        _listenCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);

        _onMessage = onMessage;
        _onError = onError;
        _onNormalClose = onNormalClose;

        try {
            while (true) {
                byte[]? encoded;
                try {
                    encoded = await _ws.ReceiveSingleAsync(_listenCancellationTokenSource.Token);
                }
                catch (WebSocketException e) {
                    _onError?.Invoke(e);
                    return;
                }

                if (encoded is null) {
                    _onNormalClose?.Invoke();
                    return;
                }

                string decoded;
                try {
                    decoded = Symmetric.DecodeToString(encoded);
                }
                catch (InvalidOperationException e) {
                    onError?.Invoke(e);
                    return;
                }

                _onMessage(JsonSerializer.Deserialize<ServerMessage>(decoded) ??
                           throw new InvalidOperationException($"Could not parse {decoded}"));
            }
        }
        finally {
            await Disconnect();
        }
    }

    private async Task SendAsync(byte[] data, CancellationToken ct = default) =>
        await _ws.SendAsync(Symmetric.Encode(data), WebSocketMessageType.Binary, true,
            CancellationToken.None);

    private async Task SendAsync(string data, CancellationToken ct = default) =>
        await SendAsync(Encoding.UTF8.GetBytes(data), ct);

    public async Task SendData(Message msg, CancellationToken ct = default) {
        await SendAsync(await Task.Run(() => JsonSerializer.Serialize(msg), ct), ct);
    }

    public async Task SendMessage(string content, CancellationToken ct = default) {
        await SendData(Message.NewMessage(content), ct);
    }

    public async Task SendCommand(string command, string args, CancellationToken ct = default) {
        await SendData(Message.NewCommand(command, args), ct);
    }

    public async Task Disconnect() {
        _listenCancellationTokenSource?.Cancel();
        try {
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
        }
        catch (WebSocketException e) {
            _onError?.Invoke(e);
        }

        Dispose();
    }

    public async Task SetNickname(string newNick, CancellationToken ct = default) {
        _nickname = newNick;
        await SendCommand("nick", newNick, ct);
    }

    public void Dispose() {
        //Task.Run(async () => {
        //    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "User requests disconnection",
        //        CancellationToken.None);

        //    _ws.Dispose();
        //});

        if (_localAsymmetric is IDisposable localAsymmetric)
            localAsymmetric.Dispose();

        if (_remoteAsymmetric is IDisposable remoteAsymmetric)
            remoteAsymmetric.Dispose();

        if (Symmetric is IDisposable symmetric)
            symmetric.Dispose();
    }
}