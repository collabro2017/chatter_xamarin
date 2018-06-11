using Chadder.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Cmd
{
    public class TakeMessageBackContent : Content
    {
        public override int Type { get { return (int)CONTENT_TYPE.CMD_TAKE_BACK; } }

        public string GroupId { get; set; }
        public List<string> MessageIds = new List<string>();

        public TakeMessageBackContent() { }
        public TakeMessageBackContent(string id)
        {
            MessageIds.Add(id);
        }

        public override void Serialize(Util.DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteShortString(GroupId);
            writer.WriteShort((short)MessageIds.Count);
            foreach (var m in MessageIds)
            {
                writer.WriteShortString(m);
            }
        }

        public override void Read(Util.DataDeserializer reader)
        {
            base.Read(reader);
            GroupId = reader.ReadShortString();
            var s = reader.ReadShort();
            for (short i = 0; i < s; ++i)
            {
                MessageIds.Add(reader.ReadShortString());
            }
        }
    }
}
