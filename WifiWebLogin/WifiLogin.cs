namespace WifiWebLogin
{
    public class WifiLogin
    {
        public DetectionTypeEnum DetectionType { get; set; }
        public string RedirectHost { get; set; }

        public string ReadableName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public enum DetectionTypeEnum
    {
        DetectByRedirect,
        /* DetectByDns ....not supported by now */

    }
}