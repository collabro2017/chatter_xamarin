using ChadderLib.Crypto;
using ChadderLib.DataModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChadderTest
{
    [TestFixture]
    public class Crypto : BaseTest
    {
        [Test]
        public async void ECDSA_DER()
        {
            var pair = await ECKeyPair.Generate();
            var pair2 = await ECKeyPair.Generate();

            var content = Encoding.UTF8.GetBytes("Testing my stuff");
            var packed = new ChadderPlainContent(content);
            var signed = await ECDSASignedContent.Sign(packed, pair.Private);

            var serial = signed.Serialize();
            var signed2 = ChadderEncryptedContent.Deserialize(serial);
            Assert.True(signed2 is ECDSASignedContent);
            Assert.True(await (signed2 as ECDSASignedContent).Validate(pair.Public));
            Assert.False(await (signed2 as ECDSASignedContent).Validate(pair2.Public));
        }

        [Test]
        public async void ECDH_KEYS()
        {
            var pair = await ECKeyPair.Generate();
            var pair2 = await ECKeyPair.Generate();
            string myDevice = "Test";
            string yourDevice = Guid.NewGuid().ToString();
            var packed = new ECDHDevice(myDevice, yourDevice);
            var serial = packed.Serialize();
            var packed2 = ChadderEncryptedContent.Deserialize(serial) as ECDHDevice;
            Assert.NotNull(packed2);
            Assert.AreEqual(packed.sourceId, packed2.sourceId);
            Assert.AreEqual(packed.targetId, packed2.targetId);

            var packed3 = new ECDHUser(yourDevice, myDevice);
            serial = packed3.Serialize();
            var packed4 = ChadderEncryptedContent.Deserialize(serial) as ECDHUser;
            Assert.NotNull(packed4);
            Assert.AreEqual(packed3.sourceId, packed4.sourceId);
            Assert.AreEqual(packed3.targetId, packed4.targetId);
        }

        [Test]
        public void Messages()
        {
            var msg = new ChadderMessage();
            msg.Type = MESSAGE_TYPE.TEXT;
            msg.Body = "Testing messages";
            msg.myMessage = true;
            msg.fromId = Guid.NewGuid().ToString();
            msg.messageId = Guid.NewGuid().ToString();

            var packed = new ChadderPackedMessageText(msg);
            var serial = packed.Serialize();

            var packed2 = ChadderEncryptedContent.Deserialize(serial) as ChadderPackedMessageText;
            Assert.NotNull(packed2);
            Assert.AreEqual(packed.Id, packed2.Id);
            Assert.AreEqual(packed.Group, packed2.Group);
            Assert.AreEqual(packed.Time, packed2.Time);
            Assert.AreEqual(packed.From, packed2.From);
            Assert.AreEqual(packed.Body, packed2.Body);

            packed = new ChadderPackedMessageText(msg);
            serial = packed.Serialize();

            packed2 = ChadderEncryptedContent.Deserialize(serial) as ChadderPackedMessageText;
            Assert.NotNull(packed2);
            Assert.AreEqual(packed.Id, packed2.Id);
            Assert.AreEqual(packed.Group, packed2.Group);
            Assert.AreEqual(packed.Time, packed2.Time);
            Assert.AreEqual(packed.From, packed2.From);
            Assert.AreEqual(packed.Body, packed2.Body);
        }
    }
}
