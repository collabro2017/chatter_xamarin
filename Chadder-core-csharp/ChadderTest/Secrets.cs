using ChadderLib.DataModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChadderTest
{
    [TestFixture]
    class Secrets : BaseTest
    {
        [Test]
        public async void Create()
        {
            var random = new Random();
            var username = "autotest" + random.Next().ToString();
            var password = random.Next().ToString("x8");
            var source1 = await CreateAccount(username, password);
            var source2 = await CreateAccount();
            var source3 = CreateSource();
            await source3.Login(username, password, null);
            await source2.ChangeShareName(true);
            await source1.addContact(source2.database.profile.myId);
            await source2.ChangeShareName(false);
            await source1.pairDevice(source1.database.getDevice(source3.database.profile.myDeviceId));

            var mySecret = new byte[100];
            random.NextBytes(mySecret);
            var secret1 = await source1.CreateSecret(mySecret, new ChadderContact[] { source1.database.getContact(source2.database.profile.myId) });
            Ok(secret1.Item1);
            Assert.NotNull(secret1.Item2);
            var secret2 = await source2.GetSecret(secret1.Item2.Id);
            var secret3 = await source3.GetSecret(secret1.Item2.Id);
            Ok(secret2.Item1);
            Ok(secret3.Item1);
            Assert.NotNull(secret2.Item2);
            Assert.NotNull(secret3.Item2);
            Assert.AreEqual(secret1.Item2.Secret, mySecret);
            Assert.AreEqual(secret2.Item2.Secret, mySecret);
            Assert.AreEqual(secret3.Item2.Secret, mySecret);
        }
    }
}
