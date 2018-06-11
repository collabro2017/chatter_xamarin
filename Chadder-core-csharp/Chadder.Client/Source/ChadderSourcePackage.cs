using Chadder.Client.Data;
using Chadder.Data;
using Chadder.Data.Asymmetric;
using Chadder.Data.Base;
using Chadder.Data.Cmd;
using Chadder.Data.Messages;
using Chadder.Data.Response;
using Chadder.Data.Util;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Client.Source
{
    public partial class ChadderSource
    {
        public virtual async Task ProcessUserPublicKey(UserPublicKey content)
        {
            var contact = db.GetContact(content.UserId);
            if (contact == null)
            {
                if (content.PublicKeyData != null)
                {
                    contact = new ChadderContact()
                    {
                        Name = "Anonymous",
                        PictureId = null,
                        Picture = db.GetPicture(null),
                        PublicKeyBookData = content.PublicKeyData,
                        UserId = content.UserId,
                        Type = RelationshipType.FRIENDS,
                    };
                    await db.AddContact(contact, false);
                }
            }
            else
            {
                if (content.PublicKeyData == null) // Delete Friend
                {
                    await db.DeleteContact(contact);
                }
                else
                {
                    contact.UpdatePublicKey(content.PublicKeyData);
                    await sqlDB.UpdateAsync(contact);
                }
            }
        }
        public virtual async Task ProcessTakeBack(TakeMessageBackContent content, string fromId, string toId)
        {
            ChadderConversation conversation = null;
            if (fromId == db.LocalUser.UserId)
                conversation = db.GetContactConversation(toId);
            else
                conversation = db.GetContactConversation(fromId);
            if (content.MessageIds.Count == 0)
            {
                foreach (var msg in conversation.Messages)
                {
                    if (msg.UserId == fromId)
                        await db.DeleteMessage(msg, conversation);
                }
            }
            else
            {
                foreach (var id in content.MessageIds)
                {
                    var msg = conversation.Messages.FirstOrDefault(i => i.MessageId == id);
                    if (msg != null)
                    {
                        if (msg.Sender.UserId == fromId)
                            await db.DeleteMessage(msg, conversation);
                        else
                            Insight.Track(string.Format("User {0} unauthorized take back", fromId));
                    }
                }
            }
        }
        public virtual async Task<bool> ProcessPackage(Package package)
        {
            var content = Content.Deserialize(package.Data);
            var identifier = await content.Find<ECDH>(this);
            string fromId = null;
            string toId = null;
            if (identifier != null)
            {
                fromId = identifier.SourceId;
                toId = identifier.TargetId;
            }
            if (content == null)
                return false;
            // If multiple layer of encryption were to be supported there should be a loop around this section
            if (content is IContentWrapper) // Decrypt section
            {
                content = await (content as IContentWrapper).GetContent(this);
            }

            // This has to be the clear content
            if (content is UserPublicKey)
            {
                if (package.OwnerId != null)
                {
                    Insight.Track("Non-server trying to update client id");
                }
                else
                    await ProcessUserPublicKey(content as UserPublicKey);
            }
            else if (content is DevicePublicKey)
            {
                if (package.OwnerId != null)
                {
                    Insight.Track("Non-server trying to update device id");
                    return true;
                }
                var device = db.GetDevice((content as DevicePublicKey).DeviceId);
                var pbk = (content as DevicePublicKey).PublicKeyData;
                if (device == null)
                {
                    if (pbk != null)
                    {
                        device = new ChadderUserDevice()
                        {
                            DeviceId = (content as DevicePublicKey).DeviceId,
                            PublicKeyBookData = pbk,
                            Name = "Temp",
                            HasUserKey = false,
                            CurrentDevice = false
                        };
                        await db.AddDevice(device);
                    }
                }
                else
                {
                    if (pbk == null)
                    {
                        db.LocalUser.Devices.Remove(device);
                        await sqlDB.DeleteAsync(device);
                    }
                    else
                    {
                        device.PublicKeyBookData = pbk;
                        await sqlDB.UpdateAsync(device);
                    }
                }
            }
            else if (content is PairDeviceContent)
            {
                if (package.OwnerId != db.LocalUser.UserId)
                {
                    Insight.Track("Not this user trying to pair device");
                    return true;
                }
                db.LocalUser.PrivateKeyBookData = (content as PairDeviceContent).Book.Serialize();
                await sqlDB.UpdateAsync(db.LocalUser);
                return true;
            }
            else if (content is TakeMessageBackContent)
                await ProcessTakeBack(content as TakeMessageBackContent, fromId, toId);
            else if (content is BasicMessage) // Process section
            {
                var msg = content as BasicMessage;
                ChadderConversation conversation = null;
                if (fromId == db.LocalUser.UserId)
                    conversation = db.GetContactConversation(toId);
                else
                    conversation = db.GetContactConversation(package.OwnerId);
                if (conversation.Messages.FirstOrDefault(i => i.MessageId == msg.Id) == null)
                {
                    var record = new ChadderMessage()
                    {
                        MessageId = msg.Id,
                        Type = ChadderMessage.MESSAGE_TYPE.TEXT,
                        Status = ChadderMessage.MESSAGE_STATUS.SENT,
                        Expiration = msg.Expiration,
                        TimeSent = msg.TimeSent,
                        TimeReceived = DateTime.UtcNow,
                        TimeServer = package.Time,
                        MyMessage = false,
                        ConversationId = conversation.recordId,
                        Sender = conversation.Contact,
                    };

                    if (conversation.ContactUserId != fromId)
                    {
                        record.MyMessage = true;
                        record.Sender = db.LocalUser;
                    }
                    record.UserId = record.Sender.UserId;

                    if (content is TextMessage)
                        record.Body = (content as TextMessage).Body;

                    await db.AddMessage(record, conversation);
                }
                else
                    Insight.Track("Repeated Message GUID");
            }
            else
                return false;
            return true;
        }
        public async Task<ChadderError> GetPackages()
        {
            var result = await AuthorizedRequest<BasicArrayResponse<Package>>(Connection.AccountHub, "GetPackages", db.LocalUser.LastPackage);
            if (result.Error == ChadderError.OK)
            {
                bool noException = true; // if there is only 1 package and it crashes it will end up on infinite loop.
                foreach (var p in result.List)
                {
                    try
                    {
                        if (await ProcessPackage(p))
                            db.LocalUser.LastPackage = p.Id;
                        else
                            noException = false;
                    }
                    catch (Exception ex)
                    {
                        noException = false;
                        Insight.Report(ex);
                    }
                }
                if (result.List.Count > 0 && noException)
                {
                    await sqlDB.UpdateAsync(db.LocalUser);
                    return await GetPackages();
                }
            }
            return result.Error;
        }

    }
}
