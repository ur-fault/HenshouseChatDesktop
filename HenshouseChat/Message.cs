using System.Text.Json.Serialization;

namespace HenshouseChat;

public enum MessageType
{
    Message,
    Command
}

public class Message
{
    [JsonIgnore] public MessageType Type { get; set; }
    [JsonPropertyName("type")]
    public string TypeString {
        get => Type.ToString().ToLower();
        set => Type = value switch {
            "message" => MessageType.Message,
            "command" => MessageType.Command,
            _ => throw new InvalidOperationException($"Invalid message type {value}")
        };
    }
    
    [JsonPropertyName("content")] public string? Content { get; set; }
    [JsonPropertyName("command")] public string? Command { get; set; }
    [JsonPropertyName("command_args")] public string? CommandArgs { get; set; }

    public static Message NewMessage(string msg) => new Message {
        Type = MessageType.Message,
        Content = msg
    };

    public static Message NewCommand(string cmd, string args) => new Message {
        Type = MessageType.Command,
        Command = cmd,
        CommandArgs = args
    };
}