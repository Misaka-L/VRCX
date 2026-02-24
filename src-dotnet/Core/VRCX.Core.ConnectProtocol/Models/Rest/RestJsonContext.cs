using System.Text.Json.Serialization;

namespace VRCX.Core.ConnectProtocol.Models.Rest;

[JsonSerializable(typeof(ProblemDetails))]
internal sealed partial class RestJsonContext : JsonSerializerContext;