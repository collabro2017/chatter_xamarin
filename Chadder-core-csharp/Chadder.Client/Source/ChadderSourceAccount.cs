using Chadder.Client.Data;
using Chadder.Client.SQL;
using Chadder.Client.Util;
using Chadder.Data;
using Chadder.Data.Keys;
using Chadder.Data.Request;
using Chadder.Data.Response;
using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Chadder.Client.Source
{
    public partial class ChadderSource
    {
        public async Task<ChadderLocalUserRecord> GetDefaultUser()
        {
            var mainDB = await ChadderSQLMainDB.GetDatabase(InstanceId);
            return await mainDB.Table<ChadderLocalUserRecord>().FirstOrDefaultAsync();
        }
        public async Task<ChadderError> LoadUser(ChadderLocalUserRecord record)
        {
            try
            {
                var keyPackage = Chadder.Data.Base.Content.Deserialize<PlainBinary>(record.DatabaseKey);
                sqlDB = await ChadderSQLUserDB.GetUserDatabase(record.UserId, keyPackage.BinData, InstanceId);
            }
            catch (Exception ex)
            {
                Insight.Report(ex);
                return ChadderError.DB_FAILED_OPEN;
            }
            await db.Load();
            IsOnline = true;

            await FinishLoading();

            return ChadderError.OK;
        }
        public async Task<ChadderError> CreateUser(string Name, string Username = null, string password = null)
        {
            if (await DeviceExists() == false)
            {
                var temp = await CreateDevice();
                if (temp != ChadderError.OK)
                    return temp;
            }
            if (password != null)
                password = await SharedUtil.GetPasswordHash(password);
            var keypair = await KeyFactory.GenerateBook();
            var pbk = await keypair.GetPublicBook();
            var devicePair = await KeyFactory.RefreshBook(db.LocalDevice.PrivateKeyBook);
            var devicePublic = await devicePair.GetPublicBook();
            var request = new CreateUserParameter()
            {
                DeviceId = db.LocalDevice.DeviceId,
                Name = Name,
                Username = Username,
                Password = password,
                PublicKey = pbk.Serialize(),
                UserDevicePublicKey = devicePublic.Serialize(),
                RefreshToken = await CryptoHelper.CreateRandomData(32)
            };
            await request.Sign(db.LocalDevice.PrivateKeyBook.GetMaster());
            var response = await Session.PostRequestAPI<BasicResponse<string>>(Urls.CreateUser, request);
            if (response.Error == ChadderError.OK)
            {
                var key = await CryptoHelper.CreateRandomData(32);
                sqlDB = await ChadderSQLUserDB.GetUserDatabase(response.Extra, key, InstanceId);

                db.LocalUser = new ChadderLocalUserInfo()
                {
                    PrivateKeyBookData = keypair.Serialize(),
                    UserId = response.Extra,
                    Name = Name,
                    Picture = db.GetPicture(null),
                    RefreshToken = request.RefreshToken
                };
                await sqlDB.InsertAsync(db.LocalUser);

                var keyPackage = new PlainBinary(key);

                var record = new ChadderLocalUserRecord()
                {
                    UserId = response.Extra,
                    Name = Name,
                    DatabaseKey = keyPackage.Serialize(),
                    RefreshToken = request.RefreshToken

                };
                var mainDB = await ChadderSQLMainDB.GetDatabase(InstanceId);
                await mainDB.InsertAsync(record);
                IsOnline = true;
                await FinishLoading();
            }
            return response.Error;
        }
        public async Task<ChadderError> Login(string username, string password)
        {
            if (await DeviceExists() == false)
            {
                var temp = await CreateDevice();
                if (temp != ChadderError.OK)
                    return temp;
            }
            password = await SharedUtil.GetPasswordHash(password);

            var devicePair = await KeyFactory.RefreshBook(db.LocalDevice.PrivateKeyBook);
            var devicePublic = await devicePair.GetPublicBook();
            var request = new LoginParameter()
            {
                Username = username,
                Password = password,
                DeviceId = db.LocalDevice.DeviceId,
                PublicKey = devicePublic.Serialize(),
                RefreshToken = await CryptoHelper.CreateRandomData(32)
            };
            await request.Sign(db.LocalDevice.PrivateKeyBook.GetMaster());
            var response = await Session.PostRequestAPI<BasicResponse<string>>(Urls.Login, request);
            if (response.Error == ChadderError.OK)
            {
                var key = await CryptoHelper.CreateRandomData(32);
                sqlDB = await ChadderSQLUserDB.GetNewUserDatabase(response.Extra, key, InstanceId);

                db.LocalUser = new ChadderLocalUserInfo()
                {
                    UserId = response.Extra,
                    Name = username,
                    RefreshToken = request.RefreshToken
                };
                await sqlDB.InsertAsync(db.LocalUser);

                var keyPackage = new PlainBinary(key);

                var record = new ChadderLocalUserRecord()
                {
                    UserId = response.Extra,
                    Name = username,
                    DatabaseKey = keyPackage.Serialize()
                };
                var mainDB = await ChadderSQLMainDB.GetDatabase(InstanceId);
                await mainDB.InsertAsync(record);
                IsOnline = true;
                await FinishLoading();
            }
            return response.Error;
        }
        protected async Task FinishLoading()
        {
            try
            {
                var timer = Insight.StartTimer("FinishLoading");
                var backoff = new ExponentialBackoff(1000, 5000);
                while (await CreateToken() != ChadderError.OK)
                    await backoff.Failed();
                backoff.Reset();
                await Connection.Connect();

                while (await GetPackages() != ChadderError.OK)
                    await backoff.Failed();
                backoff.Reset();

                while (await GetMyDevices() != ChadderError.OK)
                    await backoff.Failed();
                backoff.Reset();

                while (await UpdateContacts() != ChadderError.OK)
                    await backoff.Failed();
                Insight.StopTimer(timer);
                backoff.Reset();
                while (await GetMyInfo() != ChadderError.OK)
                    await backoff.Failed();

                StartSendPendingMessages();
            }
            catch (Exception ex)
            {
                Insight.Report(ex);
            }
        }

        public async Task<ChadderError> CreateToken()
        {
            var request = new CreateTokenParameter()
            {
                UserId = db.LocalUser.UserId,
                DeviceId = db.LocalDevice.DeviceId,
                RefreshToken = db.LocalUser.RefreshToken
            };

            //var master = db.LocalDevice.PrivateKeyBook.GetMaster();
            //request.Signature = await master.Sign(request.Serialize());
            var response = await Session.PostRequestAPI<BasicResponse<string>>(Urls.GetAccessToken, request);
            if (response.Error == ChadderError.OK)
            {
                Session.Token = response.Extra;
                Connection.Token = response.Extra;
            }
            return response.Error;
        }

        public async Task<ChadderError> Logout()
        {
            var result = await Session.PostRequestAPI<BasicResponse>(Urls.Logout);
            if (result.Error == ChadderError.OK)
                Insight.Track("Successfull Logout");
            else
                Insight.Track("Logout Failed");
            Session.Token = null;
            Connection.Disconnect();
            IsOnline = false;
            sqlDB.Dispose();
            sqlDB = null;
            var mainDb = await ChadderSQLMainDB.GetDatabase(InstanceId);
            await mainDb.DeleteUser(db.LocalUser.UserId);
            db = new ChadderDatabase(this);
            return ChadderError.OK;
        }
        public async Task<ChadderError> DeleteUser()
        {
            var result = await Session.PostRequestAPI<BasicResponse>(Urls.DeleteUser);
            if (result.Error == ChadderError.OK)
            {
                await Logout();
            }
            return result.Error;
        }
        public async Task<ChadderError> DeleteDevice()
        {
            if (await DeviceExists() == false)
                return ChadderError.OK;
            var request = new DeleteDeviceParameter()
            {
                DeviceId = db.LocalDevice.DeviceId
            };
            await request.Sign(db.LocalDevice.PrivateKeyBook.GetMaster());
            var result = await Session.PostRequestAPI<BasicResponse>(Urls.DeleteDevice, request);
            if (result.Error == ChadderError.OK)
            {
                await ChadderSQLMainDB.clearData(InstanceId);
            }
            return result.Error;
        }

        public string GetMyScannableId()
        {
            return SharedUtil.GetUserScannableId(db.LocalUser.UserId, db.LocalUser.PrivateKeyBook.GetMaster());
        }
        public async Task<ChadderContact> ProcessScannedId(string str)
        {
            try
            {
                var data = str.Split(',');
                if (int.Parse(data[0]) == 1)
                {
                    var contact = db.GetContact(data[1]);
                    if (contact == null)
                    {
                        var response = await GetUserInfo(data[1]);
                        if (response.Error == ChadderError.OK)
                            contact = response.Extra;
                        else
                        {
                            Insight.Track("ProcessScannedId GetUserInfo=", response.Error);
                            return null;
                        }
                    }
                    var compressed = Convert.FromBase64String(data[2]);
                    if (compressed.SequenceEqual(contact.PublicKeyBook.GetMaster().Compressed))
                        contact.KeyStatus = PublicKeyStatus.VERIFIED;
                    else
                        contact.KeyStatus = PublicKeyStatus.CHANGED;
                    if (contact.IsTemporary == false)
                        await sqlDB.UpdateAsync(contact);
                    return contact;
                }
            }
            catch (Exception ex)
            {
                Insight.Track("ProcessScannedId");
                Insight.Report(ex);
            }
            return null;
        }


#if DEBUG
        public async Task<ChadderError> RequestPasswordReset(string email)
        {
            return (await Session.PostRequestAPI<BasicResponse>(Urls.RequestPasswordReset, email)).Error;
        }
        public async Task<BasicResponse<string>> PasswordReset(string code)
        {
            using (var client = new System.Net.WebClient())
            {
                var text1 = await client.DownloadStringTaskAsync(string.Format("{0}?code={1}", Urls.PasswordReset, code));
                return Newtonsoft.Json.JsonConvert.DeserializeObject<BasicResponse<string>>(text1);
            }
        }
#else
        public async Task<ChadderError> RequestPasswordReset(string email)
        {
            return (await Session.PostRequestAPI<BasicResponse>(Urls.RequestPasswordReset, email)).Error;
        }
#endif
    }
}
