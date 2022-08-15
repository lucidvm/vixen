using System;
using MsgPack;
using MsgPack.Serialization;

namespace LucidVM.LEC
{
    public static class LECSerdes
    {
        private static readonly MessagePackSerializer Serial = MessagePackSerializer.Get<MessagePackObject[]>();

        public static byte[] Pack(params MessagePackObject[] args)
        {
            return Serial.PackSingleObject(args);
        }

        public static MessagePackObject[] Unpack(byte[] data)
        {
            MessagePackObject obj = Unpacking.UnpackObject(data).Value;
            if (!obj.IsArray)
            {
                throw new ArgumentException("cant unpack: root object is not array");
            }
            return obj.ToObject() as MessagePackObject[];
        }
    }
}
