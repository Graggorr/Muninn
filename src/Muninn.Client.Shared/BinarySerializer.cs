using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;

namespace Muninn
{
    internal static class BinarySerializer
    {
        public static byte[] Serialize<T>(T obj, Encoding encoding)
        {
            using var stream = GetStream<T>();
            using var writer = new BinaryWriter(stream, encoding, leaveOpen: true);
            WriteObject(obj, writer, encoding);

            return stream.ToArray();
        }

        public static T? Deserialize<T>(Stream stream, Encoding encoding)
        {
            using var reader = new BinaryReader(stream, encoding, leaveOpen: true);
            
            return (T?)ReadObject(typeof(T), reader, encoding);
        }
    
        public static T? Deserialize<T>(byte[] array, Encoding encoding)
        {
            using var stream = GetStream<T>(array);
            return Deserialize<T>(stream, encoding);
        }

        private static void WriteObject(object? obj, BinaryWriter writer, Encoding encoding)
        {
            if (obj == null)
            {
                writer.Write((byte)0);
                return;
            }

            writer.Write((byte)1);
            var type = obj.GetType();

            if (type == typeof(int))
            {
                writer.Write((int)obj);
            }
            else if (type == typeof(long))
            {
                writer.Write((long)obj);
            }
            else if (type == typeof(bool))
            {
                writer.Write((bool)obj);
            }
            else if (type == typeof(double))
            {
                writer.Write((double)obj);
            }
            else if (type == typeof(float))
            {
                writer.Write((float)obj);
            }
            else if (type == typeof(string))
            {
                var str = (string)obj;
                var bytes = encoding.GetBytes(str);
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
            else if (type.IsEnum)
            {
                writer.Write(Convert.ToInt32(obj));
            }
            else if (type.IsArray)
            {
                var array = (Array)obj;
                writer.Write(array.Length);

                foreach (var element in array)
                {
                    WriteObject(element, writer, encoding);
                }
            }
            else if (type.IsClass || type.IsValueType)
            {
                var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(memberInfo => memberInfo is PropertyInfo or FieldInfo);
            
                foreach (var memberInfo in members)
                {
                    var value = memberInfo switch
                    {
                        PropertyInfo propertyInfo => propertyInfo.CanRead ? propertyInfo.GetValue(obj) : null,
                        FieldInfo fieldInfo => fieldInfo.GetValue(obj),
                        _ => null
                    };

                    WriteObject(value, writer, encoding);
                }
            }
            else
            {
                throw new NotSupportedException($"Unsupported type {type}");
            }
        }

        private static object? ReadObject(Type type, BinaryReader reader, Encoding encoding)
        {
            var hasValue = reader.ReadByte();

            if (hasValue == 0)
            {
                return null;
            }

            if (type == typeof(int))
            {
                return reader.ReadInt32();
            }

            if (type == typeof(long))
            {
                return reader.ReadInt64();
            }

            if (type == typeof(bool))
            {
                return reader.ReadBoolean();
            }

            if (type == typeof(double))
            {
                return reader.ReadDouble();
            }

            if (type == typeof(float))
            {
                return reader.ReadSingle();
            }

            if (type == typeof(string))
            {
                var length = reader.ReadInt32();
                var bytes = reader.ReadBytes(length);
                return encoding.GetString(bytes);
            }

            if (type.IsEnum)
            {
                var enumValue = reader.ReadInt32();
                return Enum.ToObject(type, enumValue);
            }

            if (type.IsArray)
            {
                var length = reader.ReadInt32();
                var elementType = type.GetElementType();
                
                if (elementType != null)
                {
                    var array = Array.CreateInstance(elementType, length);
            
                    for (var i = 0; i < length; i++)
                    {
                        array.SetValue(ReadObject(elementType, reader, encoding), i);
                    }

                    return array;
                }
            }

            if (type.IsClass || type.IsValueType)
            {
                var obj = Activator.CreateInstance(type);
                var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(memberInfo => memberInfo is PropertyInfo or FieldInfo);
            
                foreach (var memberInfo in members)
                {
                    switch (memberInfo)
                    {
                        case PropertyInfo propertyInfo when propertyInfo.CanWrite:
                            propertyInfo.SetValue(obj, ReadObject(propertyInfo.PropertyType, reader, encoding));
                            break;
                        case FieldInfo fieldInfo:
                            fieldInfo.SetValue(obj, ReadObject(fieldInfo.FieldType, reader, encoding));
                            break;
                    }
                }

                return obj;
            }

            throw new NotSupportedException($"Unsupported type {type}");
        }

        private static MemoryStream GetStream<T>(byte[]? array = null)
        {
            if (array is null)
            {
                var size = Marshal.SizeOf<T>();
                array = new byte[size + 1];
            }
        
            return new MemoryStream(array, 0, array.Length, true);
        }
    }
}
