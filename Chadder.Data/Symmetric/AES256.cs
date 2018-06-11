using Chadder.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Symmetric
{
    public abstract class AES256 : AESAbstract
    {
        protected override uint IV_size { get { return 16; } }
    }

    public class AES256WithoutKey : AES256
    {
        public override int Type { get { return (int)CONTENT_TYPE.SYM_AES_WITHOUT_KEY; } }
        public byte[] Key { get; set; }
        public override async Task<byte[]> GetKey(IContentContext context)
        {
            return Key;
        }
        static public async Task<AES256WithoutKey> Encrypt(byte[] keyData, byte[] content)
        {
            var result = new AES256WithoutKey()
            {
                Key = keyData
            };
            await result.Create(keyData, content);
            return result;
        }
        static public Task<AES256WithoutKey> Encrypt(byte[] keyData, Content content)
        {
            return Encrypt(keyData, content.Serialize());
        }
    }
}
