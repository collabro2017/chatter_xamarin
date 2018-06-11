using Chadder.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data
{
    public class DevicePublicKey : Content
    {
        public override int Type { get { return (int)CONTENT_TYPE.PLAIN_DEVICE_KEY; } }

        public string DeviceId { get; set; }
        public byte[] PublicKeyData { get; set; }

        public override void Serialize(Util.DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteShortString(DeviceId);
            writer.WriteBin(PublicKeyData);
        }
        public override void Read(Util.DataDeserializer reader)
        {
            base.Read(reader);
            DeviceId = reader.ReadShortString();
            PublicKeyData = reader.ReadBin();
        }
    }
}
