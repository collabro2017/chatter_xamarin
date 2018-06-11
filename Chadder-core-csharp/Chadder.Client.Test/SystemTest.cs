using Chadder.Client.Data;
using Chadder.Client.Source;
using Chadder.Client.SQL;
using Chadder.Client.Util;
using Chadder.Data;
using Chadder.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Chadder.Client.Test
{
    [TestClass]
    public class SystemTest
    {
        public static async Task WaitUntilCondition(int max, string msg, Func<bool> compare)
        {
            int c = 0;
            int interval = 200;
            while (compare() == false)
            {
                if (c > max)
                    Assert.Fail("AwaitForRequest waited too long");
                await Task.Delay(interval);
                c += interval;
            }
        }
        public class AwaitNotAuthorized : IDisposable
        {
            public int Updated { get; set; }
            public int MaxTime { get; set; }
            public ChadderSource Source;
            public AwaitNotAuthorized(ChadderSource s, int time = 12000)
            {
                s.OnNotAuthorized += source_OnNotAuthorized;
                Source = s;
                MaxTime = time;
            }
            private void source_OnNotAuthorized()
            {
                Updated++;
            }

            public void Dispose()
            {
                WaitUntilCondition(MaxTime, "AwaitNotAuthorized waited too long", () => Updated > 0).Wait();
                Source.OnNotAuthorized -= source_OnNotAuthorized;
                if (Updated > 1)
                    Assert.Fail("OnNotAuthorized called {0} times", Updated);
            }
        }
        public class AwaitUpdate : IDisposable
        {
            public int Updated { get; set; }
            public int MaxTime { get; set; }
            public ChadderSource Source;
            public AwaitUpdate(ChadderSource s, int time = 2000)
            {
                s.OnRequestUpdates += source_OnRequestUpdates;
                Source = s;
                MaxTime = time;
            }
            private void source_OnRequestUpdates()
            {
                Updated++;
            }

            public void Dispose()
            {
                WaitUntilCondition(MaxTime, "AwaitUpdate waited too long", () => Updated > 0).Wait();
                Source.OnRequestUpdates -= source_OnRequestUpdates;
                if (Updated > 1)
                    Assert.Fail("OnRequestUpdates called {0} times", Updated);
            }
        }
        /// <summary>
        /// This tests the following:
        /// Create One User1 Source1 I0 (Instance 0)
        /// Login from I1 User1 Source2
        /// Test Logout (Logout Source1, check source2 devices, Login source1)
        /// Test DeleteDevice (Delete S1 from S2, wait for OnNotAuthorized, Login S1)
        /// Create new key with Source2
        /// Pair Source1
        /// Create Second User2 Source3 I0
        /// Find User1
        /// Add User1
        /// Send message from Source1 and receive on all three devices
        /// Change name
        /// Delete Contact
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task BasicSystemTest()
        {
            var random = new Random();
            var source1 = CreateSource(0);
            var username = Guid.NewGuid().ToString("N").Substring(0, 20);
            var name = string.Format("AutoTest{0}", random.Next());
            Assert.AreEqual(ChadderError.OK, await source1.CreateUser(name, username, username));

            // Login with user on a second device
            var source2 = CreateSource(1);
            using (var await1 = new AwaitUpdate(source1))
                Assert.AreEqual(ChadderError.OK, await source2.Login(username, username));
            Assert.AreEqual(source1.db.LocalUser.UserId, source2.db.LocalUser.UserId);
            Assert.AreEqual(2, source2.db.LocalUser.Devices.Count);
            Assert.AreEqual(2, source1.db.LocalUser.Devices.Count);
            Assert.AreEqual(ChadderError.OK, await source2.CreateNewKey());

            // Test Logout (Logout Source1, check source2 devices, Login source1)
            using (var await2 = new AwaitUpdate(source2))
                Assert.AreEqual(ChadderError.OK, await source1.Logout());
            Assert.AreEqual(1, source2.db.LocalUser.Devices.Count);
            using (var await2 = new AwaitUpdate(source2))
                Assert.AreEqual(ChadderError.OK, await source1.Login(username, username));

            // Pair Previous Device
            var source2_device1 = source2.db.LocalUser.Devices.FirstOrDefault(i => i.DeviceId == source1.db.LocalDevice.DeviceId);
            using (var await1 = new AwaitUpdate(source1))
                Assert.AreEqual(ChadderError.OK, await source2.PairDevice(source2_device1));
            Assert.AreEqual(2, source1.db.LocalUser.Devices.Count);
            Assert.IsTrue(source1.db.LocalUser.PrivateKeyBookData.SequenceEqual(source2.db.LocalUser.PrivateKeyBookData));

            // Create a second user
            var source3 = CreateSource(0); // Other user will share same device with source1
            var name2 = string.Format("AutoTest{0}", random.Next());
            Assert.AreEqual(ChadderError.OK, await source3.CreateUser(name2));

            // Find first user
            var list = await source3.FindUser(name);
            Assert.AreEqual(ChadderError.OK, list.Error);
            Assert.IsNotNull(list.List.FirstOrDefault(i => i.UserId == source1.db.LocalUser.UserId));

            // Add second user
            using (var await2 = new AwaitUpdate(source2))
            using (var await3 = new AwaitUpdate(source3))
            {
                var result = await source1.AddContact(source3.db.LocalUser.UserId);
                Assert.AreEqual(ChadderError.OK, result.Error);
            }
            Assert.AreEqual(1, source3.db.Contacts.Count);
            Assert.AreEqual(1, source2.db.Contacts.Count);
            Assert.AreEqual(1, source1.db.Contacts.Count);

            ChadderMessage msg = null;
            // Send message From User1
            using (var await2 = new AwaitUpdate(source2))
            using (var await3 = new AwaitUpdate(source3))
            {
                msg = await source1.SendMessage("Testing", source1.db.Conversations[0]);
                await WaitUntilCondition(2000, "Too long to send a message", () => msg.Status == ChadderMessage.MESSAGE_STATUS.SENT);
            }
            // Check if all three devices have the same message
            Assert.AreEqual(1, source1.db.Conversations[0].Messages.Count);
            Assert.AreEqual(1, source2.db.Conversations[0].Messages.Count);
            Assert.AreEqual(1, source3.db.Conversations[0].Messages.Count);
            Assert.IsTrue(source1.db.Conversations[0].Messages[0].MyMessage);
            Assert.IsTrue(source2.db.Conversations[0].Messages[0].MyMessage);
            Assert.IsFalse(source3.db.Conversations[0].Messages[0].MyMessage);
            Assert.AreEqual(msg.Body, source2.db.Conversations[0].Messages[0].Body);
            Assert.AreEqual(msg.Body, source3.db.Conversations[0].Messages[0].Body);

            // Check TakeBack feature
            using (var await2 = new AwaitUpdate(source2))
            using (var await3 = new AwaitUpdate(source3))
                Assert.AreEqual(ChadderError.OK, await source1.TakeMessageBack(source1.db.Conversations[0].Messages[0], source1.db.Conversations[0]));
            Assert.AreEqual(0, source1.db.Conversations[0].Messages.Count);
            Assert.AreEqual(0, source2.db.Conversations[0].Messages.Count);
            Assert.AreEqual(0, source3.db.Conversations[0].Messages.Count);

            // Change name
            name = string.Format("AutoTest{0}", random.Next());
            using (var await2 = new AwaitUpdate(source2))
            using (var await3 = new AwaitUpdate(source3))
                Assert.AreEqual(ChadderError.OK, await source1.ChangeName(name));
            Assert.AreEqual(name, source1.db.LocalUser.Name);
            Assert.AreEqual(name, source2.db.LocalUser.Name);
            Assert.AreEqual(name, source3.db.Contacts[0].Name);

            // Change Picture, and download
            byte[] data = new byte[256 * 256];
            random.NextBytes(data);
            using (var await2 = new AwaitUpdate(source2))
            using (var await3 = new AwaitUpdate(source3))
                Assert.AreEqual(ChadderError.OK, await source1.ChangePicture(data));
            Assert.AreEqual(source1.db.LocalUser.PictureId, source2.db.LocalUser.PictureId);
            Assert.AreEqual(source1.db.LocalUser.PictureId, source3.db.Contacts[0].PictureId);
            await source2.db.LocalUser.Picture.LoadPictureAsync(false);
            await source3.db.Contacts[0].Picture.LoadPictureAsync(false);
            Assert.IsTrue(data.SequenceEqual((await source2.sqlDB.GetPicture(source2.db.LocalUser.Picture.RecordId)).Bin));
            Assert.IsTrue(data.SequenceEqual((await source3.sqlDB.GetPicture(source3.db.Contacts[0].Picture.RecordId)).Bin));

            {   // Test Delete Contact
                using (var await2 = new AwaitUpdate(source2))
                    Assert.AreEqual(ChadderError.OK, await source1.DeleteContact(source1.db.Contacts[0]));
                Assert.AreEqual(0, source1.db.Contacts.Count);
                Assert.AreEqual(0, source2.db.Contacts.Count);
                Assert.AreEqual(1, source3.db.Contacts.Count);

                // Add the user again after deleting
                list = await source1.FindUser(name2);
                Assert.AreEqual(ChadderError.OK, list.Error);
                Assert.IsNotNull(list.List.FirstOrDefault(i => i.UserId == source3.db.LocalUser.UserId));

                using (var await2 = new AwaitUpdate(source2))
                {
                    var result = await source1.AddContact(source3.db.LocalUser.UserId);
                    Assert.AreEqual(ChadderError.OK, result.Error);
                }
                Assert.AreEqual(1, source1.db.Contacts.Count);
                Assert.AreEqual(1, source2.db.Contacts.Count);
                Assert.AreEqual(1, source3.db.Contacts.Count);
            }

            {   // Test generate new keys
                Assert.AreEqual(PublicKeyStatus.NOT_VERIFIED, source3.db.Contacts[0].KeyStatus);
                using (var await3 = new AwaitUpdate(source3))
                    Assert.AreEqual(ChadderError.OK, await source1.RefreshKeys());
                Assert.AreEqual(PublicKeyStatus.NOT_VERIFIED, source3.db.Contacts[0].KeyStatus);
                using (var await3 = new AwaitUpdate(source3))
                    Assert.AreEqual(ChadderError.OK, await source1.CreateNewKey());
                Assert.AreEqual(PublicKeyStatus.CHANGED, source3.db.Contacts[0].KeyStatus);
            }
        }
        [TestMethod]
        public async Task DeleteDevice()
        {
            var random = new Random();
            var source1 = CreateSource(0);
            var source2 = CreateSource(1);
            var source3 = CreateSource(2);
            var username = Guid.NewGuid().ToString("N").Substring(0, 20);
            var name = string.Format("AutoTest{0}", random.Next());
            Assert.AreEqual(ChadderError.OK, await source1.CreateUser(name, username, username));
            using (var await1 = new AwaitUpdate(source1))
                Assert.AreEqual(ChadderError.OK, await source2.Login(username, username));
            using (var await1 = new AwaitUpdate(source1))
            using (var await2 = new AwaitUpdate(source2))
                Assert.AreEqual(ChadderError.OK, await source3.Login(username, username));
            Assert.AreEqual(3, source1.db.LocalUser.Devices.Count);
            Assert.AreEqual(3, source2.db.LocalUser.Devices.Count);
            Assert.AreEqual(3, source3.db.LocalUser.Devices.Count);

            // Test DeleteDevice (Delete S1(Online) from S2, wait for OnNotAuthorized, Login S1)
            var source2_device1 = source2.db.LocalUser.Devices.FirstOrDefault(i => i.DeviceId == source1.db.LocalDevice.DeviceId);
            using (var await1 = new AwaitNotAuthorized(source1))
            using (var await3 = new AwaitUpdate(source3))
                Assert.AreEqual(ChadderError.OK, await source2.DeleteDevice(source2_device1));
            Assert.AreEqual(2, source2.db.LocalUser.Devices.Count);
            Assert.AreEqual(2, source3.db.LocalUser.Devices.Count);

            using (var await2 = new AwaitUpdate(source2))
                Assert.AreEqual(ChadderError.OK, await source1.Login(username, username));
            source2_device1 = source2.db.LocalUser.Devices.FirstOrDefault(i => i.DeviceId == source1.db.LocalDevice.DeviceId);
            source1.Cleanup();

            Assert.AreEqual(ChadderError.OK, await source2.DeleteDevice(source2_device1));

            using (var await1 = new AwaitNotAuthorized(source1))
            {
                var defUser = await source1.GetDefaultUser();
                Assert.AreEqual(ChadderError.OK, await source1.StartUser(defUser));
                Assert.AreEqual(ChadderError.OK, await source1.LoadUser(null));
            }
        }

#if DEBUG
        [TestMethod]
        public async Task TestPasswordReset()
        {
            var random = new Random();
            var source1 = CreateSource(0);
            var username = Guid.NewGuid().ToString().Substring(0, 20);
            var name = string.Format("AutoTest{0}", random.Next());
            Assert.AreEqual(ChadderError.OK, await source1.CreateUser(name, username, username));

            // Change email
            var email = string.Format("{0}@asdasd.com", username);
            Assert.AreEqual(ChadderError.OK, await source1.ChangeEmail(email));
            await source1.Logout();
            var response = await source1.RequestPasswordResetDebug(email);
            Assert.AreEqual(ChadderError.OK, response.Error);
            response = await source1.PasswordResetDebug(response.Extra);
            Assert.AreEqual(ChadderError.OK, response.Error);
            Assert.AreEqual(ChadderError.OK, await source1.Login(username, response.Extra));

            // Now change email half way and see
            response = await source1.RequestPasswordResetDebug(email);
            Assert.AreEqual(ChadderError.OK, response.Error);
            email = string.Format("{0}@asdasd2.com", username);
            Assert.AreEqual(ChadderError.OK, await source1.ChangeEmail(email));
            response = await source1.PasswordResetDebug(response.Extra);
            Assert.AreEqual(ChadderError.INVALID_INPUT, response.Error);
        }
#endif

        [TestMethod]
        public async Task TestSplashScreen()
        {
            await ChadderSQLMainDB.clearData(0);
            var random = new Random();
            var source1 = CreateSource(0);
            var name = string.Format("AutoTest{0}", random.Next());
            Assert.AreEqual(ChadderError.OK, await source1.CreateUser(name));

            var source2 = CreateSource(0);
            var defUser = await source2.GetDefaultUser();
            Assert.IsNotNull(defUser);
            Assert.AreEqual(defUser.UserId, source1.db.LocalUser.UserId);
            Assert.AreEqual(ChadderError.OK, await source2.StartUser(defUser));
            Assert.AreEqual(ChadderError.OK, await source2.LoadUser(null));
        }

        [TestMethod]
        public async Task TestMirrorBot()
        {
            string BotName = "Chadder Mirror Bot";
            var random = new Random();
            var source1 = CreateSource(0);
            var username = Guid.NewGuid().ToString().Substring(0, 20);
            var name = string.Format("AutoTest{0}", random.Next());
            Assert.AreEqual(ChadderError.OK, await source1.CreateUser(name, username, username));

            var list = await source1.FindUser(BotName);
            Assert.AreEqual(ChadderError.OK, list.Error);
            Assert.AreEqual(1, list.List.Count);

            // Add second user
            var result = await source1.AddContact(list.List[0].UserId);
            Assert.AreEqual(ChadderError.OK, result.Error);

            // send message
            using (var await1 = new AwaitUpdate(source1, 4000))
            {
                var msg = await source1.SendMessage(random.Next().ToString(), source1.db.Conversations[0]);
                await WaitUntilCondition(2000, "Too long to send a message", () => msg.Status == ChadderMessage.MESSAGE_STATUS.SENT);
            }

            Assert.AreEqual(2, source1.db.Conversations[0].Messages.Count);
            Assert.AreEqual(source1.db.Conversations[0].Messages[0].Body, source1.db.Conversations[0].Messages[1].Body);
        }

        [TestMethod]
        public async Task CheckUsernameNameValid()
        {
            var source = CreateSource(0);
            var source2 = CreateSource(1);
            Assert.AreEqual(ChadderError.INVALID_NAME, await source.CreateUser("chadder"));
            Assert.AreEqual(ChadderError.INVALID_NAME, await source.CreateUser("etransfr"));
            Assert.AreEqual(ChadderError.INVALID_NAME, await source.CreateUser("test\n"));
            Assert.AreEqual(ChadderError.INVALID_NAME, await source.CreateUser("test\t"));
            Assert.AreEqual(ChadderError.INVALID_USERNAME, await source.CreateUser("Test", "chadder"));
            Assert.AreEqual(ChadderError.INVALID_USERNAME, await source.CreateUser("Test", "  test"));
            Assert.AreEqual(ChadderError.INVALID_USERNAME, await source.CreateUser("Test", "test&"));
            Assert.AreEqual(ChadderError.INVALID_USERNAME, await source.CreateUser("Test", "test   "));
            Assert.AreEqual(ChadderError.INVALID_USERNAME, await source.CreateUser("Test", "te"));
            Assert.AreEqual(ChadderError.INVALID_USERNAME, await source.CreateUser("Test", "te123ghjghjghjghjghjghjghj456789123456"));

            var pwd = Guid.NewGuid().ToString();
            Assert.AreEqual(ChadderError.OK, await source.CreateUser("Test Test123", "tEst123", pwd));
            Assert.AreEqual(ChadderError.USERNAME_ALREADY_IN_USE, await source2.CreateUser("Test", "Test123"));
            Assert.AreEqual(ChadderError.OK, await source2.Login("Test123", pwd));
        }
        // Common code to all Tests 
        Dictionary<int, List<ChadderSource>> SourceInstances = new Dictionary<int, List<ChadderSource>>();
        public ChadderSource CreateSource(int id)
        {
            var source = new ChadderSource(id);
            if (SourceInstances.ContainsKey(id) == false)
            {
                SourceInstances[id] = new List<ChadderSource>();
            }
            SourceInstances[id].Add(source);
            return source;
        }
        [TestCleanup]
        public void Cleanup()
        {
            List<string> deletedUsers = new List<string>();
            foreach (var pair in SourceInstances)
            {
                foreach (var source in pair.Value)
                {
                    if (source.IsOnline && deletedUsers.Contains(source.db.LocalUser.UserId) == false)
                    {
                        var userId = source.db.LocalUser.UserId;
                        Assert.AreEqual(ChadderError.OK, source.DeleteUser().Result);
                        deletedUsers.Add(userId);
                    }
                }
                var temp = pair.Value.FirstOrDefault();
                if (temp != null)
                    Assert.AreEqual(ChadderError.OK, temp.DeleteDevice().Result);
            }
            SourceInstances.Clear();
        }
        public class TestInsight : IInsight
        {
            public void Report(Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }

            public void Track(string e, ChadderError error)
            {
            }

            public void Track(string e)
            {
            }

            public object StartTimer(string e)
            {
                return null;
            }

            public void StopTimer(object h)
            {
            }
        }
        [TestInitialize]
        public void Initialize()
        {
            Insight.Instance = new TestInsight();
            int i = 0;
            while (i < 10) // Will always delete the first 10 databases no matter what
                while (ChadderSQLMainDB.Exists(i++).Result)
                    ChadderSQLMainDB.clearData(i - 1).Wait();
        }
    }

}
