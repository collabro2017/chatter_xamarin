using Chadder.Data.Keys;
using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Asymmetric
{
    public class ECDHDevice : ECDH
    {
        public override int Type { get { return (int)CONTENT_TYPE.ECDH_DEVICE; } }

        public ECDHDevice() { }
        public ECDHDevice(string fromId, string toId)
        {
            SourceId = fromId;
            TargetId = toId;
        }

        public override async Task<byte[]> GetBinContent(Base.IContentContext context)
        {
            var prBook = await context.GetDeviceKeyPair();
            if (TargetId == await context.GetMyDeviceId())
            {
                var device = await context.GetDevice(SourceId);
                if(device == null)
                {
                    Insight.Track("SourceId is not a valid device - " + SourceId);
                    return null;
                }
                return await prBook.GetValidKey<ECDSAKeyPair>(KeyPurpose.DH_RECEIVE).SharedSecret(device.PublicKeyBook.GetValidKey<ECDSAPublicKey>(KeyPurpose.DH_SEND));
            }
            else
            {
                var device2 = await context.GetDevice(TargetId);
                if (device2 == null)
                {
                    Insight.Track("TargetId is not a valid device - " + TargetId);
                    return null;
                }
                return await prBook.GetValidKey<ECDSAKeyPair>(KeyPurpose.DH_SEND).SharedSecret(device2.PublicKeyBook.GetValidKey<ECDSAPublicKey>(KeyPurpose.DH_RECEIVE));
            }
        }
    }

    public class ECDHDeviceEphemeral : ECDHEphemeral
    {
        private ECDSAKeyPair PrivateKey;
        public override int Type { get { return (int)CONTENT_TYPE.ECDH_DEVICE_EPHEMERAL; } }
        public ECDHDeviceEphemeral() { }
        public static async Task<ECDHDeviceEphemeral> Create(string TargetDeviceId)
        {
            var result = new ECDHDeviceEphemeral();
            result.PrivateKey = await ECDSAKeyPair.Generate();
            result.PublicKey = result.PrivateKey.Public;
            result.TargetId = TargetDeviceId;
            return result;
        }
        public override async Task<byte[]> GetBinContent(Base.IContentContext context)
        {
            if (TargetId == await context.GetMyDeviceId())
            {
                var prBook = await context.GetDeviceKeyPair();
                return await prBook.GetValidKey<ECDSAKeyPair>(KeyPurpose.DH_RECEIVE).SharedSecret(PublicKey);
            }
            else
            {
                var device2 = await context.GetDevice(TargetId);
                if (device2 == null)
                {
                    Insight.Track("TargetId is not a valid device - " + TargetId);
                    return null;
                }
                return await PrivateKey.SharedSecret(device2.PublicKeyBook.GetValidKey<ECDSAPublicKey>(KeyPurpose.DH_RECEIVE));
            }
        }
    }
}
