using Microsoft.AspNetCore.Mvc;

namespace Muninn.Api;

// Post
public sealed record PostRequest([FromRoute] string Key, [FromBody] PostRequestBody Body);

public sealed record PostRequestBody(string Value, TimeSpan LifeTime, string EncodingName);

// Get

public sealed record GetRequest([FromRoute] string Key);

// Put

public sealed record PutRequest([FromRoute] string Key, [FromBody] PutRequestBody Body);

public sealed record PutRequestBody(string Value, TimeSpan LifeTime, string EncodingName);

// Insert

public sealed record InsertRequest([FromRoute] string Key, [FromBody] InsertRequestBody Body);

public sealed record InsertRequestBody(string Value, TimeSpan LifeTime, string EncodingName);

// Delete 

public sealed record DeleteRequest([FromRoute] string Key);

public sealed record Response(string EncodingName, byte[] EncodedValue);
