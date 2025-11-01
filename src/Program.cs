using BiliInteractiveVideoResolver.API;

namespace BiliInteractiveVideoResolver;

public static class Program
{
    public static async Task Main(string[] args)
    {
        using HttpClient httpClient = new();
        if (args.Length == 0)
        {
            while (true)
            {
                Console.Error.WriteLine("AID/BVID:");
                string? line = Console.In.ReadLine();
                if (line is null)
                    break;
                await ProcessId(httpClient, line);
                Console.Out.WriteLine();
            }
        }
        else
        {
            foreach (string line in args)
            {
                await ProcessId(httpClient, line);
                Console.Out.WriteLine();
            }
        }
    }

    public static async Task ProcessId(HttpClient httpClient, string id)
    {
        try
        {
            XWebInterfaceViewDetail.Root detail;
            if ((id.StartsWith("av", StringComparison.OrdinalIgnoreCase) && ulong.TryParse(id.AsSpan(2), out ulong aid))
                || ulong.TryParse(id, out aid))
                detail = await XWebInterfaceViewDetail.GetAsync(httpClient, aid: aid);
            else
                detail = await XWebInterfaceViewDetail.GetAsync(httpClient, bvid: id);
            if (detail.Data is null)
            {
                WriteError(null, detail.Message);
                return;
            }
            ulong cid = detail.Data.View.Cid;
            XPlayerV2.Root player = await XPlayerV2.GetAsync(httpClient, cid, detail.Data.View.Aid);
            if (player.Data is null)
            {
                WriteError(null, player.Message);
                return;
            }
            ulong graphVersion = player.Data.Interaction.GraphVersion;
            XSteinEdgeinfoV2.Root edge = await XSteinEdgeinfoV2.GetAsync(httpClient, graphVersion, detail.Data.View.Aid);
            if (edge.Data is null)
            {
                WriteError(null, edge.Message);
                return;
            }
            HashSet<ulong> eids = [];
            Stack<(XSteinEdgeinfoV2.Choice[], int)> stack = [];
            stack.Push(([new() { Id = edge.Data.EdgeId, Cid = cid }], 0));
            while (stack.Count > 0)
            {
                (XSteinEdgeinfoV2.Choice[] choices, int index) = stack.Pop();
                if (index >= choices.Length)
                    continue;
                XSteinEdgeinfoV2.Choice choice = choices[index++];
                stack.Push((choices, index));
                if (eids.Add(choice.Id))
                {
                    edge = await XSteinEdgeinfoV2.GetAsync(httpClient, graphVersion, detail.Data.View.Aid, edge_id: choice.Id);
                    if (edge.Data is null)
                    {
                        WriteError(null, edge.Message);
                        return;
                    }
                    Console.Out.WriteLine($"{choice.Id}:{choice.Cid}:{edge.Data.Title?.ReplaceLineEndings("")}");
                    XSteinEdgeinfoV2.Choice[]? a = edge.Data.Edges.Questions?.SelectMany(it => (IEnumerable<XSteinEdgeinfoV2.Choice>?)it.Choices ?? []).ToArray();
                    stack.Push((a ?? [], 0));
                }
            }
        }
        catch (Exception ex)
        {
            WriteError(ex, null);
        }
        static void WriteError(Exception? e, string? message)
        {
            string msg = e?.Message ?? message ?? "";
            Console.Out.Write('!');
            Console.Out.WriteLine(msg.ReplaceLineEndings(""));
            Console.Error.WriteLine((object?)e ?? message);
        }
    }
}