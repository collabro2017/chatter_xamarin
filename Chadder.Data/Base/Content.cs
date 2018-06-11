using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Base
{
    public abstract class Content : IContentSerializable
    {
        public enum CONTENT_TYPE : int
        {
            CT_RESERVED = 0,
            RESERVED_KEY = 0x00010000, KEY_ECDSA_PUBLIC, KEY_ECDSA_PRIVATE, KEY_PUBLICK_BOOK, KEY_PRIVATE_BOOK,
            RESERVED_PLAIN = 0x00020000, PLAIN_BINARY, PLAIN_USER_KEY, PLAIN_DEVICE_KEY, PLAIN_IMAGE,
            RESERVED_SYMMETRIC = 0x00030000, SYM_AES_WITHOUT_KEY, SYM_AES_WITH_KEY,
            RESERVED_AGREEMENT = 0x00040000, ECDH_USER, ECDH_DEVICE, ECDH_DEVICE_EPHEMERAL,
            RESERVED_SIGNATURE = 0x00050000, SIGN_ECDSA,
            RESERVED_MESSAGE = 0x00060000, MSG_TEXT, MSG_IMAGE,
            RESERVED_COMMAND = 0x00070000, CMD_TAKE_BACK, CMD_PAIR_DEVICE,
        };
        public abstract int Type { get; }
        private static List<IContentFactory> Factories;
        static Content()
        {
            Factories = new List<IContentFactory>();
            AddFactory(new BasicContentFactory());
        }
        public static void AddFactory(IContentFactory factory)
        {
            Factories.Add(factory);
        }

        public static Content Deserialize(DataDeserializer reader)
        {
            var type = reader.PeekInt();
            foreach (var factory in Factories)
            {
                var content = factory.Create(type);
                if (content != null)
                {
                    content.Read(reader);
                    return content;
                }
            }
            return null;
        }

        public static Content DeserializeBase64(string str)
        {
            return Deserialize(new DataDeserializer(Convert.FromBase64String(str)));
        }
        public static Content Deserialize(byte[] data)
        {
            return Deserialize(new DataDeserializer(data));
        }
        public static T Deserialize<T>(byte[] data) where T : class
        {
            return Deserialize(new DataDeserializer(data)) as T;
        }

        public string SerializeBase64()
        {
            return Convert.ToBase64String(Serialize());
        }
        public virtual void Serialize(DataSerializer writer)
        {
            writer.WriteInt((int)Type);
        }

        public virtual void Read(DataDeserializer reader)
        {
            var type = reader.ReadInt();
            if (type != Type)
                throw new Exception("Invalid Content Type");
        }

        public static byte[] Serialize(IContentSerializable content)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                var writer = new DataSerializer(stream);
                content.Serialize(writer);
                return stream.ToArray();
            }
        }
        public virtual byte[] Serialize()
        {
            using (var stream = new System.IO.MemoryStream())
            {
                Serialize(new DataSerializer(stream));
                return stream.ToArray();
            }
        }

        public virtual async Task<T> Find<T>(IContentContext context) where T : class
        {
            if (this is T)
            {
                return this as T;
            }
            return null;
        }
    }
}
