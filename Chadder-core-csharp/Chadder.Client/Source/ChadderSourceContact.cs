using Chadder.Client.Data;
using Chadder.Data;
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
        public async Task<BasicResponse<ChadderContact>> AddContact(string userId)
        {
            var response = await AuthorizedRequest<BasicResponse<ContactInfo>>(Connection.ContactHub, "AddUser", userId);
            var result = new BasicResponse<ChadderContact>();
            result.Copy(response);
            if (response.Error == ChadderError.OK)
            {
                result.Extra = new ChadderContact(response.Extra, this);
                await db.AddContact(result.Extra, false);
            }
            return result;
        }
        public async Task<ChadderError> UpdateContacts()
        {
            var response = await AuthorizedRequest<BasicArrayResponse<ContactInfo>>(Connection.ContactHub, "GetUpdatedFriends", db.LocalUser.LastContactUpdate);
            if (response.Error == ChadderError.OK)
            {
                foreach (var c in response.List)
                {
                    var contact = db.GetContact(c.Id);
                    if (contact == null)
                        await db.AddContact(new ChadderContact(c, this), true);
                    else
                    {
                        contact.Update(c, this);
                        await sqlDB.UpdateAsync(contact);
                    }
                }
                db.LocalUser.LastContactUpdate = response.Time;
                await sqlDB.UpdateAsync(db.LocalUser);
            }
            return response.Error;
        }
        public async Task<BasicArrayResponse<ChadderContact>> FindUser(string str)
        {
            var response = await AuthorizedRequest<BasicArrayResponse<ContactInfo>>(Connection.ContactHub, "FindUser", str);
            var result = new BasicArrayResponse<ChadderContact>();
            result.Error = response.Error;
            result.InnerException = response.InnerException;
            result.Time = response.Time;
            if (response.Error == ChadderError.OK)
            {
                foreach (var c in response.List)
                {
                    result.List.Add(new ChadderContact(c, this) { IsTemporary = true });
                }
            }
            return result;
        }
        public async Task<ChadderError> SetFriendType(ChadderContact contact, RelationshipType type)
        {
            if (contact.Type == type)
                return ChadderError.OK;
            var result = await AuthorizedRequest<BasicResponse>(Connection.ContactHub, "SetFriendType", contact.UserId, type);
            if (result.Error == ChadderError.OK)
            {
                contact.Type = type;
                await sqlDB.UpdateAsync(contact);
            }
            return result.Error;
        }
        public async Task<ChadderError> DeleteContact(ChadderContact contact)
        {
            var result = await AuthorizedRequest<BasicResponse>(Connection.ContactHub, "DeleteContact", contact.UserId);
            if (result.Error == ChadderError.OK)
            {
                await db.DeleteContact(contact);
            }
            return result.Error;
        }

        public Task SetPublicKeyStatus(ChadderContact contact, PublicKeyStatus status)
        {
            contact.KeyStatus = status;
            return sqlDB.UpdateAsync(contact);
        }

        public async Task<BasicResponse<ChadderContact>> GetUserInfo(string id)
        {
            var response = await AuthorizedRequest<BasicResponse<ContactInfo>>(Connection.ContactHub, "GetUserInfo", id);
            var result = new BasicResponse<ChadderContact>();
            result.Error = response.Error;
            result.InnerException = response.InnerException;
            result.Time = response.Time;
            if (response.Error == ChadderError.OK)
            {
                result.Extra = ChadderContact.Create(response.Extra, this, true);
            }
            return result;
        }
        public async Task<ChadderError> ReportContact(ChadderContact contact, string reason)
        {
            var response = await AuthorizedRequest<BasicResponse>(Connection.ContactHub, "ReportUser", contact.UserId, reason, null);
            if (response.Error == ChadderError.OK)
            {
                return await SetFriendType(contact, RelationshipType.BLOCKED);
            }
            return response.Error;
        }

    }
}
