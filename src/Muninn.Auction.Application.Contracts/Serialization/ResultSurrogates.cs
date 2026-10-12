using FluentResults;

namespace Muninn.Auction.Application.Contracts.Serialization;

[GenerateSerializer]
public struct ResultSurrogate
{
    [Id(0)] public string[] Errors;
}

[GenerateSerializer]
public struct ResultSurrogate<TValue>
{
    [Id(0)] public string[] Errors;

    [Id(1)] public TValue? Value;
}

[RegisterConverter]
public sealed class ResultConverter : IConverter<Result, ResultSurrogate>
{
    public Result ConvertFromSurrogate(in ResultSurrogate surrogate)
        => surrogate.Errors is { Length: > 0 } errors ? Result.Fail(errors) : Result.Ok();

    public ResultSurrogate ConvertToSurrogate(in Result value)
        => new() { Errors = value.Errors.Select(error => error.Message).ToArray() };
}

[RegisterConverter]
public sealed class ResultConverter<TValue> : IConverter<Result<TValue>, ResultSurrogate<TValue>>
{
    public Result<TValue> ConvertFromSurrogate(in ResultSurrogate<TValue> surrogate)
        => surrogate.Errors is { Length: > 0 } errors ? Result.Fail<TValue>(errors) : Result.Ok(surrogate.Value!);

    public ResultSurrogate<TValue> ConvertToSurrogate(in Result<TValue> value)
        => new()
        {
            Errors = value.Errors.Select(error => error.Message).ToArray(),
            Value = value.IsSuccess ? value.Value : default,
        };
}
