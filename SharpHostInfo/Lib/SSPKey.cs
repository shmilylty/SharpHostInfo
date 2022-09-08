using System;

namespace SharpHostInfo.Lib
{
    public class SSPKey
    {
        public string Target { get; set; }
        public int Port { get; set; }
        public string Type { get; set; }
        public int NDR64Syntax { get; set; }
        public int OsBuildNumber { get; set; }
        public byte OsMajor { get; set; }
        public byte OsMinor { get; set; }
        public string NbtComputerName { get; set; }
        public string NbtDomainName { get; set; }
        public string DnsComputerName { get; set; }
        public string DnsDomainName { get; set; }
        public DateTime TimeStamp { get; set; }
        public string NativeOs { get; set; }
        public string NativeLanManager { get; set; }
    }
}
