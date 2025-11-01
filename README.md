# BiliInteractiveVideoResolver

Recursively resolves all parts of Bilibili interactive videos from an AID/BVID.

## Input Format

Please choose one of the following two formats:

1. Pass all AIDs/BVIDs as process parameters.
2. Pass all AIDs/BVIDs as standard process input, one per line.

## Output Format

### Standard Error

- Prompt message, `AID/BVID:`.
- Complete error message.

### Standard Output

- Parsing results, each line in the format `<edge_id>:<cid>:<title>`, outputting a blank line after each input ID has been completely resolved.
- Single-line error message, starting with `!`.
