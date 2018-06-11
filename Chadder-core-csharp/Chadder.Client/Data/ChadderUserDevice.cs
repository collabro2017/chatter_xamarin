using Chadder.Client.Util;
using Chadder.Data.Base;
using Chadder.Data.Keys;
using Chadder.Data.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chadder.Client.Data
{
    public class ChadderUserDevice : SQLRecordBase, IIdentity
    {
        public string DeviceId { get; set; }
        private string _name;
        public string Name { get { return _name; } set { SetField(ref _name, value); } }
        public bool HasUserKey { get; set; }
        public bool CurrentDevice { get; set; }
        public byte[] PublicKeyBookData { get; set; }
        public Chadder.Data.Keys.PublicKeyBook PublicKeyBook { get { return Content.Deserialize<PublicKeyBook>(PublicKeyBookData); } }

        public ChadderUserDevice()
        {

        }

        public ChadderUserDevice(DeviceInfo info)
        {
            DeviceId = info.Id;
            Update(info);
        }

        public void Update(DeviceInfo info)
        {
            Name = info.Name;
            HasUserKey = info.HasKey;
            PublicKeyBookData = info.PublicKey;
        }
    }
}
