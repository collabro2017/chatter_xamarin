using Chadder.Data.Base;
using Chadder.Data.Keys;
using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Asymmetric
{
    public abstract class ECDH : Content, IBinaryContent, IIdentifySource
    {
        public string SourceId { get; protected set; }
        public string TargetId { get; protected set; }

        public override void Read(DataDeserializer reader)
        {
            base.Read(reader);
            SourceId = reader.ReadShortString();
            TargetId = reader.ReadShortString();
        }

        public override void Serialize(DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteShortString(SourceId);
            writer.WriteShortString(TargetId);
        }

        public abstract Task<byte[]> GetBinContent(IContentContext context);
    }

    public abstract class ECDHEphemeral : Content, IBinaryContent
    {
        public string TargetId { get; protected set; }
        public ECDSAPublicKey PublicKey { get; set; }
        public override void Read(DataDeserializer reader)
        {
            base.Read(reader);
            TargetId = reader.ReadShortString();
            PublicKey = reader.ReadContent<ECDSAPublicKey>();
        }

        public override void Serialize(DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteShortString(TargetId);
            writer.WriteContent(PublicKey);
        }
        public abstract Task<byte[]> GetBinContent(IContentContext context);
    }
}
