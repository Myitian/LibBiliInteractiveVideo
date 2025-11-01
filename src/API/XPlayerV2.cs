#if DEBUG
using System.Text.Json;
#else
using System.Net.Http.Json;
#endif
using System.Text.Json.Serialization;

namespace BiliInteractiveVideoResolver.API;

/// <summary>
/// x/player/v2
/// </summary>
public class XPlayerV2
{
    public struct Root
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("interaction")]
        public Interaction Interaction { get; set; }
    }

    public struct Interaction
    {
        [JsonPropertyName("graph_version")]
        public ulong GraphVersion { get; set; }
    }

    public static async Task<Root> GetAsync(HttpClient client, ulong cid, ulong? aid = null, string? bvid = null)
    {
        string url = $"https://api.bilibili.com/x/player/v2?cid={cid}{(aid is not null ? $"&aid={aid}" : "")}{(bvid is not null ? $"&bvid={bvid}" : "")}";
#if DEBUG
        Console.Error.WriteLine(url);
        string json = await client.GetStringAsync(url);
        File.WriteAllText($"XPlayerV2.{DateTime.UtcNow.Ticks}.json", json);
        return JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.XPlayerV2_Root);
#else
        return await client.GetFromJsonAsync(url, AppJsonSerializerContext.Default.XPlayerV2_Root);
#endif
    }
}