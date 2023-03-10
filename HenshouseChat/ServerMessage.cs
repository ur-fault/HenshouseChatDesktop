using System.Text.Json.Serialization;

namespace HenshouseChat;

public record ServerMessage : Message
{
    [JsonPropertyName("author")] public string Author { get; set; }
    [JsonPropertyName("author_id")] public long AuthorId { get; set; }

    [JsonPropertyName("recipient")] public string Recipient { get; set; }
    [JsonPropertyName("recipient_id")] public long RecipientId { get; set; }
}