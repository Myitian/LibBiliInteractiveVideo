# LibBiliInteractiveVideo

Recursively resolves all parts of Bilibili interactive videos from an AID/BVID.

## ResolveAllEdges

### Input Format

Please choose one of the following two formats:

1. Pass all AIDs/BVIDs as process parameters.
2. Pass all AIDs/BVIDs as standard process input, one per line.

### Output Format

#### Standard Error

- Prompt message, `AID/BVID:`.
- Complete error message.

#### Standard Output

- Parsing results, each line in the format `{edge_id}:{cid}:{title}`, outputting a blank line after each input ID has been completely resolved.
- Single-line error message, starting with `!`.

## ResolveShortestPath

> [!NOTE]
>
> The `ResolveShortestPath` method has a `depthLimit` parameter, which defaults to 100. If the depth is too large, the program may take a long time to complete, consume too much memory, or be terminated due to insufficient memory.

## Input Format

Same as `ResolveAllEdges`.

### Output Format
The first line is `NODE:{number of nodes}`, then the following is repeated for each node:
```
De:<depth>;Pr:<probability>
{node#final id}:{node#1 name}={choice}=>{node#2 name}...{node#final name}
FinalV:[<vars>]
```
