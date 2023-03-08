using System.Net.WebSockets;

namespace HenshouseChat.Extensions;

public static class WebSocketExtensions
{
    public static async Task<byte[]?> ReceiveSingleAsync(this ClientWebSocket ws, CancellationToken ct = default) {
        var tmpBuffer = new byte[1024];
        var receiveBuffer = new MemoryStream();

        while (true) {
            var segment = new ArraySegment<byte>(tmpBuffer);
            var receiveResult = await ws.ReceiveAsync(segment, ct);

            receiveBuffer.Write(tmpBuffer, 0, receiveResult.Count);
            if (receiveResult.EndOfMessage)
                break;

            if (receiveResult.MessageType == WebSocketMessageType.Close)
                return null;
        }

        receiveBuffer.Seek(0, SeekOrigin.Begin);
        var buffer = new byte[receiveBuffer.Length];
        if (await receiveBuffer.ReadAsync(buffer) != buffer.Length)
            throw new EndOfStreamException("Could not read enough data");

        return buffer;
    }
}
