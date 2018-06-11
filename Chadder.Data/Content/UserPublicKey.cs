using Chadder.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data
{
    public class UserPublicKey : Content
    {
        public override int Type { get { return (int)CONTENT_TYPE.PLAIN_USER_KEY; } }

        public string UserId { get; set; }
        public byte[] PublicKeyData { get; set; }

        public override void Serialize(Util.DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteShortString(UserId);
            writer.WriteBin(PublicKeyData);
        }
        public override void Read(Util.DataDeserializer reader)
        {
            base.Read(reader);
            UserId = reader.ReadShortString();
            PublicKeyData = reader.ReadBin();
        }
    }
}
