using Chadder.Data.Keys;
using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Base
{
    public interface IContentSerializable
    {
        void Serialize(DataSerializer writer);
    }
    public interface IContentWrapper
    {
        Task<Content> GetContent(IContentContext c);
    }
    public interface IIdentifySource
    {
        string SourceId { get; }
    }
    public interface IIdentity
    {
        PublicKeyBook PublicKeyBook { get; }
    }
    public interface IContentContext
    {
        Task<IIdentity> GetUser(string id);
        Task<IIdentity> GetDevice(string id);
        Task<PrivateKeyBook> GetUserKeyPair();
        Task<PrivateKeyBook> GetDeviceKeyPair();
        Task<string> GetMyUserId();
        Task<string> GetMyDeviceId();
    }
}
