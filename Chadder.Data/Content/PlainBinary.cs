using Chadder.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data
{
    public interface IBinaryContent
    {
        Task<byte[]> GetBinContent(IContentContext context);
    }
    public class PlainBinary : Chadder.Data.Base.Content, IBinaryContent
    {
        public override int Type { get { return (int)CONTENT_TYPE.PLAIN_BINARY; } }

        public byte[] BinData { get; set; }

        public PlainBinary(byte[] data = null)
        {
            this.BinData = data;
        }

        public override void Read(Util.DataDeserializer reader)
        {
            base.Read(reader);
            BinData = reader.ReadBin();
        }

        public override void Serialize(Util.DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteBin(BinData);
        }

        public async Task<byte[]> GetBinContent(IContentContext context)
        {
            return BinData;
        }
    }
}
