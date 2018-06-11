using Chadder.Data.Response;
using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Client.Source
{
    public partial class ChadderSource
    {
        public async Task<ChadderError> GetMyInfo()
        {
            var response = await AuthorizedRequest<BasicResponse<ProfileInfo>>(Connection.AccountHub, "GetMyInfo");
            if (response.Error == ChadderError.OK)
            {
                db.LocalUser.Email = response.Extra.Email;
                db.LocalUser.EmailVerified = response.Extra.EmailVerified;
                db.LocalUser.IsNamePublic = response.Extra.IsNamePublic;
                db.LocalUser.Name = response.Extra.Name;
                db.LocalUser.PictureId = response.Extra.PictureId;
                db.LocalUser.Picture = db.GetPicture(db.LocalUser.PictureId);
                db.LocalUser.Phone = response.Extra.PhoneNumber;
                db.LocalUser.PhoneVerified = response.Extra.PhoneVerified;
                await sqlDB.UpdateAsync(db.LocalUser);
            }
            return response.Error;
        }
        public async Task<ChadderError> ChangeShareName(bool s)
        {
            var result = await AuthorizedRequest<BasicResponse>(Connection.AccountHub, "ChangeShareName", s);
            if (result.Error == ChadderError.OK)
            {
                db.LocalUser.IsNamePublic = s;
                await sqlDB.UpdateAsync(db.LocalUser);
            }
            return result.Error;
        }
        public async Task<ChadderError> ChangeName(string name)
        {
            var result = await AuthorizedRequest<BasicResponse>(Connection.AccountHub, "ChangeName", name);
            if (result.Error == ChadderError.OK)
            {
                db.LocalUser.Name = name;
                await sqlDB.UpdateAsync(db.LocalUser);
            }
            return result.Error;
        }
        public async Task<ChadderError> ChangeEmail(string email)
        {
            var result = await AuthorizedRequest<BasicResponse>(Connection.AccountHub, "ChangeEmail", email);
            if (result.Error == ChadderError.OK)
            {
                db.LocalUser.Email = email;
                await sqlDB.UpdateAsync(db.LocalUser);
            }
            return result.Error;
        }

        public async Task<ChadderError> ChangePicture(byte[] data)
        {
            var result = await UploadPicture(data);
            if (result.Error == ChadderError.OK)
            {
                var result2 = await AuthorizedRequest<BasicResponse>(Connection.AccountHub, "ChangePicture", result.Extra);
                if (result2.Error == ChadderError.OK)
                {
                    db.LocalUser.PictureId = result.Extra;
                    db.LocalUser.Picture = db.GetPicture(result.Extra);
                    await sqlDB.UpdateAsync(db.LocalUser);
                }
                return result2.Error;
            }
            return result.Error;
        }
    }
}
