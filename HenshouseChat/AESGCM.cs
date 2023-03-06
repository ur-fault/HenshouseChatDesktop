using System.Security.Cryptography;
using System.Text;

namespace HenshouseChat;

public class AESGCM : ISymmetric, IDisposable
{
    private byte[] _keyBytes;
    public AesGcm Key { get; }

    public AESGCM() {
        _keyBytes = RandomNumberGenerator.GetBytes(16);
        Key = new AesGcm(_keyBytes);
    }

    public AESGCM(byte[] key) {
        _keyBytes = (byte[])key.Clone();
        Key = new AesGcm(key);
    }

    public byte[] Encode(string decoded) => Encode(Encoding.UTF8.GetBytes(decoded));

    public byte[] Encode(byte[] decoded) {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];

        var encrypted = new byte[decoded.Length];
        Key.Encrypt(nonce, decoded, encrypted, tag);

        return nonce.Concat(encrypted).Concat(tag).ToArray();
    }

    public byte[] Decode(byte[] encoded) {
        var encodedSpan = (Span<byte>)encoded;

        var tag = encodedSpan[^16..];
        var nonce = encodedSpan[..12];
        var content = encodedSpan[12..^16];

        var decoded = new byte[content.Length];

        Key.Decrypt(nonce, content, tag, decoded);

        return decoded;
    }

    public string DecodeToString(byte[] encoded) => Encoding.UTF8.GetString(Decode(encoded));

    public byte[] Export() => (byte[])_keyBytes.Clone();

    public void Dispose() {
        Key.Dispose();
    }
}