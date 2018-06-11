using Chadder.Client.Util;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chadder.Client.Source
{
    public class ChadderConnection : SignalRConnection
    {
        public ChadderConnection(string d) : base(d) { }

        public delegate void VoidDelegate();
        public event VoidDelegate OnRequestUpdate;

        protected override void CreateHubConnection()
        {
            base.CreateHubConnection();

            ChatHub = HubConnection.CreateHubProxy("ChatHub");
            AccountHub = HubConnection.CreateHubProxy("AccountHub");
            ContactHub = HubConnection.CreateHubProxy("ContactHub");
            ChatHub.On("update", () =>
            {
                if (OnRequestUpdate != null)
                    OnRequestUpdate();
            });
        }



        public IHubProxy ChatHub;
        public IHubProxy AccountHub;
        public IHubProxy ContactHub;

        // Legacy, keep here until completing new system
        public IHubProxy MessagesHub;
    }
}
