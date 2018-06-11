using Chadder.Client.Data;
using Chadder.Data.Asymmetric;
using Chadder.Data.Base;
using Chadder.Data.Messages;
using Chadder.Data.Request;
using Chadder.Data.Response;
using Chadder.Data.Symmetric;
using Chadder.Data.Util;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Chadder.Data.Cmd;
using Chadder.Data;
using System.Collections.Concurrent;
using Chadder.Client.Util;

namespace Chadder.Client.Source
{
    public partial class ChadderSource
    {
        protected ConcurrentQueue<ChadderMessage> PendingMessages = new ConcurrentQueue<ChadderMessage>();
        protected Task SendPendingMessagesTask;
        protected async Task SendPendingMessages()
        {
            ChadderMessage msg = null;
            try
            {
                var backoff = new ExponentialBackoff(1000, 5000);
                while (PendingMessages.Count > 0)
                {
                    if (PendingMessages.TryPeek(out msg) && msg.Status != ChadderMessage.MESSAGE_STATUS.SENT)
                    {
                        var conversation = db.GetConversation(msg.ConversationId);
                        if (await SendMessageToServer(msg, conversation) == ChadderError.OK)
                        {
                            while (PendingMessages.TryDequeue(out msg)) ;
                            backoff.Reset();
                        }
                        else
                        {
                            await backoff.Failed();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Insight.Track("SendPendingMessages safe try-catch");
                Insight.Report(ex);
            }
            SendPendingMessagesTask = null;
        }
        public void StartSendPendingMessages()
        {
            if (SendPendingMessagesTask == null || SendPendingMessagesTask.Status != TaskStatus.Running)
            {
                SendPendingMessagesTask = SendPendingMessages();
            }
            else
                Insight.Track("SendPendingMessagesTask not null");
        }
        public void AddPendingMessage(ChadderMessage msg, bool start = true)
        {
            PendingMessages.Enqueue(msg);
            if (start)
                StartSendPendingMessages();
        }

        public async Task<Content> EncryptForUser(Content content, ChadderContact contact)
        {
            var key = new ECDHUser(db.LocalUser.UserId, contact.UserId);
            return await AES256WithKey.Encrypt(this, key, content);
        }
        public async Task<ChadderMessage> SendMessage(string txt, ChadderConversation conversation)
        {
            var msg = ChadderMessage.Create(conversation, db.LocalUser, ChadderMessage.MESSAGE_TYPE.TEXT);
            msg.Body = txt;

            await db.AddMessage(msg, conversation);
            AddPendingMessage(msg);
            return msg;
        }
        public async void SendPicture(byte[] data, ChadderConversation conversation)
        {
            var record = new ChadderSQLPicture()
            {
                ToBeUploaded = true,
                Bin = data,
                PictureId = Guid.NewGuid().ToString()// Temporary
            };

            await sqlDB.InsertAsync(record);
            var picture = await db.LoadPicture(record, true);

            var msg = ChadderMessage.Create(conversation, db.LocalUser, ChadderMessage.MESSAGE_TYPE.PICTURE);
            msg.PictureId = record.PictureId;
            msg.Picture = picture;

            await db.AddMessage(msg, conversation);
            AddPendingMessage(msg);
        }

        protected async Task<ChadderError> SendMessageToServer(ChadderMessage msg, ChadderConversation conversation)
        {
            try
            {
                BasicMessage package = null;
                if (msg.Type == ChadderMessage.MESSAGE_TYPE.TEXT)
                {
                    package = new TextMessage()
                    {
                        Body = msg.Body
                    };
                }
                else if (msg.Type == ChadderMessage.MESSAGE_TYPE.PICTURE)
                {
                    if (msg.Picture.ToBeUploaded)
                    {
                        var record = await sqlDB.GetPicture(msg.Picture.RecordId);
                        var presult = await UploadPicture(record);
                        if (presult.Error == ChadderError.OK)
                        {
                            msg.PictureId = record.PictureId;
                            await sqlDB.UpdateAsync(msg);
                        }
                        else
                            return presult.Error;
                    }
                    package = new ImageMessage()
                    {
                        PictureId = msg.PictureId
                    };
                }
                else
                {
                    Insight.Track("Invalid Message Type in SendMessageToServer");
                    return ChadderError.INVALID_INPUT;
                }
                package.Id = msg.MessageId;
                package.Group = null;
                package.TimeSent = msg.TimeSent.Value;
                package.Expiration = msg.Expiration.Value;

                var content = await EncryptForUser(package, conversation.Contact);
                var request = new SendPackageParameter()
                {
                    UserId = conversation.ContactUserId,
                    Data = content.Serialize()
                };
                var result = await AuthorizedRequest<BasicResponse<string>>(Connection.ChatHub, "SendPackageToUser", request);
                if (result.Error == ChadderError.OK)
                {
                    msg.Status = ChadderMessage.MESSAGE_STATUS.SENT;
                    msg.TimeServer = result.Time;
                    msg.ReferenceId = result.Extra;
                    await sqlDB.UpdateAsync(msg);
                }
                return result.Error;
            }
            catch (Exception ex)
            {
                Insight.Report(ex);
                return ChadderError.GENERAL_EXCEPTION;
            }
        }

        public async Task DeleteAllMessages()
        {
            foreach (var conversation in db.Conversations)
            {
                await db.DeleteAllMessages(conversation);
            }
        }

        public async Task SetHidden(ChadderConversation conversation)
        {
            try
            {
                conversation.Hidden = true;
                await sqlDB.UpdateAsync(conversation);
            }
            catch (Exception ex)
            {
                Insight.Report(ex);
            }
        }

        public async Task<ChadderError> TakeMessageBack(ChadderMessage msg, ChadderConversation conversation)
        {
            var package = new TakeMessageBackContent(msg.MessageId);
            var content = await EncryptForUser(package, conversation.Contact);
            var result = await AuthorizedRequest<BasicResponse>(Connection.ChatHub, "TakeMessageBack", msg.ReferenceId);
            if (result.Error == ChadderError.OK)
            {
                var request = new SendPackageParameter()
                {
                    UserId = conversation.ContactUserId,
                    Data = content.Serialize()
                };
                result = await AuthorizedRequest<BasicResponse<string>>(Connection.ChatHub, "SendPackageToUser", request);
                if (result.Error == ChadderError.OK)
                {
                    await db.DeleteMessage(msg, conversation);
                }
            }
            return result.Error;
        }
        public async Task<ChadderError> TakeAllMessagesBack(ChadderConversation conversation)
        {
            var package = new TakeMessageBackContent();
            var content = await EncryptForUser(package, conversation.Contact);
            var request = new SendPackageParameter()
            {
                UserId = conversation.ContactUserId,
                Data = content.Serialize()
            };
            var result = await AuthorizedRequest<BasicResponse<string>>(Connection.ChatHub, "SendPackageToUser", request);
            if (result.Error == ChadderError.OK)
            {
                await db.DeleteAllMessages(conversation);
            }
            return result.Error;
        }
    }
}
