#if DEBUG
using System.Text.Json;
#else
using System.Net.Http.Json;
#endif
using System.Text.Json.Serialization;

namespace BiliInteractiveVideoResolver.API;

/// <summary>
/// x/stein/edgeinfo_v2
/// </summary>
public static class XSteinEdgeinfoV2
{
    public struct Root
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public Data? Data { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("edge_id")]
        public ulong EdgeId { get; set; }

        [JsonPropertyName("edges")]
        public Edges Edges { get; set; }
    }

    public struct Edges
    {
        [JsonPropertyName("questions")]
        public List<Question>? Questions { get; set; }
    }

    public struct Question
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    public struct Choice
    {
        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("cid")]
        public ulong Cid { get; set; }
    }

    public static async Task<Root> GetAsync(HttpClient client, ulong graph_version, ulong? aid = null, string? bvid = null, ulong? edge_id = null)
    {
        string url = $"https://api.bilibili.com/x/stein/edgeinfo_v2?graph_version={graph_version}{(aid is not null ? $"&aid={aid}" : "")}{(bvid is not null ? $"&bvid={bvid}" : "")}{(edge_id is not null ? $"&edge_id={edge_id}" : "")}";
#if DEBUG
        Console.Error.WriteLine(url);
        string json = await client.GetStringAsync(url);
        File.WriteAllText($"XSteinEdgeinfoV2.{DateTime.UtcNow.Ticks}.json", json);
        return JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.XSteinEdgeinfoV2_Root);
#else
        return await client.GetFromJsonAsync(url, AppJsonSerializerContext.Default.XSteinEdgeinfoV2_Root);
#endif
    }
}