using Chadder.Client.Data;
using Chadder.Client.SQL;
using Chadder.Client.Util;
using Chadder.Data.Base;
using Chadder.Data.Keys;
using Chadder.Data.Response;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Client.Source
{
    public partial class ChadderSource : IContentContext
    {
        public bool IsOnline { get; private set; }
        protected ChadderUrls Urls { get; set; }
        protected ChadderRequest Session { get; set; }
        public ChadderDatabase db { get; protected set; }
        public ChadderSQLUserDB sqlDB { get; protected set; }
        public ChadderConnection Connection { get; protected set; }
        public string NotificationHandle { get; set; }
        public IECDSAKeyFactory KeyFactory;

        public ChadderSource(int instanceId = 0)
        {
            InstanceId = instanceId;
            Urls = new ChadderUrls();
            db = new ChadderDatabase(this);
            Session = new ChadderRequest();
            KeyFactory = new ECDSASimpleFactory();
            Connection = new ChadderConnection(Urls.Domain);
            Connection.OnRequestUpdate += () => UIThread.Run(async () =>
            {
                await RequestUpdates();
            });
        }

        protected virtual Task<T> AuthorizedRequest<T>(Microsoft.AspNet.SignalR.Client.IHubProxy proxy, string method, params object[] data) where T : BasicResponse, new()
        {
            return Connection.Invoke<T>(proxy, method, data);
        }

        // HELP FUNCTIONS

        public delegate void VoidDelegate();
        public event VoidDelegate OnRequestUpdates;
        public async Task RequestUpdates()
        {
            await GetPackages();
            await UpdateContacts();
            await GetMyInfo();
            if (OnRequestUpdates != null)
                OnRequestUpdates();
        }
        public void RecoverFromIdle()
        {
            SendPendingMessagesTask = null;
            StartSendPendingMessages();
        }
        public bool HasUserKey { get { return db.LocalUser.HasUserKey; } }

        public Task<IIdentity> GetUser(string id)
        {
            return Task.FromResult<IIdentity>(db.GetContact(id));
        }
        public Task<IIdentity> GetDevice(string id)
        {
            return Task.FromResult<IIdentity>(db.GetDevice(id));
        }
        public Task<PrivateKeyBook> GetUserKeyPair()
        {
            return Task.FromResult(db.LocalUser.PrivateKeyBook);
        }
        public Task<PrivateKeyBook> GetDeviceKeyPair()
        {
            return Task.FromResult(db.LocalDevice.PrivateKeyBook);
        }
        public Task<string> GetMyUserId()
        {
            return Task.FromResult(db.LocalUser.UserId);
        }
        public Task<string> GetMyDeviceId()
        {
            return Task.FromResult(db.LocalDevice.DeviceId);
        }
    }
}
