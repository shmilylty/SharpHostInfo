using System;

namespace SharpHostInfo.Helpers
{
    public static class Info
    {
        public static void ShowLogo()
        {
            Console.WriteLine($@"
 __ _                                      _    _____        __       
/ _\ |__   __ _ _ __ _ __   /\  /\___  ___| |_  \_   \_ __  / _| ___  
\ \| '_ \ / _` | '__| '_ \ / /_/ / _ \/ __| __|  / /\/ '_ \| |_ / _ \ 
_\ \ | | | (_| | |  | |_) / __  / (_) \__ \ |_/\/ /_ | | | |  _| (_) |
\__/_| |_|\__,_|_|  | .__/\/ /_/ \___/|___/\__\____/ |_| |_|_|  \___/ 
                    |_|           Version: 0.0.1                                
");
        }

        /// <summary>
        /// 
        /// </summary>
        public static void ShowUsage()
        {
            string usage = $@"
USAGE:
   SharpHostInfo.exe [arguments...]

ARGUMENTS:
   --target,  -i   The IP address of the target (Required)
   --thread,  -t   Number of detection threads (Default: 100)
   --timeout, -m   Thread timeout millisecond (Default: 500)
   --service, -s   Specify which services to use to detect (Default: nbns,smb)

EXAMPLES:
   SharpHostInfo.exe --target=192.168.1.1
   SharpHostInfo.exe --target=ip.txt --threads=40 -s nbns,wmi
   SharpHostInfo.exe --target=192.168.1.1-192.168.255.255 --timeout=1000
   SharpHostInfo.exe -i 192.168.1.1,192.168.1.2 -s nbns
   SharpHostInfo.exe -i 192.168.1.1/20 -t 200 -m 1000 -s wmi
";
            Console.WriteLine(usage);
            Environment.Exit(0);
        }
    }
}
