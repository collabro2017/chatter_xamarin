using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data
{
    public class ImageContent : PlainBinary
    {
        public override int Type { get { return (int)CONTENT_TYPE.PLAIN_IMAGE; } }
    }
}
