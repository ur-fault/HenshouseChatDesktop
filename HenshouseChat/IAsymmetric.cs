namespace HenshouseChat;

public interface IAsymmetric
{
    byte[] Encode(string decoded);
    byte[] Encode(byte[] decoded);
    byte[] Decode(byte[] encoded);
    string DecodeToString(byte[] encoded);

    byte[] ExportPublic();
}