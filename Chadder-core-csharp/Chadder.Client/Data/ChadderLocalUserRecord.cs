using Chadder.Client.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chadder.Client.Data
{
    public class ChadderLocalUserRecord : SQLRecordBase
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public byte[] DatabaseKey { get; set; }
        public byte[] RefreshToken { get; set; }
    }
}
