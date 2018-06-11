using Chadder.Data.Asymmetric;
using Chadder.Data.Base;
using Chadder.Data.Cmd;
using Chadder.Data.Keys;
using Chadder.Data.Messages;
using Chadder.Data.Sign;
using Chadder.Data.Symmetric;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Test
{
    [TestClass]
    public class Factory
    {
        [TestMethod]
        public void TestFactory()
        {
            var factory = new BasicContentFactory();
            var content = factory.Create((int)Content.CONTENT_TYPE.CMD_PAIR_DEVICE);
            Assert.IsInstanceOfType(content, typeof(PairDeviceContent));

            content = factory.Create((int)Content.CONTENT_TYPE.CMD_TAKE_BACK);
            Assert.IsInstanceOfType(content, typeof(TakeMessageBackContent));

            content = factory.Create((int)Content.CONTENT_TYPE.CT_RESERVED);
            Assert.IsNull(content);

            content = factory.Create((int)Content.CONTENT_TYPE.ECDH_DEVICE);
            Assert.IsInstanceOfType(content, typeof(ECDHDevice));

            content = factory.Create((int)Content.CONTENT_TYPE.ECDH_USER);
            Assert.IsInstanceOfType(content, typeof(ECDHUser));

            content = factory.Create((int)Content.CONTENT_TYPE.KEY_ECDSA_PRIVATE);
            Assert.IsInstanceOfType(content, typeof(ECDSAKeyPair));

            content = factory.Create((int)Content.CONTENT_TYPE.KEY_ECDSA_PUBLIC);
            Assert.IsInstanceOfType(content, typeof(ECDSAPublicKey));

            content = factory.Create((int)Content.CONTENT_TYPE.KEY_PRIVATE_BOOK);
            Assert.IsInstanceOfType(content, typeof(PrivateKeyBook));

            content = factory.Create((int)Content.CONTENT_TYPE.KEY_PUBLICK_BOOK);
            Assert.IsInstanceOfType(content, typeof(PublicKeyBook));

            content = factory.Create((int)Content.CONTENT_TYPE.MSG_IMAGE);
            Assert.IsInstanceOfType(content, typeof(ImageMessage));

            content = factory.Create((int)Content.CONTENT_TYPE.MSG_TEXT);
            Assert.IsInstanceOfType(content, typeof(TextMessage));

            content = factory.Create((int)Content.CONTENT_TYPE.PLAIN_BINARY);
            Assert.IsInstanceOfType(content, typeof(PlainBinary));

            content = factory.Create((int)Content.CONTENT_TYPE.PLAIN_DEVICE_KEY);
            Assert.IsInstanceOfType(content, typeof(DevicePublicKey));

            content = factory.Create((int)Content.CONTENT_TYPE.PLAIN_IMAGE);
            Assert.IsInstanceOfType(content, typeof(ImageContent));

            content = factory.Create((int)Content.CONTENT_TYPE.PLAIN_USER_KEY);
            Assert.IsInstanceOfType(content, typeof(UserPublicKey));

            content = factory.Create((int)Content.CONTENT_TYPE.SIGN_ECDSA);
            Assert.IsInstanceOfType(content, typeof(ECDSA));

            content = factory.Create((int)Content.CONTENT_TYPE.SYM_AES_WITH_KEY);
            Assert.IsInstanceOfType(content, typeof(AES256WithKey));

            content = factory.Create((int)Content.CONTENT_TYPE.SYM_AES_WITHOUT_KEY);
            Assert.IsInstanceOfType(content, typeof(AES256WithoutKey));
        }
    }
}
