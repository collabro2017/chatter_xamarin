using Chadder.Client.Data;
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
        public async Task<bool> AddContact(string userId)
        {
            ChadderApp.UIHelper.ShowLoading();
            var result = await Source.AddContact(userId);
            ChadderApp.UIHelper.HideLoading();
            if (result.Error == ChadderError.OK)
                return true;
            ChadderApp.UIHelper.ShowError(result.Error);
            return false;
        }

        public bool SendMessage(string txt, ChadderConversation conversation)
        {
            txt = txt.Trim();

            if (string.IsNullOrWhiteSpace(txt))
            {
                ChadderApp.UIHelper.ShowError("Can't send an empty message!");
                return false;
            }
            Source.SendMessage(txt, conversation);
            return true;
        }

        public bool SendPicture(byte[] data, ChadderConversation conversation)
        {
            Source.SendPicture(data, conversation);
            return true;
        }
        public async void ToggleBlock(ChadderContact contact)
        {
            ChadderApp.UIHelper.ShowLoading();
            ChadderError result = ChadderError.OK;
            if (contact.Type == Data.RelationshipType.FRIENDS)
                result = await Source.SetFriendType(contact, Data.RelationshipType.BLOCKED);
            else
                result = await Source.SetFriendType(contact, Data.RelationshipType.FRIENDS);
            ChadderApp.UIHelper.HideLoading();
            ShowErrorIfNotOk(result);
        }

        public async void ReportContact(ChadderContact contact)
        {

        }
    }
}