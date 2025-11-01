using BiliInteractiveVideoResolver.API;
using System.Text.Json.Serialization;

namespace BiliInteractiveVideoResolver;

[JsonSerializable(typeof(XWebInterfaceViewDetail.Root), TypeInfoPropertyName = $"{nameof(XWebInterfaceViewDetail)}_{nameof(XWebInterfaceViewDetail.Root)}")]
[JsonSerializable(typeof(XWebInterfaceViewDetail.Data), TypeInfoPropertyName = $"{nameof(XWebInterfaceViewDetail)}_{nameof(XWebInterfaceViewDetail.Data)}")]
[JsonSerializable(typeof(XPlayerV2.Root), TypeInfoPropertyName = $"{nameof(XPlayerV2)}_{nameof(XPlayerV2.Root)}")]
[JsonSerializable(typeof(XPlayerV2.Data), TypeInfoPropertyName = $"{nameof(XPlayerV2)}_{nameof(XPlayerV2.Data)}")]
[JsonSerializable(typeof(XSteinEdgeinfoV2.Root), TypeInfoPropertyName = $"{nameof(XSteinEdgeinfoV2)}_{nameof(XSteinEdgeinfoV2.Root)}")]
[JsonSerializable(typeof(XSteinEdgeinfoV2.Data), TypeInfoPropertyName = $"{nameof(XSteinEdgeinfoV2)}_{nameof(XSteinEdgeinfoV2.Data)}")]
internal partial class AppJsonSerializerContext : JsonSerializerContext;