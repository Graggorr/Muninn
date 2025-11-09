namespace Muninn;

internal record RequestBody(byte[] Value, TimeSpan LifeTime, string EncodingName);