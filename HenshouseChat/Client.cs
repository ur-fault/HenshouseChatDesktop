using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using HenshouseChat.Extensions;

namespace HenshouseChat;

public class Client : IDisposable
{
    private ClientWebSocket _ws;
    private IAsymmetric _localAsymmetric;
    private IAsymmetric _remoteAsymmetric;

    private ISymmetric _symmetric;

    public ISymmetric Symmetric => _symmetric;

    private string _nickname = "";
    public string Nickname => _nickname;

    private int _id;
    public int Id => _id;

    private Client(ClientWebSocket ws, IAsymmetric localAsymmetric, IAsymmetric remoteAsymmetric,
        ISymmetric symmetric) {
        _ws = ws;
        _localAsymmetric = localAsymmetric;
        _remoteAsymmetric = remoteAsymmetric;
        _symmetric = symmetric;
    }

    public static async Task<Client> ConnectTo(string domain, int port = 25017, CancellationToken ct = default) {
        var ws = new ClientWebSocket();
        await ws.ConnectAsync(new Uri($"ws://{domain}:{port}"), ct);

        var remoteAsymmetric = new RSAOAEP(await ws.ReceiveSingleAsync(ct));
        var localAsymmetric = await Task.Run(() => new RSAOAEP(), ct);
        await ws.SendAsync(localAsymmetric.ExportPublic(), WebSocketMessageType.Binary, true, ct);

        var symmetric = new AESGCM(localAsymmetric.Decode(await ws.ReceiveSingleAsync(ct)));

        return new Client(ws, localAsymmetric, remoteAsymmetric, symmetric);
    }

    public async Task ListenAsync(Action<ServerMessage> callback, CancellationToken ct = default) {
        try {
            while (true) {
                var encoded = await _ws.ReceiveSingleAsync(ct: ct);
                var decoded = Symmetric.DecodeToString(encoded);
                
                callback(JsonSerializer.Deserialize<ServerMessage>(decoded) ??
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

    public async Task Disconnect(CancellationToken ct = default) {
        try {
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
        }
        catch (WebSocketException) { }

        Dispose();
    }

    public void Dispose() {
        _ws.Dispose();

        if (_localAsymmetric is IDisposable localAsymmetric)
            localAsymmetric.Dispose();

        if (_remoteAsymmetric is IDisposable remoteAsymmetric)
            remoteAsymmetric.Dispose();

        if (_symmetric is IDisposable symmetric)
            symmetric.Dispose();
    }
}