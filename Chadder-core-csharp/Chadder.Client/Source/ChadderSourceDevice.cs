using Chadder.Client.Data;
using Chadder.Client.SQL;
using Chadder.Data;
using Chadder.Data.Asymmetric;
using Chadder.Data.Base;
using Chadder.Data.Cmd;
using Chadder.Data.Request;
using Chadder.Data.Response;
using Chadder.Data.Symmetric;
using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Client.Source
{
    public partial class ChadderSource
    {
        public int InstanceId { get; private set; }
        public async Task<bool> DeviceExists()
        {
            if (db.LocalDevice == null)
            {
                var mainDB = await ChadderSQLMainDB.GetDatabase(InstanceId);
                db.LocalDevice = await mainDB.GetDevice();
            }

            return db.LocalDevice != null;
        }
        public async Task<ChadderError> CreateDevice()
        {
            var keypair = await KeyFactory.GenerateBook();
            var pbk = await keypair.GetPublicBook();
            var request = new CreateDeviceParameter()
            {
                Name = GetDeviceName(),
                PublicKey = pbk.Serialize(),
                Type = GetDeviceType(),
                NotificationHandle = NotificationHandle
            };

            var response = await Session.PostRequestAPI<BasicResponse<string>>(Urls.CreateDevice, request);
            if (response.Error == ChadderError.OK)
            {
                var mainDB = await ChadderSQLMainDB.GetDatabase(InstanceId);
                db.LocalDevice = new ChadderLocalDeviceInfo()
                {
                    DeviceId = response.Extra,
                    PrivateKeyBookData = keypair.Serialize()
                };
                await mainDB.InsertAsync(db.LocalDevice);
            }
            return response.Error;
        }

        public async Task<ChadderError> CreateNewKey()
        {
            var keypair = await KeyFactory.GenerateBook();
            var pbk = await keypair.GetPublicBook();
            var response = await AuthorizedRequest<BasicResponse>(Connection.AccountHub, "CreateNewKey", pbk.Serialize());
            if (response.Error == ChadderError.OK)
            {
                db.LocalUser.PrivateKeyBookData = keypair.Serialize();
                await sqlDB.UpdateAsync(db.LocalUser);
            }
            return response.Error;
        }

        public async Task<ChadderError> ChangeDeviceName(ChadderUserDevice device, string name)
        {
            var request = new ChangeDeviceNameParameter()
            {
                DeviceId = device.DeviceId,
                Name = name
            };
            var result = await AuthorizedRequest<BasicResponse>(Connection.AccountHub, "ChangeDeviceName", request);
            if (result.Error == ChadderError.OK)
            {
                device.Name = name;
                await sqlDB.UpdateAsync(device);
            }
            return result.Error;
        }
        public async Task<ChadderError> DeleteDevice(ChadderUserDevice device)
        {
            var response = await AuthorizedRequest<BasicResponse>(Connection.AccountHub, "DeleteDevice", device.DeviceId);
            if (response.Error == ChadderError.OK)
            {
                await sqlDB.DeleteAsync(device);
                db.LocalUser.Devices.Remove(device);
            }
            return response.Error;
        }
        public async Task<ChadderError> PairDevice(ChadderUserDevice device)
        {
            try
            {
                var package = new PairDeviceContent()
                {
                    Book = db.LocalUser.PrivateKeyBook
                };
                var content = await EncryptForDevice(package, device);
                var request = new PairDeviceParameters()
                {
                    DeviceId = device.DeviceId,
                    Data = content.Serialize()
                };
                var result = await AuthorizedRequest<BasicResponse<string>>(Connection.AccountHub, "PairDevice", request);
                if (result.Error == ChadderError.OK)
                {
                    device.HasUserKey = true;
                    await sqlDB.UpdateAsync(device);
                }
                return result.Error;
            }
            catch (Exception ex)
            {
                Insight.Report(ex);
                return ChadderError.GENERAL_EXCEPTION;
            }
        }

        public async Task<ChadderError> GetMyDevices()
        {
            var response = await AuthorizedRequest<BasicArrayResponse<DeviceInfo>>(Connection.AccountHub, "GetMyDevices");
            if (response.Error == ChadderError.OK)
            {
                foreach (var d in response.List)
                {
                    var device = db.GetDevice(d.Id);
                    if (device == null)
                    {
                        device = new ChadderUserDevice(d);
                        device.CurrentDevice = device.DeviceId == db.LocalDevice.DeviceId;
                        await db.AddDevice(device);
                    }
                    else
                    {
                        device.Update(d);
                        await sqlDB.UpdateAsync(device);
                    }
                }
            }
            return response.Error;
        }
        public async Task<Content> EncryptForDevice(Content content, ChadderUserDevice device)
        {
            var key = new ECDHDevice(db.LocalDevice.DeviceId, device.DeviceId);
            return await AES256WithKey.Encrypt(this, key, content);
        }
        public async Task<ChadderError> UpdateNotificationHandleParameter(string handle)
        {
            NotificationHandle = handle;
            if (await DeviceExists() && db.LocalDevice.NotificationHandle != handle)
            {
                var request = new UpdateNotificationHandleParameter()
                {
                    DeviceId = db.LocalDevice.DeviceId,
                    Handle = handle
                };
                await request.Sign(db.LocalDevice.PrivateKeyBook.GetMaster());
                var result = await Session.PostRequestAPI<BasicResponse>(Urls.UpdateNotification, request);
                if (result.Error == ChadderError.OK)
                {
                    db.LocalDevice.NotificationHandle = handle;
                    var mainDB = await ChadderSQLMainDB.GetDatabase(InstanceId);
                    await mainDB.UpdateAsync(db.LocalDevice);
                }
                return result.Error;
            }
            return ChadderError.OK;
        }
        public DeviceType GetDeviceType()
        {
#if __ANDROID__
            return DeviceType.ANDROID;
#else
            return DeviceType.RESERVED;
#endif
        }

        /// <summary>
        /// Gets the name of the device. Depending on the OS
        /// </summary>
        /// <returns></returns>
        public string GetDeviceName()
        {
#if TEST
            if(TestDeviceName != null)
                return TestDeviceName;
#endif
#if WINDOWS_PHONE_APP
			return "WindowsPhone";
#elif WINDOWS_PHONE
            return "Windows Phone";
#elif __ANDROID__
            return Android.OS.Build.Model;
#elif __IOS__
            return UIKit.UIDevice.CurrentDevice.Name;
#else
			return "Other";
#endif
        }
    }
}
