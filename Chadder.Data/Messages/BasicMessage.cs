using Chadder.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Messages
{
    public abstract class BasicMessage : Content
    {
        public string Id { get; set; }
        public string Group { get; set; } // Not used for now
        public DateTime TimeSent { get; set; }
        public DateTime Expiration { get; set; }

        public BasicMessage() { }

        public override void Serialize(Util.DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteShortString(Id);
            writer.WriteShortString(Group);
            writer.WriteDate(TimeSent);
            writer.WriteDate(Expiration);
        }

        public override void Read(Util.DataDeserializer reader)
        {
            base.Read(reader);
            Id = reader.ReadShortString();
            Group = reader.ReadShortString();
            TimeSent = reader.ReadDate();
            Expiration = reader.ReadDate();
        }
    }
}
