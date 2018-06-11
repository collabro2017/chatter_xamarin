using Chadder.Client.Data;
using Chadder.Client.Util;
using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Client.SQL
{
    public class ChadderSQLUserDB : SQLDatabase
    {
        protected ChadderSQLUserDB(string filename, byte[] password)
            : base(filename, password)
        {
        }
        public async Task Migrate(long from)
        {
            await SetVersion(2);
        }
        static public async Task<ChadderSQLUserDB> GetUserDatabase(string userId, byte[] password, int instanceId)
        {
            var db = new ChadderSQLUserDB(GetFilePath(userId, instanceId), password);
            try
            {
                await db.CreateTableAsync<ChadderLocalUserInfo>();
                await db.CreateTableAsync<ChadderContact>();
                await db.CreateTableAsync<ChadderConversation>();
                await db.CreateTableAsync<ChadderMessage>();
                await db.CreateTableAsync<ChadderSQLPicture>();
                await db.CreateTableAsync<ChadderUserDevice>();

                var oldVersion = await db.GetVersion();
                await db.Migrate(oldVersion);

                return db;
            }
            catch (Exception ex)
            {
                Insight.Report(ex);
                db.Close();
                throw;
            }
        }
        static public async Task<ChadderSQLUserDB> GetNewUserDatabase(string userId, byte[] password, int instaceId)
        {
            await DeleteFile(GetFilePath(userId, instaceId));
            return await GetUserDatabase(userId, password, instaceId);
        }
        static public string GetFilePath(string userId, int InstanceId)
        {
            return System.IO.Path.Combine(DatabaseFilePath, GetFileName(userId, InstanceId));
        }


        // For unexplained reasons GetItem doesn't work with ChadderSQLPicture so use this method instead:
        public Task<Chadder.Client.Data.ChadderSQLPicture> GetPicture(int RecordId)
        {
            return Table<Chadder.Client.Data.ChadderSQLPicture>().Where(i => i.recordId == RecordId).FirstOrDefaultAsync();
        }
        static protected string GetFileName(string username, int InstanceId)
        {
#if DEBUG
            string databaseFilename = "ChadderDebug{0}.user.{1}.db";
#else
            string databaseFilename = "Chadder{0}.user.{1}.db";
#endif
            if (InstanceId == 0)
                return string.Format(databaseFilename, "", username);
            return string.Format(databaseFilename, InstanceId.ToString(), username);
        }
    }
}