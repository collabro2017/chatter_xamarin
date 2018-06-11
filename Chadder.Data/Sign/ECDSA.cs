using Chadder.Data.Base;
using Chadder.Data.Keys;
using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Sign
{
    public class ECDSA : Content
    {
        public override int Type { get { return (int)CONTENT_TYPE.SIGN_ECDSA; } }
        protected Content content { get; set; }
        public byte[] Signature { get; private set; }

        protected ECDSA(Content content, byte[] signature)
        {
            this.content = content;
            this.Signature = signature;
        }

        static public async Task<ECDSA> Sign(Content content, ECDSAKeyPair key)
        {
            return new ECDSA(content, await key.Sign(content.Serialize()));
        }

        public ECDSA() { }

        public override void Read(DataDeserializer reader)
        {
            base.Read(reader);
            content = Deserialize(reader) as Content;

            Signature = reader.ReadBin();
        }

        public Task<bool> Validate(ECDSAPublicKey key)
        {
            return key.Verify(content.Serialize(), Signature);
        }

        public override void Serialize(DataSerializer writer)
        {
            base.Serialize(writer);
            content.Serialize(writer);
            writer.WriteBin(Signature);
        }

        public override async Task<T> Find<T>(IContentContext context)
        {
            var a = await base.Find<T>(context);
            if (a == null)
                return await content.Find<T>(context);
            return a;
        }
    }
}
