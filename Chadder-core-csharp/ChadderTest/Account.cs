using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ChadderLib.DataSource;
using ChadderCDM;
using System.Threading.Tasks;

namespace ChadderTest
{
    [TestFixture]
    class Account : BaseTest
    {
        [Test]
        [Explicit]
        public async void TestInvalidRegistration()
        {
            var random = new Random();
            var username = "autotest" + random.Next().ToString();
            var password = random.Next().ToString("x8");
            // Need username
            var source = CreateSource();
            var result = await source.register("AutoTest", null, "password");
            Assert.NotNull(result);
            result = await source.register("AutoTest", "", "password");
            Assert.NotNull(result);

            // need password
            result = await source.register("AutoTest", username, null);
            Assert.NotNull(result);
            result = await source.register("AutoTest", username, "");
            Assert.NotNull(result);

            // Valid password
            result = await source.register("AutoTest", username, "00");
            Assert.NotNull(result);

            source.TestDeviceName = "Testing a really long device name nasdansdjaskdnasjdn ";
            // Valid Device Name
            result = await source.register("AutoTest", username, "000000");
            Assert.Null(result);
            Assert.AreEqual(await source.ChangeShareName(false), ChadderError.OK);
        }

        [Test]
        public async void TestRegistration()
        {
            var source = CreateSource();
            source.NotificationHandle = "asdinasidajsnd";
            var random = new Random();
            var username = "autotest" + random.Next().ToString();
            var password = random.Next().ToString("x8");
            // Valid account
            var result = await source.register("AutoTest", username, password);
            Assert.Null(result);

            //test duplicate username
            var source2 = CreateSource();
            result = await source2.register("AutoTest", username, "000000");
            Assert.NotNull(result);
        }

        [Test]
        public async void TestLogin()
        {
            var random = new Random();
            var username = "autotest" + random.Next().ToString();
            var password = random.Next().ToString("x8");
            var source = await CreateAccount(username, password);
            // Valid account
            var str = await source.Logout(false);
            Assert.Null(str);

            var result = await source.Login(username, password, null);
            Assert.AreEqual(ChadderError.OK, result);
            result = await source.GenerateNewKey();
            Assert.AreEqual(ChadderError.OK, result);
        }
    }
}
