using Chadder.Data.Base;
using Chadder.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Cmd
{
    public class PairDeviceContent : Content
    {
        public override int Type { get { return (int)CONTENT_TYPE.CMD_PAIR_DEVICE; } }

        public PrivateKeyBook Book { get; set; }

        public override void Serialize(Util.DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteContent(Book);
        }

        public override void Read(Util.DataDeserializer reader)
        {
            base.Read(reader);
            Book = reader.ReadContent<PrivateKeyBook>();
        }
    }
}
