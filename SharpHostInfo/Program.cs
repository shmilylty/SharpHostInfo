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
                //HashSet<string> toDetectSMBIPs = new HashSet<string>();
                string[] toDetectSMBIPs = new string[]{};
                // 只进行SMB服务探测的情况
                if (service == "smb")
                {
                    toDetectSMBIPs = ips.ToArray();
                }
                // 其他情况则探测上一个协议探测失败的IP
                else
                {
                    toDetectSMBIPs = failedSet.ToArray();
                    failedSet.Clear();
                }
                var smbCount = new CountdownEvent(toDetectSMBIPs.Length);
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
                // HashSet<string> toDetectWMIIPs = new HashSet<string>();
                string[] toDetectWMIIPs = new string[] { };
                // 只进行SMB服务探测的情况
                if (service == "wmi")
                {
                    toDetectWMIIPs = ips.ToArray();
                }
                // 其他情况则探测上一个协议探测失败的IP
                else
                {
                    toDetectWMIIPs = failedSet.ToArray();
                    failedSet.Clear();
                }
                var WMICount = new CountdownEvent(toDetectWMIIPs.Length);
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

            /*// NBNS服务探测
            if (service.Contains("nbns"))
            {
                Console.WriteLine("");
                Writer.Info("Start NBNS service detection\r\n");
                foreach (string ip in ips)
                {
                    ThreadPool.QueueUserWorkItem(status =>
                    {
                        NBNS nbns = new NBNS();
                        bool success = nbns.Execute(ip, 137, timeout, macdict);
                        if (!success)
                        {
                            failedSet.Add(ip);
                        }
                    });
                }
                // 主线程睡眠一会儿让线程池中的子线程跑起来
                Thread.Sleep(500);
                // 主线程等待所有线程池中的子线程结束
                while (true)
                {
                    *//*
                     GetAvailableThreads()：检索由 GetMaxThreads 返回的线程池线程的最大数目和当前活动数目之间的差值。
                     而GetMaxThreads 检索可以同时处于活动状态的线程池请求的数目。
                     通过最大数目减可用数目就可以得到当前活动线程的数目，如果为零，那就说明没有活动线程，说明所有线程运行完毕。
                     *//*
                    ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int portThreads);
                    ThreadPool.GetAvailableThreads(out int workerThreads, out portThreads);
                    if (maxWorkerThreads - workerThreads == 0)
                    {
                        break;
                    }
                }
            }*/
            /*// NBNS服务探测
            List<ManualResetEvent> nbnsEvents = new List<ManualResetEvent>();
            if (service.Contains("nbns"))   
            {
                Console.WriteLine("");
                Writer.Info("Start NBNS service detection\r\n");
                foreach (string ip in ips)
                {
                    ManualResetEvent mre = new ManualResetEvent(false);
                    nbnsEvents.Add(mre);
                    ThreadPool.QueueUserWorkItem(status =>
                    {
                        NBNS nbns = new NBNS();
                        bool success = nbns.Execute(ip, 137, timeout, macdict);
                        mre.Set();
                        if (!success)
                        {
                            failedSet.Add(ip);
                        }
                    });
                }
                WaitHandle.WaitAll(nbnsEvents.ToArray());
            }*/

            /*// NBNS服务探测
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
            }*/
            // NBNS服务探测
            /*if (service.Contains("nbns"))
            {
                Console.WriteLine("");
                Writer.Info("Start NBNS service detection\r\n");
                ManualResetEvent mre = new ManualResetEvent(false);
                int count = ips.Count;
                foreach (string ip in ips)
                {
                    ThreadPool.QueueUserWorkItem(status =>
                    {
                        count--;
                        NBNS nbns = new NBNS();
                        bool success = nbns.Execute(ip, 137, timeout, macdict);
                        if (!success)
                        {
                            failedSet.Add(ip);
                        }
                        Thread.Sleep(100);
                        if (count == 0)
                        {
                            mre.Set();
                        }
                    });
                }
                mre.WaitOne();
            }*/
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
