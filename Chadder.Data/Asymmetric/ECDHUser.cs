using Chadder.Data.Keys;
using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Asymmetric
{
    public class ECDHUser : ECDH
    {
        public override int Type { get { return (int)CONTENT_TYPE.ECDH_USER; } }

        public ECDHUser() { }

        public ECDHUser(string fromId, string toId)
        {
            SourceId = fromId;
            TargetId = toId;
        }

        public override async Task<byte[]> GetBinContent(Base.IContentContext context)
        {
            var prBook = await context.GetUserKeyPair();
            if (prBook == null)
            {
                Insight.Track("Device doesn't hold user key");
                return null;
            }
            if (await context.GetMyUserId() == TargetId)
            {
                var contact = await context.GetUser(SourceId);

                if (contact == null)
                {
                    Insight.Track("SourceId is not a valid user - " + SourceId);
                    return null;
                }
                return await prBook.GetValidKey<ECDSAKeyPair>(KeyPurpose.DH_RECEIVE).SharedSecret(contact.PublicKeyBook.GetValidKey<ECDSAPublicKey>(KeyPurpose.DH_SEND));

            }
            else
            {
                var contact2 = await context.GetUser(TargetId);

                if (contact2 == null)
                {
                    Insight.Track("TargetId is not a valid user - " + TargetId);
                    return null;
                }
                return await prBook.GetValidKey<ECDSAKeyPair>(KeyPurpose.DH_SEND).SharedSecret(contact2.PublicKeyBook.GetValidKey<ECDSAPublicKey>(KeyPurpose.DH_RECEIVE));
            }
        }
    }
}
