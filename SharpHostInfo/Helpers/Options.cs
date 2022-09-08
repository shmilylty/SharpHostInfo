using SharpHostInfo.Argument;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;

namespace SharpHostInfo.Helpers
{
    public class Options
    {
        public static int SetMaxThreads(ParserContent arguments)
        {
            int maxThreads;
            int thread = Convert.ToInt32(arguments.Thread);
            ThreadPool.GetAvailableThreads(out int workers, out _);
            if (thread < workers)
                maxThreads = thread;
            else
                maxThreads = workers;
            return maxThreads;
        }

        public static Dictionary<string, string> GetMACDict()
        {
            Dictionary<string, string> MACDict = new Dictionary<string, string>()
                {
                    {"000c29","Vmware"},
                    {"005056","Vmware"},
                    {"000569","Vmware"},
                    {"001c14","Vmware"},
                    {"0242ac","Docker"},
                    {"0003ff","HyperV" },
                    {"000D3a","HyperV" },
                    {"00125a","HyperV" },
                    {"00155d","HyperV" },
                    {"0017fa","HyperV" },
                    {"001dd8","HyperV" },
                    {"002248","HyperV" },
                    {"0025ae","HyperV" },
                    {"0050f2","HyperV" },
                    {"444553","HyperV" },
                    {"7Ced8d","HyperV" },
                    {"0010e0","VirtualBox" },
                    {"00144f","VirtualBox" },
                    {"0020f2","VirtualBox" },
                    {"002128","VirtualBox" },
                    {"0021f6","VirtualBox" },
                    {"080027","VirtualBox" },
                    {"001c42","ParallelsVM" },
                    {"00163e","XensourceVM" },
                    {"080020","VirtualBox" },
                    {"0050c2","IEEE ReGi VM" },
                };
            // https://www.wireshark.org/assets/js/manuf.json
            string path = "manuf.json";
            if (!File.Exists(path))
            {
                return MACDict;
            }
            JavaScriptSerializer jss = new JavaScriptSerializer();
            try
            {
                string jsonData = File.ReadAllText(path);
                MACDict = jss.Deserialize<Dictionary<string, string>>(jsonData);
            }
            catch
            {
                return MACDict;
            }
            return MACDict;
        }
    }
}
