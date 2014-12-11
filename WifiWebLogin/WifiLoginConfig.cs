using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WifiWebLogin
{
    [DataContract]
    public class WifiLoginConfig
    {
        public WifiLoginConfig()
        {
            Logins = new List<WifiLogin>();
        }

        [DataMember]
        public List<WifiLogin> Logins { get; set; } 
    }
}