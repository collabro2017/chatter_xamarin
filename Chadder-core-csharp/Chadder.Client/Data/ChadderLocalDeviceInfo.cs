using Chadder.Client.Util;
using Chadder.Data.Keys;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chadder.Client.Data
{
    public class ChadderLocalDeviceInfo : SQLSingleton
    {
        public string DeviceId { get; set; }
        public string NotificationHandle { get; set; }
        public byte[] PrivateKeyBookData { get; set; }


        [Ignore]
        public PrivateKeyBook PrivateKeyBook
        {
            get { return Chadder.Data.Base.Content.Deserialize<PrivateKeyBook>(PrivateKeyBookData); }
        }
    }
}
