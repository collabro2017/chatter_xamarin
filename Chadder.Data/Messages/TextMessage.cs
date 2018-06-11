using Chadder.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Messages
{
    public class TextMessage : BasicMessage
    {
        public override int Type { get { return (int)CONTENT_TYPE.MSG_TEXT; } }

        public string Body { get; set; }

        public override void Serialize(Util.DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteString(Body);
        }

        public override void Read(Util.DataDeserializer reader)
        {
            base.Read(reader);
            Body = reader.ReadString();
        }
    }
}
