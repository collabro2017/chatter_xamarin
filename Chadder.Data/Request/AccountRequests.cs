using Chadder.Data.Keys;
using Chadder.Data.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Request
{
    public abstract class BasicSignedRequest
    {
        [JsonProperty(PropertyName = "s")]
        public byte[] Signature { get; set; }

        public abstract void Serialize(DataSerializer writer);
        public byte[] Serialize()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new DataSerializer(stream);
                Serialize(writer);
                return stream.ToArray();
            }
        }
        public async Task<bool> Verify(ECDSAPublicKey publicKey)
        {
            return await publicKey.Verify(Serialize(), Signature);
        }
        public async Task Sign(ECDSAKeyPair keyPair)
        {
            Signature = await keyPair.Sign(Serialize());
        }
    }
    public class UpdateNotificationHandleParameter : BasicSignedRequest
    {
        public string DeviceId { get; set; }
        public string Handle { get; set; }
        public override void Serialize(DataSerializer writer)
        {
            writer.WriteString(Handle);
        }
    }
    public class CreateTokenParameter : BasicSignedRequest
    {
        public string DeviceId { get; set; }
        public string UserId { get; set; }
        public byte[] RefreshToken { get; set; }

        public override void Serialize(DataSerializer writer)
        {
            writer.WriteShortString(DeviceId);
            writer.WriteShortString(UserId);
            writer.WriteBin(RefreshToken);
        }
    }
    public class DeleteDeviceParameter : BasicSignedRequest
    {
        public string DeviceId { get; set; }
        public override void Serialize(DataSerializer writer)
        {
            writer.WriteShortString(DeviceId);
        }
    }
    public class CreateUserParameter : BasicSignedRequest
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] UserDevicePublicKey { get; set; }
        public byte[] RefreshToken { get; set; }

        public override void Serialize(DataSerializer writer)
        {
            writer.WriteShortString(DeviceId);
            writer.WriteShortString(Name);
            writer.WriteShortString(Username);
            writer.WriteString(Password);
            writer.WriteBin(PublicKey);
            writer.WriteBin(UserDevicePublicKey);
            writer.WriteBin(RefreshToken);
        }
    }

    public class LoginParameter : BasicSignedRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] RefreshToken { get; set; }

        public override void Serialize(DataSerializer writer)
        {
            writer.WriteShortString(Username);
            writer.WriteShortString(DeviceId);
            writer.WriteBin(PublicKey);
            writer.WriteBin(RefreshToken);
        }
    }

    public class CreateDeviceParameter
    {
        public CreateDeviceParameter()
        {
            AppVersion = 0;
        }
        public int AppVersion { get; set; }
        public string Name { get; set; }
        public DeviceType Type { get; set; }
        public byte[] PublicKey { get; set; }
        [JsonProperty(PropertyName = "h")]
        public string NotificationHandle { get; set; }
    }

    public class SendPackageParameter
    {
        public string UserId { get; set; }
        public byte[] Data { get; set; }
    }
    public class SetFriendTypeParameter
    {
        public string UserId { get; set; }
        public RelationshipType Type { get; set; }
    }
    public class ChangeDeviceNameParameter
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
    }
    public class PairDeviceParameters
    {
        public string DeviceId { get; set; }
        public byte[] Data { get; set; }
    }
}
