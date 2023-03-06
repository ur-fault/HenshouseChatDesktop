using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;

namespace HenshouseChat;

public class RSAOAEP : IAsymmetric, IDisposable
{
    public RSA Key { get; }

    public RSAOAEP() {
#pragma warning disable CA1416
        Key = new RSACng();
#pragma warning restore CA1416
    }

    public RSAOAEP(ReadOnlySpan<byte> publicKey) {
        var rsa = new RSACng();
        rsa.ImportSubjectPublicKeyInfo(publicKey, out var read);
        if (read != publicKey.Length)
            throw new AggregateException();

        Key = rsa;
    }

    public byte[] Encode(string decoded) => Encode(Encoding.UTF8.GetBytes(decoded));

    public byte[] Encode(byte[] decoded) => Key.Encrypt(decoded, RSAEncryptionPadding.OaepSHA256);

    public byte[] Decode(byte[] encoded) => Key.Decrypt(encoded, RSAEncryptionPadding.OaepSHA256);

    public string DecodeToString(byte[] encoded) => Encoding.UTF8.GetString(Decode(encoded));

    public string ExportPublicPem() {
        var parameters = Key.ExportParameters(false);
        
        AsnWriter writer = new AsnWriter(AsnEncodingRules.DER);
        writer.PushSequence();
        writer.WriteInteger(parameters.Modulus);
        writer.WriteInteger(parameters.Exponent);
        writer.PopSequence();
        byte[] rsaPublicKey = writer.Encode();

        string base64 = Convert.ToBase64String(rsaPublicKey);
        string pem = $"-----BEGIN RSA PUBLIC KEY-----\n{base64}\n-----END RSA PUBLIC KEY-----";
        return pem;
    }

    public byte[] ExportPublic() => Key.ExportRSAPublicKey();

    public void Dispose() {
        Key.Dispose();
    }
}