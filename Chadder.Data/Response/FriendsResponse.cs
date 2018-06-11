using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Response
{
    public class ContactInfo
    {
        public string Id { get; set; }
        public string ProfilePicture { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public RelationshipType RelaType { get; set; }
        public byte[] PublicKey { get; set; }
    }
    public class Package
    {
        public long Id { get; set; }
        public DateTime Time { get; set; }
        public string OwnerId { get; set; }
        public byte[] Data { get; set; }
    }
}
