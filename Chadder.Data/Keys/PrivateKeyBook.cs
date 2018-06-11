using Chadder.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Keys
{
    public class PrivateKeyBook : Content
    {
        public class PrivateKeyBookRecord
        {
            public KeyPurpose Purpose { get; set; }
            public ECDSAKeyPair PrivateKey { get; set; }
        };
        public override int Type { get { return (int)CONTENT_TYPE.KEY_PRIVATE_BOOK; } }
        public int Version { get; set; }

        public PrivateKeyBook()
        {
            Book = new List<PrivateKeyBookRecord>();
            Version = 1;
        }
        public List<PrivateKeyBookRecord> Book { get; private set; }

        public override void Serialize(Util.DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteShort((short)Book.Count);
            foreach (var r in Book)
            {
                writer.WriteShort((short)r.Purpose);
                writer.WriteContent(r.PrivateKey);
            }
        }

        public override void Read(Util.DataDeserializer reader)
        {
            base.Read(reader);
            var size = reader.ReadShort();
            Book.Capacity = size;
            for (short i = 0; i < size; ++i)
            {
                var r = new PrivateKeyBookRecord();
                r.Purpose = (KeyPurpose)reader.ReadShort();
                r.PrivateKey = reader.ReadContent<ECDSAKeyPair>();
                Book.Add(r);
            }
        }

        public T GetValidKey<T>(KeyPurpose purpose) where T : Content
        {
            foreach (var r in Book)
            {
                if (r.Purpose == purpose && r.PrivateKey is T)
                    return r.PrivateKey as T;
            }
            return null;
        }

        public async Task<PublicKeyBook> GetPublicBook()
        {
            var result = new PublicKeyBook();
            foreach(var r in Book) {
                result.Book.Add(new PublicKeyBook.PublicKeyBookRecord() {
                    Purpose = r.Purpose,
                    PublicKey = r.PrivateKey.Public
                });
            }
            var master = GetMaster();
            result.Signature = await master.Sign(await result.GetFullFingerprint());
            return result;
        }

        public ECDSAKeyPair GetMaster()
        {
            return GetValidKey<ECDSAKeyPair>(KeyPurpose.MASTER_KEY);
        }

        public bool IsEqual(PrivateKeyBook book)
        {
            if (Version != book.Version)
                return false;
            if (Book.Count != book.Book.Count)
                return false;
            for (int i = 0; i < Book.Count; ++i)
            {
                if (Book[i].Purpose != book.Book[i].Purpose)
                    return false;
                if (Book[i].PrivateKey.Serialize().SequenceEqual(book.Book[i].PrivateKey.Serialize()) == false)
                    return false;
            }
            return true;
        }
    }
}
