using SharpHostInfo.Lib;
using System;

namespace SharpHostInfo.Helpers
{
    public class SSPKeyOutput
    {
        public static int count = 0;
        protected static string Format(string args_1, string args_2) => String.Format("  [>] {0,-22}: {1}\r\n", args_1, args_2);

        public static void Print(SSPKey _SSPKey)
        {
            count += 1;
            var result = String.Empty;
            result += String.Format($"[*] Used {_SSPKey.Type} to detect {_SSPKey.Target}:{_SSPKey.Port}\r\n");

            if (String.IsNullOrEmpty(_SSPKey.NativeOs))
            {
                if (_SSPKey.NDR64Syntax != 0)
                    result += Format("Native OS", $"Windows Version {_SSPKey.OsMajor}.{_SSPKey.OsMinor} Build {_SSPKey.OsBuildNumber} x{_SSPKey.NDR64Syntax}");
                else
                    result += Format("Native OS", $"Windows Version {_SSPKey.OsMajor}.{_SSPKey.OsMinor} Build {_SSPKey.OsBuildNumber}");
            }
            else
            {
                result += Format("Native OS", _SSPKey.NativeOs);
                result += Format("Native LAN Manager", _SSPKey.NativeLanManager);
            }
            if (!string.IsNullOrEmpty(_SSPKey.DnsDomainName))
            {
                result += Format("DNS Domain Name", $"{_SSPKey.DnsDomainName}");
            }
            if (!string.IsNullOrEmpty(_SSPKey.DnsComputerName))
            {
                result += Format("DNS Computer Name", $"{_SSPKey.DnsComputerName}");
            }
            if (!string.IsNullOrEmpty(_SSPKey.NbtDomainName))
            {
                result += Format("NetBIOS Domain Name", $"{_SSPKey.NbtDomainName}");
            }
            result += Format("NetBIOS Computer Name", _SSPKey.NbtComputerName);
            result += Format("Time stamp", _SSPKey.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"));

            Writer.Line(result);
        }
    }
}
