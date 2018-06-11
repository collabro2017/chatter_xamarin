using Chadder.Data.Base;
using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Keys
{
    public class PublicKeyBook : Content
    {
        public class PublicKeyBookRecord
        {
            public KeyPurpose Purpose { get; set; }
            public ECDSAPublicKey PublicKey { get; set; }
        };
        public PublicKeyBook()
        {
            Book = new List<PublicKeyBookRecord>();
            Version = 1;
        }
        public override int Type { get { return (int)CONTENT_TYPE.KEY_PUBLICK_BOOK; } }
        public int Version { get; set; }
        public byte[] Signature { get; set; }
        public List<PublicKeyBookRecord> Book { get; private set; }
        public override void Serialize(Util.DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteShort((short)Book.Count);
            WriteRecords(writer);
            writer.WriteBin(Signature);
        }

        public override void Read(Util.DataDeserializer reader)
        {
            base.Read(reader);
            var size = reader.ReadShort();
            Book.Capacity = size;
            for (short i = 0; i < size; ++i)
            {
                var r = new PublicKeyBookRecord();
                r.Purpose = (KeyPurpose)reader.ReadShort();
                r.PublicKey = reader.ReadContent<ECDSAPublicKey>();
                Book.Add(r);
            }
            Signature = reader.ReadBin();
        }

        public T GetValidKey<T>(KeyPurpose purpose) where T : Content
        {
            foreach (var r in Book)
            {
                if (r.Purpose == purpose && r.PublicKey is T)
                    return r.PublicKey as T;
            }
            return null;
        }

        public void WriteRecords(DataSerializer writer)
        {
            foreach (var r in Book)
            {
                writer.WriteShort((short)r.Purpose);
                writer.WriteContent(r.PublicKey);
            }
        }

        public async Task<byte[]> GetFullFingerprint()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new DataSerializer(stream);
                WriteRecords(writer);
                return CryptoHelper.Sha256(stream.ToArray());
            }
        }

        public async Task<bool> Validate()
        {
            var master = GetMaster();
            if (master == null)
                return false;

            if (await master.Verify(await GetFullFingerprint(), Signature) == false)
                return false;

            return true;
        }

        public ECDSAPublicKey GetMaster()
        {
            return GetValidKey<ECDSAPublicKey>(KeyPurpose.MASTER_KEY);
        }
    }
}
