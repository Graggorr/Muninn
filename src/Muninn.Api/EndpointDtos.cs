using Microsoft.AspNetCore.Mvc;

namespace Muninn.Api;

// Post
public record PostRequest([FromRoute] string Key, [FromBody] PostRequestBody Body);

public record PostRequestBody(string Value, TimeSpan LifeTime, string EncodingName);

// Get

public record GetRequest([FromRoute] string Key);

// Put

public record PutRequest([FromRoute] string Key, [FromBody] PutRequestBody Body);

public record PutRequestBody(string Value, TimeSpan LifeTime, string EncodingName);

// Insert

public record InsertRequest([FromRoute] string Key, [FromBody] InsertRequestBody Body);

public record InsertRequestBody(string Value, TimeSpan LifeTime, string EncodingName);

// Delete 

public record DeleteRequest([FromRoute] string Key);

public record Response(string Value);
