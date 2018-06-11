using Chadder.Data.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Response
{
    public class BasicResponse
    {
        [JsonIgnore]
        public Exception InnerException { get; set; }
        [JsonProperty(PropertyName="error")]
        public ChadderError Error { get; set; }
        [JsonProperty(PropertyName = "time")]
        public DateTime Time { get; set; }
        public int ProtocolVersion { get; set; }
        public BasicResponse(ChadderError e)
        {
            Time = DateTime.UtcNow;
            Error = e;
        }

        public BasicResponse() : this(ChadderError.OK)
        {
        }

        public void Copy(BasicResponse response)
        {
            InnerException = response.InnerException;
            Error = response.Error;
            Time = response.Time;
            ProtocolVersion = response.ProtocolVersion;
        }
    }
    public class BasicResponse<T> : BasicResponse
    {
        public BasicResponse(ChadderError error) : base(error) { }
        public BasicResponse() : base() { }
        public BasicResponse(T e) { Extra = e; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public T Extra { get; set; }
    }

    public class BasicArrayResponse<T> : BasicResponse
    {
        public BasicArrayResponse() : this(ChadderError.OK)
        {
        }
        public BasicArrayResponse(ChadderError error)
        {
            Error = error;
            List = new List<T>();
        }
        public List<T> List { get; set; }
    }
}
