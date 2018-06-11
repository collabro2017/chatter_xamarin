using Chadder.Client.Source;
using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Chadder.Droid;

namespace Chadder
{
    public partial class ChadderUIHelper
    {
        public ChadderSource Source { get; set; }
        public ChadderUIHelper()
        {
            Source = new ChadderSource();
        }

        public bool ShowErrorIfNotOk(ChadderError result)
        {
            if (result == ChadderError.OK)
                return true;
            ChadderApp.UIHelper.ShowError(result);
            return false;
        }

        public void OnIdle()
        {
            Source.Connection.Disconnect(false);
        }

        public async void BackFromIdle()
        {
            if (Source.Connection.ShouldBeConnected)
            {
                var result = await Source.Connection.Connect();
                if (result)
                {
                    await Source.RequestUpdates();
                    Source.StartSendPendingMessages();
                }
            }
            Source.RecoverFromIdle();
        }

        public async void OnConnectionStateChanged(bool hasConnection)
        {
            if (hasConnection)
            {
                if (Source.Connection.ShouldBeConnected)
                {
                    var result = await Source.Connection.Connect();
                    if (result)
                    {
                        await Source.RequestUpdates();
                        Source.StartSendPendingMessages();
                    }
                }
            }
            else
                Source.Connection.Disconnect(false);
        }

        public async void SetNotificationHandle(string handle)
        {
            await Source.UpdateNotificationHandleParameter(handle);
        }
    }
}