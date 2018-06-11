using Chadder.Client.SQL;
using Chadder.Client.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Chadder.Client.Data
{
    public partial class ChadderDatabase
    {
        public Chadder.Client.Source.ChadderSource Source;
        public ChadderDatabase(Chadder.Client.Source.ChadderSource s) { Source = s; }
        public ChadderSQLUserDB sqlDB { get { return Source.sqlDB; } }
        public ChadderLocalDeviceInfo LocalDevice { get; set; }
        public ChadderLocalUserInfo LocalUser { get; set; }

        public ChadderObservableCollection<ChadderContact> Contacts = new ChadderObservableCollection<ChadderContact>();
        public ChadderObservableCollection<ChadderConversation> Conversations = new ChadderObservableCollection<ChadderConversation>();

        public async Task Load()
        {
            LocalUser = await sqlDB.Table<ChadderLocalUserInfo>().FirstOrDefaultAsync();

            Contacts.AddItems(await sqlDB.GetItems<ChadderContact>());

            var conversations = await sqlDB.GetItems<ChadderConversation>();
            foreach (var c in conversations)
            {
                c.Contact = GetContact(c.ContactUserId);
            }
            Conversations.AddItems(conversations);

            foreach (var msg in await sqlDB.GetItems<ChadderMessage>())
            {
                if (msg.MyMessage)
                    msg.Sender = LocalUser;
                else
                    msg.Sender = GetContact(msg.UserId);

                Conversations.FirstOrDefault(i => i.recordId == msg.ConversationId).Messages.Add(msg);

                if (msg.Status != ChadderMessage.MESSAGE_STATUS.SENT)
                    Source.AddPendingMessage(msg, false);
            }

            foreach (var picture in await sqlDB.GetItems<ChadderSQLPicture>())
            {
                await LoadPicture(picture, false);
            }

            LocalUser.Picture = GetPicture(LocalUser.PictureId);

            foreach (var contact in Contacts)
            {
                contact.Picture = GetPicture(contact.PictureId);
            }

            foreach (var c in Conversations)
            {
                foreach (var m in c.Messages)
                {
                    if (m.PictureId != null)
                    {
                        m.Picture = GetPicture(m.PictureId);
                    }
                }
            }

            foreach (var d in await sqlDB.GetItems<ChadderUserDevice>())
            {
                LocalUser.Devices.Add(d);
            }
        }

        public ChadderUserDevice GetDevice(string id)
        {
            var t = LocalUser.Devices.FirstOrDefault(i => i.DeviceId == id);
            return t;
        }
        public async Task AddDevice(ChadderUserDevice device)
        {
            LocalUser.Devices.Add(device);
            await sqlDB.InsertAsync(device);
        }
        public async Task AddContact(ChadderContact contact, bool Hidden)
        {
            Contacts.Add(contact);
            await sqlDB.InsertAsync(contact);

            var conversation = new ChadderConversation()
            {
                ContactUserId = contact.UserId,
                Contact = contact,
                Hidden = Hidden,
            };
            await sqlDB.InsertAsync(conversation);
            Conversations.Add(conversation);
        }
        public async Task DeleteContact(ChadderContact contact)
        {
            var conversation = GetContactConversation(contact.UserId);
            await DeleteAllMessages(conversation);
            Conversations.Remove(conversation);
            await sqlDB.DeleteAsync(conversation);
            Contacts.Remove(contact);
            await sqlDB.DeleteAsync(contact);
        }
        public async Task DeleteAllMessages(ChadderConversation conversation)
        {
            while (conversation.Messages.Count > 0)
                await DeleteMessage(conversation.Messages.Last(), conversation);
        }
        public async Task DeletePicture(ChadderPicture picture)
        {
            var record = await sqlDB.GetPicture(picture.RecordId);
            await sqlDB.DeleteAsync(record);
            Pictures.Remove(picture);
        }
        public async Task DeleteMessage(ChadderMessage msg, ChadderConversation conversation)
        {
            if (msg.Type == ChadderMessage.MESSAGE_TYPE.PICTURE)
            {
                await DeletePicture(msg.Picture);
            }
            await sqlDB.DeleteAsync(msg);
            conversation.Messages.Remove(msg);
        }
        public async Task AddMessage(ChadderMessage msg, ChadderConversation conversation)
        {
            conversation.Messages.Add(msg);
            await sqlDB.InsertAsync(msg);

            conversation.Hidden = false;
            await sqlDB.UpdateAsync(conversation);
        }
        public ChadderContact GetContact(string userId)
        {
            return Contacts.FirstOrDefault(i => i.UserId == userId);
        }

        public ChadderConversation GetContactConversation(string userId)
        {
            return Conversations.FirstOrDefault(i => i.ContactUserId == userId);
        }
        public ChadderConversation GetConversation(int id)
        {
            return Conversations.FirstOrDefault(i => i.recordId == id);
        }
    }
}
