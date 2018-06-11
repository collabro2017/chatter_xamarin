using Chadder.Data.Cmd;
using Chadder.Data.Keys;
using Chadder.Data.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Base
{
    public interface IContentFactory
    {
        Content Create(int type);
    }
    public class BasicContentFactory : IContentFactory
    {
        public Content Create(int type)
        {
            switch (type)
            {
                // Keys
                case (int)Content.CONTENT_TYPE.KEY_ECDSA_PUBLIC:
                    return new Chadder.Data.Keys.ECDSAPublicKey();
                case (int)Content.CONTENT_TYPE.KEY_ECDSA_PRIVATE:
                    return new Chadder.Data.Keys.ECDSAKeyPair();
                case (int)Content.CONTENT_TYPE.KEY_PUBLICK_BOOK:
                    return new PublicKeyBook();
                case (int)Content.CONTENT_TYPE.KEY_PRIVATE_BOOK:
                    return new PrivateKeyBook();

                // Plain Content
                case (int)Content.CONTENT_TYPE.PLAIN_BINARY:
                    return new PlainBinary();
                case (int)Content.CONTENT_TYPE.PLAIN_IMAGE:
                    return new ImageContent();
                case (int)Content.CONTENT_TYPE.PLAIN_USER_KEY:
                    return new UserPublicKey();
                case (int)Content.CONTENT_TYPE.PLAIN_DEVICE_KEY:
                    return new DevicePublicKey();

                // Symmetric
                case (int)Content.CONTENT_TYPE.SYM_AES_WITH_KEY:
                    return new Chadder.Data.Symmetric.AES256WithKey();
                case (int)Content.CONTENT_TYPE.SYM_AES_WITHOUT_KEY:
                    return new Chadder.Data.Symmetric.AES256WithoutKey();

                // Agreements
                case (int)Content.CONTENT_TYPE.ECDH_USER:
                    return new Chadder.Data.Asymmetric.ECDHUser();
                case (int)Content.CONTENT_TYPE.ECDH_DEVICE:
                    return new Chadder.Data.Asymmetric.ECDHDevice();
                case(int)Content.CONTENT_TYPE.ECDH_DEVICE_EPHEMERAL:
                    return new Chadder.Data.Asymmetric.ECDHDeviceEphemeral();

                // Signatures
                case (int)Content.CONTENT_TYPE.SIGN_ECDSA:
                    return new Chadder.Data.Sign.ECDSA();

                // Messages
                case (int)Content.CONTENT_TYPE.MSG_TEXT:
                    return new Chadder.Data.Messages.TextMessage();
                case (int)Content.CONTENT_TYPE.MSG_IMAGE:
                    return new ImageMessage();

                    // Command
                case (int)Content.CONTENT_TYPE.CMD_TAKE_BACK:
                    return new TakeMessageBackContent();
                case (int)Content.CONTENT_TYPE.CMD_PAIR_DEVICE:
                    return new PairDeviceContent();

            }
            return null;
        }
    }
}
