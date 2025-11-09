using Google.Protobuf;

namespace Muninn.Grpc.Contracts.Extensions;

public static class ByteExtensions
{
    public static ByteString ToByteString(this byte[] bytes) => ByteString.CopyFrom(bytes);
}
