using SharpHostInfo.Argument;
using SharpHostInfo.Helpers;
using SharpHostInfo.Lib;
using SharpHostInfo.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SharpHostInfo
{
    class Program
    {
        private static void MainExecute(ParserContent parsedArgs)
        {
            Helpers.Info.ShowLogo();
            Writer.Info($"Detect target: {parsedArgs.Target}");
            Writer.Info($"Detect Service: {parsedArgs.Service}");
            Writer.Info($"Detect thead: {parsedArgs.Thread}");
            Writer.Info($"Detect timeout: {parsedArgs.Timeout}ms");

            int timeout = Convert.ToInt32(parsedArgs.Timeout);
            HashSet<string> ips = TargetParser.Parser(parsedArgs.Target);
            if (ips.Count < 1)
            {
                Writer.Error("The parsed detection target is empty");
            }
            String service = parsedArgs.Service.ToLower();

            Dictionary<string, string> macdict = Options.GetMACDict();

            // 开始NBNS服务探测
            if (service.Contains("nbns"))
            {
                Console.WriteLine("");
                Writer.Info("Start NBNS service detection\r\n");
                NBNS nbns = new NBNS();
                nbns.Execute(ips, 137, timeout, macdict);
            }

            ThreadPool.SetMaxThreads(Helpers.Options.SetMaxThreads(parsedArgs), 1);

            // 开始SMB服务探测
            var smbCount = new CountdownEvent(ips.Count);
            if (parsedArgs.Service.Contains("smb"))
            {
                Console.WriteLine("");
                Writer.Info("Start SMB service detection\r\n");
                foreach (string ip in ips)
                {
                    ThreadPool.QueueUserWorkItem(status =>
                    {
                        if (parsedArgs.Service.Contains("smb"))
                        {

                            SMB smb = new SMB();
                            smb.Execute(ip, 445, timeout);
                        }
                        smbCount.Signal();
                    });
                }
                smbCount.Wait();
            }

            // 开始WMI服务探测
            var WMICount = new CountdownEvent(ips.Count);
            if (parsedArgs.Service.Contains("wmi"))
            {
                Console.WriteLine("");
                Writer.Info("Start WMI service detection\r\n");
                foreach (string ip in ips)
                {
                    ThreadPool.QueueUserWorkItem(status =>
                    {
                        if (parsedArgs.Service.Contains("wmi"))
                        {
                            WMI wmi = new WMI();
                            wmi.Execute(ip, 135, timeout);
                        }
                        WMICount.Signal();
                    });
                }
                WMICount.Wait();
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 1 || args.Contains("-h") || args.Contains("--help"))
            {
                Helpers.Info.ShowLogo();
                Helpers.Info.ShowUsage();
                return;
            }
            // 尝试解析命令行参数
            var parsed = Parser.Parse(args);
            if (parsed.ParsedOk == false)
            {
                Helpers.Info.ShowLogo();
                Helpers.Info.ShowUsage();
                return;
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            MainExecute(parsed.Arguments);
            stopwatch.Stop();
            TimeSpan timespan = stopwatch.Elapsed;
            Writer.Info($"Time taken: {timespan.TotalSeconds}s");
        }
    }
}
