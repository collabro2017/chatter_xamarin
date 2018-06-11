using Chadder.Client.Data;
using Chadder.Client.SQL;
using Chadder.Data.Base;
using Chadder.Data.Keys;
using Chadder.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Client.Test
{
    [TestClass]
    public class SQL
    {
        [TestMethod]
        public async Task MainSqlDatabase()
        {
            var factory = new Chadder.Data.Keys.ECDSASimpleFactory();
            var deviceInfo = new ChadderLocalDeviceInfo()
            {
                DeviceId = Guid.NewGuid().ToString(),
                NotificationHandle = Guid.NewGuid().ToString(),
                PrivateKeyBookData = (await factory.GenerateBook()).Serialize()
            };
            var user = new ChadderLocalUserRecord()
            {
                Name = "Test name",
                UserId = Guid.NewGuid().ToString(),
                DatabaseKey = await CryptoHelper.CreateRandomData(32)
            };
            using (var mainSql = await ChadderSQLMainDB.GetDatabase(0))
            {
                await mainSql.InsertAsync(deviceInfo);
                await mainSql.InsertAsync(user);
            }
            using (var mainSql = await ChadderSQLMainDB.GetDatabase(0))
            {
                var device2 = await mainSql.GetDevice();
                Assert.AreEqual(deviceInfo.DeviceId, device2.DeviceId);
                Assert.AreEqual(deviceInfo.NotificationHandle, device2.NotificationHandle);
                Assert.IsTrue(deviceInfo.PrivateKeyBookData.SequenceEqual(device2.PrivateKeyBookData));

                var user2 = await mainSql.Table<ChadderLocalUserRecord>().FirstOrDefaultAsync();
                Assert.AreEqual(user.Name, user2.Name);
                Assert.AreEqual(user.UserId, user2.UserId);
                Assert.IsTrue(user.DatabaseKey.SequenceEqual(user2.DatabaseKey));
            }
            await ChadderSQLMainDB.clearData(0);
            System.IO.File.Delete(ChadderSQLMainDB.GetFilePath(0));
            Assert.IsFalse(System.IO.File.Exists(ChadderSQLMainDB.GetFilePath(0)));


        }

        [TestMethod]
        public async Task UserSqlDatabase()
        {
            var key = await CryptoHelper.CreateRandomData(32);
            var key2 = await CryptoHelper.CreateRandomData(32);
            var userId = Guid.NewGuid().ToString();

            using (var db = await ChadderSQLUserDB.GetNewUserDatabase(userId, key, 0)) ;
            try
            {
                //using (var db = await ChadderSQLUserDB.GetUserDatabase(userId, key2, 0)) ;
                Assert.Fail("Opened db with wrong dbkey");
            }
            catch { }

            File.Delete(ChadderSQLUserDB.GetFilePath(userId, 0));
        }
    }
}
