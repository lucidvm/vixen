using System;
using MsgPack;

namespace LucidVM.LEC
{
    public static class LECNormalize
    {
        public static bool Check<T>(MessagePackObject obj)
        {
            bool? i = obj.IsTypeOf<T>();
            return i.HasValue && i.Value;
        }

        public static string EnsureOpcode(MessagePackObject obj, string[] codebook)
        {
            // if string already, just return string
            if (Check<string>(obj))
            {
                return obj.AsString().ToLower();
            }

            // consult protocol codebook
            if (Check<int>(obj))
            {
                int i = obj.AsInt32();
                if (i >= codebook.Length)
                {
                    throw new ArgumentException("could not cast to opcode: int beyond the end of the codebook");
                }
                return codebook[i];
            }

            throw new ArgumentException("could not cast to opcode: bad type");
        }

        public static string EnsureString(MessagePackObject obj)
        {
            if (Check<string>(obj))
            {
                return obj.AsString();
            }

            return obj.ToString();
        }

        public static int EnsureNumber(MessagePackObject obj)
        {
            if (Check<int>(obj)) return obj.AsInt32();
            if (Check<bool>(obj)) return obj.AsBoolean() ? 1 : 0;
            if (Check<string>(obj)) return int.Parse(obj.AsString());
            throw new ArgumentException("could not cast to number: bad type");
        }

        public static bool EnsureBoolean(MessagePackObject obj)
        {
            if (Check<bool>(obj))
            {
                return obj.AsBoolean();
            }

            int i;
            if (Check<string>(obj))
            {
                i = int.Parse(obj.AsString());
            }
            else if (Check<int>(obj))
            {
                i = obj.AsInt32();
            }
            else
            {
                throw new ArgumentException("could not cast to boolean: bad type");
            }
            if (i == 0) return false;
            if (i == 1) return true;

            throw new ArgumentException("could not cast to boolean: int out of range 0-1");
        }

        public static byte[] EnsureBuffer(MessagePackObject obj)
        {
            if (Check<byte[]>(obj))
            {
                return obj.ToObject() as byte[];
            }

            if (Check<string>(obj))
            {
                return Convert.FromBase64String(obj.AsString());
            }

            throw new ArgumentException("could not cast to buffer: bad type");
        }
    }
}
