using Chadder.Client.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Chadder.Droid;

namespace Chadder
{
    public partial class ChadderUIHelper
    {
        public async Task DeleteMessage(ChadderMessage msg, ChadderConversation conversation)
        {
            //++await Source.DeleteMessage(msg, conversation);
            await Source.DeleteAllMessages();
        }
        public async Task TakeBack(ChadderMessage msg, ChadderConversation conversation)
        {
            ChadderApp.UIHelper.ShowLoading();
            var result = await Source.TakeMessageBack(msg, conversation);
            ChadderApp.UIHelper.HideLoading();
            ShowErrorIfNotOk(result);
        }
        public async Task TakeAllBack(ChadderConversation conversation)
        { }
        public async Task DeleteAllMessages(ChadderConversation conversation)
        {
            ChadderApp.UIHelper.ShowLoading();
            //++await Source.DeleteAllMessages(conversation);
            await Source.DeleteAllMessages();
            ChadderApp.UIHelper.HideLoading();
        }
        public Task SetHidden(ChadderConversation conversation)
        {
            return Source.SetHidden(conversation);
        }
    }
}