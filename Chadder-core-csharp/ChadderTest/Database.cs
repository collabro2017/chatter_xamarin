using ChadderLib.DataModel;
using ChadderLib.SQL;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ChadderTest
{
    [TestFixture]
    public class Database : BaseTest
    {
        [Test]
        public async void Main()
        {
            var i = 999999;
            if (File.Exists(ChadderSQLMainDB.GetFilePath(i)))
                File.Delete(ChadderSQLMainDB.GetFilePath(i));
            var db = await ChadderSQLMainDB.GetDatabase(i);
            var acc = new ChadderAccount();
            acc.username = "UserName";
            acc.sessionCookie = null;
            acc.lastUpdateProcessed = 123;
            acc.dbKey = Encoding.UTF8.GetBytes("Test");
            acc.dbKeyType = KEY_TYPE.MASTER_KEY;

            await db.SaveItem(acc);
            var acc2 = await db.GetDefaultAccount();
            Assert.Null(acc2);
            acc.sessionCookie = "LoL";
            await db.SaveItem(acc);
            acc2 = await db.GetDefaultAccount();
            Assert.NotNull(acc2);
            Assert.AreEqual(acc2.username, acc.username);
            Assert.AreEqual(acc2.sessionCookie, acc.sessionCookie);
            Assert.AreEqual(acc2.lastUpdateProcessed, acc.lastUpdateProcessed);
            Assert.AreEqual(acc2.dbKey, acc.dbKey);
            Assert.AreEqual(acc2.dbKeyType, acc.dbKeyType);

            db.Close();

            db = await ChadderSQLMainDB.GetDatabase(i);
            acc2 = await db.GetDefaultAccount();
            Assert.NotNull(acc2);
            Assert.AreEqual(acc2.username, acc.username);
            Assert.AreEqual(acc2.sessionCookie, acc.sessionCookie);
            Assert.AreEqual(acc2.lastUpdateProcessed, acc.lastUpdateProcessed);
            Assert.AreEqual(acc2.dbKey, acc.dbKey);
            Assert.AreEqual(acc2.dbKeyType, acc.dbKeyType);

            acc2 = await db.GetAccount(acc.username.ToLower());
            Assert.NotNull(acc2);
            Assert.AreEqual(acc2.username, acc.username);
            Assert.AreEqual(acc2.sessionCookie, acc.sessionCookie);
            Assert.AreEqual(acc2.lastUpdateProcessed, acc.lastUpdateProcessed);
            Assert.AreEqual(acc2.dbKey, acc.dbKey);
            Assert.AreEqual(acc2.dbKeyType, acc.dbKeyType);

            await db.clearData(i);
        }

#if WINDOWS_PHONE || __ANDROID__ || __IOS__
        [Test]
        public async void User()
        {
            var i = 321;
            var pwd = ChadderSQLMainDB.getDefaulPassword();
            var pwd2 = ChadderSQLMainDB.getDefaulPassword();
            pwd2[0] = 123;
            var username = "test.db";
            if (File.Exists(ChadderSQLDatabase.GetFilePath(username, i)))
                File.Delete(ChadderSQLDatabase.GetFilePath(username, i));
            var db2 = new ChadderLib.SQL.Legacy.SQLDatabase(ChadderSQLDatabase.GetFilePath(username, i), pwd);
            db2.CreateTable<ChadderMessage>();
            var db = await ChadderSQLDatabase.GetUserDatabase(username, pwd, i);
            db.Close();
            db = null;
            Assert.Throws(typeof(SQLite.SQLiteException), async () =>
            {
                db = await ChadderSQLDatabase.GetUserDatabase(username, pwd2, i);
            });
            Assert.Null(db);
            db = await ChadderSQLDatabase.GetUserDatabase(username, pwd, i);
            Assert.NotNull(db);
            db.Close();
            db = null;
        }
#endif
    }
}
