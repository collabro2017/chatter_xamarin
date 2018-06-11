using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data
{
    public enum DeviceType
    {
        RESERVED,
        WP81,
        ANDROID,
        IOS,
        WP8
    }

    public enum RelationshipType
    {
        RESERVED,
        FRIENDS,
        BLOCKED,
        NOT_FRIENDS,
        DELETED
    }

    public enum KeyPurpose : short
    {
        RESERVED,
        MASTER_KEY,
        DH_SEND,
        DH_RECEIVE,
    }

    public enum PublicKeyStatus
    {
        NOT_VERIFIED,
        VERIFIED,
        CHANGED
    }
}
