using System.Text;

namespace Muninn
{
    public interface IBinarySerializable<out TSelf>
    {
        public byte[] Serialize(Encoding encoding);

        public TSelf? Deserialize(byte[] encodedValue, Encoding encoding);
    }
}
