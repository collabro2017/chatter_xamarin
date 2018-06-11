using Chadder.Data.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Client.Util
{
    public class SQLDatabase : SQLite.Async.SQLiteAsyncConnection, IDisposable
    {
#if WINDOWS_PHONE_APP || WINDOWS_PHONE || WINDOWS_APP
        public static Windows.Storage.StorageFolder DatabaseFolder
        {
            get
            {
                return Windows.Storage.ApplicationData.Current.LocalFolder;
            }
        }
#endif
        public static string DatabaseFilePath
        {
            get
            {
#if WINDOWS_PHONE || WINDOWS_APP
                // Windows Phone expects a local path, not absolute
                return DatabaseFolder.Path;
#elif __ANDROID__
                // Just use whatever directory SpecialFolder.Personal returns
                return Environment.GetFolderPath(Environment.SpecialFolder.Personal); ;
#elif __IOS__
                // we need to put in /Library/ on iOS5.1 to meet Apple's iCloud terms
                // (they don't want non-user-generated data in Documents)
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
                return Path.Combine(documentsPath, "..", "Library"); // Library folder
#elif WINDOWS_DESKTOP
                if (System.IO.File.Exists("Data") == false)
                    System.IO.Directory.CreateDirectory("Data");
                return "Data";
#endif
            }
        }



        protected SQLDatabase(string path, byte[] password)
            : base(path, password)
        {
        }

        public Task<long> GetVersion()
        {
            return ExecuteScalarAsync<long>("PRAGMA user_version;");
        }
        public Task SetVersion(long v)
        {
            return ExecuteAsync("PRAGMA user_version = " + v.ToString());
        }
        public async Task<string> GetSetting(string name)
        {
            var item = await GetItem<ChadderSQLDeviceSetting>(i => i.name == name);
            if (item != null)
                return item.value;
            return null;
        }
        public async Task InsertSetting(string name, string value)
        {
            await InsertAsync(new ChadderSQLDeviceSetting() { name = name, value = value });
        }
        public async Task InsertOrUpdateSetting(string name, string value)
        {
            var item = await GetItem<ChadderSQLDeviceSetting>(i => i.name == name);
            if (item != null)
            {
                item.value = value;
                await UpdateAsync(item);
            }
            else
            {
                await InsertAsync(new ChadderSQLDeviceSetting() { name = name, value = value });
            }
        }
        public Task<List<T>> GetItems<T>() where T : ISQLRecord, new()
        {
            return Table<T>().ToListAsync();
        }

        public Task<T> GetItem<T>(int id) where T : ISQLRecord, new()
        {
            return Table<T>().Where(x => x.recordId == id).FirstOrDefaultAsync();
        }

        public Task<T> GetItem<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : ISQLRecord, new()
        {
            return Table<T>().Where(predicate).FirstOrDefaultAsync();
        }

        public async Task<int> SaveItem<T>(T item) where T : ISQLRecord
        {
            if (item.recordId != 0)
            {
                await UpdateAsync(item);
                return item.recordId;
            }
            else
            {
                return await InsertAsync(item);
            }
        }

        public Task<int> DeleteItem<T>(int id) where T : ISQLRecord, new()
        {
            return DeleteAsync(new T() { recordId = id });
        }

        public async Task<bool> TableExists(string tableName)
        {
            return await ExecuteScalarAsync<int>("SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name = ?", tableName) == 0;
        }
        public static Task<bool> FileExists(string filename)
        {
            return Task.FromResult(System.IO.File.Exists(filename));
        }
        public static async Task DeleteFile(string filename)
        {
            try
            {
#if WINDOWS_PHONE_APP || WINDOWS_PHONE
            Windows.Storage.StorageFolder folder = DatabaseFolder;
            var mainFile = await folder.GetFileAsync(filename);
            await mainFile.DeleteAsync();
#elif __ANDROID__ || __IOS__ || WINDOWS_DESKTOP
                bool exists = System.IO.File.Exists(filename);
                if (exists)
                    System.IO.File.Delete(filename);
#endif
            }
            catch (Exception ex)
            {
                Insight.Track("DeleteFile: " + filename);
                Insight.Report(ex);
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
    public interface ISQLRecord
    {
        int recordId { get; set; }
    }
    public class SQLRecordBase : Util.MyNotifyChanged, ISQLRecord
    {
        [PrimaryKey, AutoIncrement]
        public int recordId { get; set; }
        public bool insertOnZeroOnly { get { return true; } }
    }

    public class SQLSingleton : Util.MyNotifyChanged, ISQLRecord
    {
        public bool insertOnZeroOnly { get { return false; } }
        [PrimaryKey]
        public int recordId
        {
            get { return 1; }
            set { }
        }
    }
    public class ChadderSQLDeviceSetting : SQLRecordBase
    {
        [SQLite.Indexed(Unique = true)]
        [SQLite.MaxLength(32)]
        public string name { get; set; }
        public string value { get; set; }
    }
}
