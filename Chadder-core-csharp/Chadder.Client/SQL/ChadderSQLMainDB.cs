using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Chadder.Data.Util;
using Chadder.Client.Util;
using Chadder.Client.Data;
//using Mono.Data.Sqlcipher;
#if WINDOWS_PHONE_APP
using Windows.UI.Xaml;
using System.Diagnostics;
using Windows.Storage;
#endif

#if __ANDROID__

#endif

namespace Chadder.Client.SQL
{
    public class ChadderSQLMainDB : SQLDatabase
    {
        private static readonly byte[] defaultKey = new byte[32]
        {
            0xE3, 0x57, 0x83, 0x64, 0x05, 0x84, 0xc7, 0x25,
            0x3D, 0xDE, 0x29, 0x66, 0x7C, 0xC1, 0x12, 0x4C,
            0xB5, 0x61, 0xDC, 0x1D, 0x94, 0x35, 0x13, 0xF1,
            0x2D, 0xE8, 0xB5, 0x6F, 0x9A, 0xC2, 0xC5, 0xF4,
        };

#if TEST
        public
#endif
        static byte[] getDefaulPassword()
        {
            return CryptoHelper.Sha256(defaultKey);
        }

        public int InstaceId { get; private set; }

        private ChadderSQLMainDB(int instanceId)
            : base(GetFilePath(instanceId), getDefaulPassword())
        {
            InstaceId = instanceId;
        }
        private static Dictionary<int, ChadderSQLMainDB> Instances = new Dictionary<int, ChadderSQLMainDB>();
        static public async Task<ChadderSQLMainDB> GetDatabase(int instanceId)
        {
            if (Instances.ContainsKey(instanceId))
                return Instances[instanceId];
            var db = new ChadderSQLMainDB(instanceId);
            Instances[instanceId] = db;

            try
            {
                await db.CreateTableAsync<ChadderLocalDeviceInfo>();
                await db.CreateTableAsync<ChadderLocalUserRecord>();
                await db.SetVersion(1);
                return db;
            }
            catch (Exception ex)
            {
                Insight.Report(ex);
                db.Close();
                throw;
            }
        }

        public Task<ChadderLocalDeviceInfo> GetDevice()
        {
            return Table<ChadderLocalDeviceInfo>().FirstOrDefaultAsync();
        }

        public async Task DeleteUser(string userId)
        {
            await DeleteUser(await Table<ChadderLocalUserRecord>().Where(i => i.UserId == userId).FirstOrDefaultAsync());
        }
        public async Task DeleteUser(ChadderLocalUserRecord user)
        {
            await DeleteAsync(user);

            await DeleteFile(ChadderSQLUserDB.GetFilePath(user.UserId, InstaceId));
        }
        public static Task<bool> Exists(int instanceId)
        {
            return FileExists(GetFilePath(instanceId));
        }
        public static async Task<bool> clearData(int instanceId)
        {
            if (await Exists(instanceId))
            {
                using (var mainSql = await GetDatabase(instanceId))
                {
                    foreach (var acc in await mainSql.GetItems<ChadderLocalUserRecord>())
                    {
                        await mainSql.DeleteUser(acc);
                    }
                }
                await DeleteFile(GetFilePath(instanceId));
                return true;
            }
            return false;
        }

#if LOCAL
        public const string databaseFilename = "SharedDebug{0}.db";
#elif PRODUCTION
        public const string databaseFilename = "Shared{0}.db";
#endif
        public override void Close()
        {
            Instances.Remove(InstaceId);
            base.Close();
        }

        static public string GetFilePath(int instanceId)
        {
            if (instanceId == 0)
                return Path.Combine(DatabaseFilePath, string.Format(databaseFilename, ""));
            return Path.Combine(DatabaseFilePath, string.Format(databaseFilename, instanceId.ToString()));
        }
    }

}
