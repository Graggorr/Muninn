using System.Text.Json.Serialization;

namespace Muninn;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(RequestBody))]
[JsonSerializable(typeof(ResponseBody))]
internal partial class MuninnJsonSerializerContext : JsonSerializerContext;
