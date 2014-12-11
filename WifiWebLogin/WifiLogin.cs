using System.Runtime.Serialization;

namespace WifiWebLogin
{
    [DataContract]
    public class WifiLogin
    {

        [DataMember]
        public DetectionTypeEnum DetectionType { get; set; }

        [DataMember]
        public string RedirectHost { get; set; }


        [DataMember]
        public string ReadableName { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }
    }

    public enum DetectionTypeEnum
    {
        DetectByRedirect,
        /* DetectByDns ....not supported by now */

    }
}