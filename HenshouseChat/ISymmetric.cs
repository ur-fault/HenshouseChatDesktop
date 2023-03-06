namespace HenshouseChat;

public interface ISymmetric
{
    byte[] Encode(string decoded);
    byte[] Encode(byte[] decoded);
    byte[] Decode(byte[] encoded);
    string DecodeToString(byte[] encoded);

    byte[] Export();
}