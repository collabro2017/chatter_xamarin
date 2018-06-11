using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Messages
{
    public class ImageMessage : BasicMessage
    {
        public override int Type { get { return (int)CONTENT_TYPE.MSG_IMAGE; } }
        public string PictureId { get; set; }
        public byte[] Key { get; set; }
        public override void Serialize(Util.DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteShortString(PictureId);
            writer.WriteBin(Key);
        }

        public override void Read(Util.DataDeserializer reader)
        {
            base.Read(reader);
            PictureId = reader.ReadShortString();
            Key = reader.ReadBin();
        }
    }
}
