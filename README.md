# LibBiliInteractiveVideo

Recursively resolves all parts of Bilibili interactive videos from an AID/BVID.

> [!NOTE]
>
> While this library is compatible with NativeAOT, enabling NativeAOT is not recommended for optimal performance. Expression trees may run in interpreted mode under NativeAOT.

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

Please choose one of the following two formats:

1. Pass all IDs as process parameters.
2. Pass all IDs as standard process input, one ID per line.

Each ID should be in the format `{AID/BVID}`, `{AID/BVID},{trace}`, or `{AID/BVID},{trace},{mode}`.

If the `trace` parameter is provided, only the path of the traced NodeID/CID will be printed verbatim; other information will not be printed.

`mode` can be either `NodeId` or `Cid`, case-insensitive.

### Output Format

#### Trace ID is not set

```
ProcessId:{id};Trace:{trace};Mode:{mode}
GraphVersion:{graph version};Total:{number of NodeIDs/CIDs}
```

Then the following is repeated for each NodeID/CID:

```
{id}:{node name (only in NodeId mode)}
De:{depth};Pr:{probability}
<{node#final id}<{node#final-1 id}<...<{node#2 id}<{node#1 id}
```

#### Trace ID set

```
ProcessId:{id};Trace:{trace};Mode:{mode}`
GraphVersion:{graph version};Total:{number of NodeIDs/CIDs}`
{id}:{node name (only in NodeId mode)}
De:{depth};Pr:{probability}
```

Then the following is repeated for each NodeID in the path to traced NodeID/CID:

```
==>{edge.Option (if exists)}
  C: {condition (if exists)}
  N: {native action (if exists)}
{node id}:{node name}
Va:[{variables}]
```
