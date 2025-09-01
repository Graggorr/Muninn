using Google.Protobuf;

namespace Muninn.Api.Grpc.Extensions;

public static class ByteExtensions
{
    public static ByteString ToByteString(this byte[] bytes) => ByteString.CopyFrom(bytes);
}
