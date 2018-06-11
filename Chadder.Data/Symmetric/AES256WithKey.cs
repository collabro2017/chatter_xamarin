using Chadder.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Symmetric
{
    public class AES256WithKey : AES256, IContentWrapper
    {
        public override int Type { get { return (int)CONTENT_TYPE.SYM_AES_WITH_KEY; } }
        public IBinaryContent Key { get; protected set; }
        public AES256WithKey() { }
        static public async Task<AES256WithKey> Encrypt(IContentContext context, IBinaryContent keyData, Content content)
        {
            var result = new AES256WithKey()
            {
                Key = keyData
            };
            await result.Create(await keyData.GetBinContent(context), content.Serialize());
            return result;
        }

        public override void Read(Util.DataDeserializer reader)
        {
            base.Read(reader);
            Key = Deserialize(reader) as IBinaryContent;
        }

        public override void Serialize(Util.DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteContent(Key as Content); 
        }

        public override Task<byte[]> GetKey(IContentContext context)
        {
            return Key.GetBinContent(context);
        }

        public override async Task<T> Find<T>(IContentContext context)
        {
            if (Key is T)
                return Key as T;
            return await base.Find<T>(context);
        }
    }
}
