using ChadderCDM;
using ChadderLib.DataSource;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChadderTest
{
    [TestFixture]
    class Contact : BaseTest
    {
        [Test]
        public async void SearchContact()
        {
            var source1 = await CreateAccount();
            var source2 = await CreateAccount();
            await source2.ChangeShareName(true);

            var result = await source1.findContact(source2.currentAccount.username);
            var found = false;
            foreach (var contact in result)
            {
                if(contact.id == source2.database.profile.myId)
                {
                    found = true;
                    break;
                }
            }
            Assert.True(found);
        }
        [Test]
        public async void AddContact()
        {
            var source1 = await CreateAccount();
            var source2 = await CreateAccount();
            await source2.ChangeShareName(true);

            var result = await source1.addContact(source2.database.profile.myId);
            Ok(result.Item1);
            var contact = source2.database.getContact(source1.database.profile.myId);
            Assert.NotNull(contact);
            var conversation = source2.database.GetConversationByContact(contact);
            Assert.NotNull(conversation);
        }

        [Test]
        public async void Groups()
        {
            var n = 3;
            var sources = await CreateAccountsAndAdd(n);

            var result = await sources[0].CreateGroup("Test");
            var s1group = result.Item2;
            Assert.AreEqual(ChadderError.OK, result.Item1);
            Assert.NotNull(s1group);

            for (var i = 1; i < n; ++i)
            {
                var user = sources[0].database.getContact(sources[i].database.profile.myId);
                Assert.NotNull(user);
                Assert.AreEqual(ChadderError.OK, await sources[0].AddUserToGroup(s1group, user));
            }

            Assert.AreEqual(n, s1group.Members.Count);
            await sources[0].GenerateNewSecretForGroup(s1group);

            ChadderDataSource.InsightTrack("Sending messages");

            foreach (var source in sources)
            {
                var group = source.database.GetGroup(s1group.GroupId);
                Assert.NotNull(group);
                var conversation = await source.database.GetConversationByGroup(group);
                Assert.NotNull(conversation);
                await source.SendNewMessage(conversation, source.database.profile.myDeviceId);
            }
            foreach (var source in sources)
                while (source.HasPendingMessages())
                    await Task.Delay(100);
            await Task.Delay(1000);

            foreach (var source in sources)
            {
                var group = source.database.GetGroup(s1group.GroupId);
                var conversation = await source.database.GetConversationByGroup(group);
                await source.requestUpdates();
                Assert.AreEqual(n, conversation.messages.Count);
            }
        }


    }
}
