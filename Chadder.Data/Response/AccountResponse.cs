using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Response
{
    public class DeviceInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public byte[] PublicKey { get; set; }
        public bool HasKey { get; set; }

        public string userId { get; set; }
    }
    public class ProfileInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public bool IsNamePublic { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneVerified { get; set; }
        public byte[] PublicKey { get; set; }
        public string PictureId { get; set; }
    }
}
