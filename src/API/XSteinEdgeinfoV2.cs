using BiliInteractiveVideoResolver;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibBiliInteractiveVideo.API;

/// <summary>
/// x/stein/edgeinfo_v2
/// </summary>
public static class XSteinEdgeinfoV2
{
    public static event Action<string>? RequestReady;
    public static event Action<string>? RawJsonReceived;

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

        [JsonPropertyName("hidden_vars")]
        public List<HiddenVar>? HiddenVars { get; set; }

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

        [JsonPropertyName("native_action")]
        public string? NativeAction { get; set; }

        [JsonPropertyName("condition")]
        public string? Condition { get; set; }

        [JsonPropertyName("option")]
        public string? Option { get; set; }
    }

    public struct HiddenVar
    {
        [JsonPropertyName("value")]
        public double Value { get; set; }

        [JsonPropertyName("id_v2")]
        public string? IdV2 { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("is_show")]
        public int IsShow { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public static async Task<Root> GetAsync(
        HttpClient client,
        ulong graph_version,
        ulong? aid = null,
        string? bvid = null,
        ulong? edge_id = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string url = $"https://api.bilibili.com/x/stein/edgeinfo_v2?graph_version={graph_version}{(aid is not null ? $"&aid={aid}" : "")}{(bvid is not null ? $"&bvid={bvid}" : "")}{(edge_id is not null ? $"&edge_id={edge_id}" : "")}";
        RequestReady?.Invoke(url);
        if (RawJsonReceived is not null)
        {
            string json = await client.GetStringAsync(url, cancellationToken);
            RawJsonReceived?.Invoke(json);
            return JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.XSteinEdgeinfoV2_Root);
        }
        return await client.GetFromJsonAsync(url, AppJsonSerializerContext.Default.XSteinEdgeinfoV2_Root, cancellationToken);
    }
}