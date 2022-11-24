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
            Info.ShowLogo();
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

            ThreadPool.SetMaxThreads(Options.SetMaxThreads(parsedArgs), 1);
            HashSet<string> failedSet = new HashSet<string>();

            // NBNS服务探测
            if (service.Contains("nbns"))
            {
                var nbnsCount = new CountdownEvent(ips.Count);
                Console.WriteLine("");
                Writer.Info("Start NBNS service detection\r\n");
                foreach (string ip in ips)
                {
                    ThreadPool.QueueUserWorkItem(status =>
                    {
                        NBNS nbns = new NBNS();
                        bool success = nbns.Execute(ip, 137, timeout, macdict);
                        nbnsCount.Signal();
                        if (!success)
                        {
                            failedSet.Add(ip);
                        }
                    });
                }
                nbnsCount.Wait();
            }

            // SMB服务探测
            if (service.Contains("smb"))
            {
                HashSet<string> toDetectSMBIPs = new HashSet<string>();
                //string[] toDetectSMBIPs = new string[] { };
                // 只进行SMB服务探测的情况
                if (service == "smb")
                {
                    toDetectSMBIPs = ips.ToHashSet();
                }
                // 其他情况则探测上一个协议探测失败的IP
                else
                {
                    toDetectSMBIPs = failedSet.ToHashSet();
                    failedSet.Clear();
                }
                var smbCount = new CountdownEvent(toDetectSMBIPs.Count);
                Console.WriteLine("");
                Writer.Info("Start SMB service detection\r\n");
                foreach (string ip in toDetectSMBIPs)
                {
                    ThreadPool.QueueUserWorkItem(status =>
                    {
                        SMB smb = new SMB();
                        bool success = smb.Execute(ip, 445, timeout);
                        smbCount.Signal();
                        if (!success)
                        {
                            failedSet.Add(ip);
                        }
                    });
                }
                smbCount.Wait();
            }

            // WMI服务探测
            if (parsedArgs.Service.Contains("wmi"))
            {
                HashSet<string> toDetectWMIIPs = new HashSet<string>();
                // 只进行SMB服务探测的情况
                if (service == "wmi")
                {
                    toDetectWMIIPs = ips.ToHashSet();
                }
                // 其他情况则探测上一个协议探测失败的IP
                else
                {
                    toDetectWMIIPs = failedSet.ToHashSet();
                    failedSet.Clear();
                }
                var WMICount = new CountdownEvent(toDetectWMIIPs.Count);
                Console.WriteLine("");
                Writer.Info("Start WMI service detection\r\n");
                foreach (string ip in toDetectWMIIPs)
                {
                    ThreadPool.QueueUserWorkItem(status =>
                    {
                        if (parsedArgs.Service.Contains("wmi"))
                        {
                            WMI wmi = new WMI();
                            bool success = wmi.Execute(ip, 135, timeout);
                            if (!success)
                            {
                                failedSet.Add(ip);
                            }
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
                Info.ShowLogo();
                Info.ShowUsage();
                return;
            }
            // 尝试解析命令行参数
            var parsed = Parser.Parse(args);
            if (parsed.ParsedOk == false)
            {
                Info.ShowLogo();
                Info.ShowUsage();
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
