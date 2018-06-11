using ChadderLib.Crypto;
using ChadderLib.DataModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChadderTest
{
    [TestFixture]
    class Messages : BaseTest
    {
        [Test]
        public async void SendMessage()
        {
            var source1 = await CreateAccount();
            var source2 = await CreateAccount();
            await source2.ChangeShareName(true);

            var result = await source1.addContact(source2.database.profile.myId);
            await source2.ChangeShareName(false);
            Ok(result.Item1);

            var contact1 = source1.database.getContact(source2.database.profile.myId);
            Assert.NotNull(contact1);
            var conversation1 = await source1.database.GetConversationByContact(contact1);
            Assert.NotNull(conversation1);

            var contact2 = source2.database.getContact(source1.database.profile.myId);
            Assert.NotNull(contact2);
            var conversation2 = await source2.database.GetConversationByContact(contact2);
            Assert.NotNull(conversation2);

            var body = "Testing";
            await source2.SendNewMessage(conversation2, body);
            await source1.WaitForUpdate();
            var msg1 = conversation1.lastMessage;
            var msg2 = conversation2.lastMessage;
            Assert.AreEqual(body, msg1.Body);
            Assert.AreEqual(body, msg2.Body);
        }

        [Test]
        public async void NewMessageProtocolTransition()
        {
            var source1 = await CreateAccount();
            var source2 = await CreateAccount();
            await source2.ChangeShareName(true);

            var result = await source1.addContact(source2.database.profile.myId);
            await source2.ChangeShareName(false);
            Ok(result.Item1);

            var contact1 = source1.database.getContact(source2.database.profile.myId);
            Assert.NotNull(contact1);
            source1.database.conversations.Clear();
            var conversation1 = await source1.database.GetConversationByContact(contact1);
            Assert.Null(conversation1);

            var contact2 = source2.database.getContact(source1.database.profile.myId);
            Assert.NotNull(contact2);
            var conversation2 = await source2.database.GetConversationByContact(contact2);
            Assert.NotNull(conversation2);

            var body = "Testing";
            var msg = new ChadderMessage()
            {
                Type = MESSAGE_TYPE.TEXT,
                messageId = Guid.NewGuid().ToString(),
                myMessage = true,
                Body = "Test",
                fromId = source1.database.profile.myId,
                LocalTime = DateTime.UtcNow,
                Status = MESSAGE_STATUS.SENDING
            };
            var packed = new ChadderPackedMessageText(msg);
            var key = new ChadderECDHUserUserLegacy(source1.database, contact1);
            var encrypted = await ChadderAESEncryptedContent.Encrypt(source1, key, packed);
            await source2.SendNewMessage(conversation2, body);

            await Task.Delay(1000);
            Assert.AreEqual(2, conversation2.messages.Count);
        }
    }
}
