namespace NineSimulator.Model;

using System.Text.Json;
using System.Text.Json.Serialization;

public struct Arena
{
    [JsonPropertyName("rankid")]
    public int Rank { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("avatarname")]
    public required string AvatarName { get; set; }

    [JsonPropertyName("avataraddress")]
    public required string AvatarAddress { get; set; }

    [JsonPropertyName("cp")]
    public int Cp { get; set; }

    [JsonConstructor]
    public Arena(int rank, int score, string avatarName, string avatarAddress, int cp) =>
        (Rank, Score, AvatarName, AvatarAddress, Cp) = (rank, score, avatarName, avatarAddress, cp);
}
