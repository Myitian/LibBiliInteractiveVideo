#if DEBUG
using System.Text.Json;
#else
using System.Net.Http.Json;
#endif
using System.Text.Json.Serialization;

namespace BiliInteractiveVideoResolver.API;

/// <summary>
/// x/web-interface/view/detail
/// </summary>
public static class XWebInterfaceViewDetail
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
        [JsonPropertyName("View")]
        public View View { get; set; }
    }

    public struct View
    {
        [JsonPropertyName("aid")]
        public ulong Aid { get; set; }

        [JsonPropertyName("cid")]
        public ulong Cid { get; set; }
    }

    public static async Task<Root> GetAsync(HttpClient client, ulong? aid = null, string? bvid = null)
    {
        string url = $"https://api.bilibili.com/x/web-interface/view/detail?{(aid is not null ? $"&aid={aid}" : "")}{(bvid is not null ? $"&bvid={bvid}" : "")}";
#if DEBUG
        Console.Error.WriteLine(url);
        string json = await client.GetStringAsync(url);
        File.WriteAllText($"XWebInterfaceViewDetail.{DateTime.UtcNow.Ticks}.json", json);
        return JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.XWebInterfaceViewDetail_Root);
#else
        return await client.GetFromJsonAsync(url, AppJsonSerializerContext.Default.XWebInterfaceViewDetail_Root);
#endif
    }
}
