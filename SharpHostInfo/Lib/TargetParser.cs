using SharpHostInfo.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpHostInfo.Lib
{
    public class TargetParser
    {
        public static HashSet<string> Parser(String target)
        {
            HashSet<string> ips = new HashSet<string>();
            // 处理 ipstartIndex
            if (!"".Equals(target))
            {
                // 如果输入文本文件
                if (File.Exists(target))
                {
                    ReadFileToList(target, ref ips);
                }
                // 如果输入单个 IP
                else if (Regex.IsMatch(target, "^([\\w\\-\\.]{1,100}[a-zA-Z]{1,8})$|^(\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3})$"))
                {
                    ips.Clear();
                    ips.Add(target);
                }
                // 如果同时输入多个
                else if (target.Contains(','))
                {
                    ips.Clear();
                    foreach (var list in target.Split(',').ToList())
                        ips.Add(list);
                }
                // 如果输入自定义 IP 段，仅支持 B 段自定义范围（ex: 192.168.1.1-192.168.20.2）
                else if (Regex.IsMatch(target, "^\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\-\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}$"))
                {
                    B(target, ref ips);
                }
                // 如果输入 Prefix(cidr) 格式
                else if (Regex.IsMatch(target, "^([\\w\\-\\.]{1,100}[a-zA-Z]{1,8})$|^(\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\/\\d{1,3})$"))
                {
                    CIDR(target, ref ips);
                }

                if (ips.Count <= 0)
                {
                    Info.ShowUsage();
                    Environment.Exit(0);
                }
            }

            return ips;
        }

        private static void B(String target, ref HashSet<string> ips)
        {
            ips.Clear();
            string[] arrayall = target.Split(new char[]
            {
                        '-'
            });
            if (arrayall.Length == 2)
            {
                string text1 = arrayall[0];
                string text2 = arrayall[1];
                string[] array1 = text1.Split(new char[]
                {
                            '.'
                });
                string[] array2 = text2.Split(new char[]
                {
                            '.'
                });
                if (array1.Length == 4 && array2.Length == 4)
                {
                    int num1_2 = int.Parse(array1[1]);
                    int num1_3 = int.Parse(array1[2]);
                    int num1_4 = int.Parse(array1[3]);

                    int num2_2 = int.Parse(array2[1]);
                    int num2_3 = int.Parse(array2[2]);
                    int num2_4 = int.Parse(array2[3]);

                    if (num2_3 >= num1_3 && num2_3 <= 255 && num2_4 <= 255)
                    {
                        for (int i = num1_3; i <= num2_3; i++)
                        {
                            if (num1_3 == num2_3)
                            {
                                if (num1_4 <= num2_4)
                                {
                                    for (int j = num1_4; j <= num2_4; j++)
                                    {
                                        string item = string.Concat(new string[]
                                        {
                                                    array2[0],
                                                    ".",
                                                    array2[1],
                                                    ".",
                                                    array2[2],
                                                    ".",
                                                    j.ToString()
                                        });
                                        ips.Add(item);
                                    }
                                }
                            }
                            else
                            {
                                int num5 = 0;
                                int num6 = 255;
                                if (i == num1_3)
                                {
                                    num5 = num1_4;
                                }
                                if (i == num2_3)
                                {
                                    num6 = num2_4;
                                }
                                for (int k = num5; k <= num6; k++)
                                {
                                    string item2 = string.Concat(new string[]
                                    {
                                                array2[0],
                                                ".",
                                                array2[1],
                                                ".",
                                                i.ToString(),
                                                ".",
                                                k.ToString()
                                    });
                                    ips.Add(item2);
                                }
                            }
                        }
                    }
                }
            }
        }
        #region cidr parser
        private static void CIDR(String target, ref HashSet<string> list_target)
        {

            uint ip,    /* ip address */
            mask,       /* subnet mask */
            broadcast,  /* Broadcast address */
            network;    /* Network address */
            int bits;
            string[] elements = target.Split(new Char[] { '/' });
            if (elements.Length == 1)
            {
                list_target.Add(target);
            }
            ip = IP2Int(elements[0]);
            bits = Convert.ToInt32(elements[1]);
            mask = ~(0xffffffff >> bits);
            network = ip & mask;
            broadcast = network + ~mask;
            uint usableIps = (bits > 30) ? 0 : (broadcast - network - 1);
            Console.WriteLine("[*] IP range: {0}-{1} ", Int2IP(network), Int2IP(broadcast));
            for (uint i = 1; i < usableIps + 1; i++)
            {
                list_target.Add(Int2IP(network + i));
            }
        }

        public static uint IP2Int(string IPNumber)
        {
            uint ip = 0;
            string[] elements = IPNumber.Split(new Char[] { '.' });
            if (elements.Length == 4)
            {
                ip = Convert.ToUInt32(elements[0]) << 24;
                ip += Convert.ToUInt32(elements[1]) << 16;
                ip += Convert.ToUInt32(elements[2]) << 8;
                ip += Convert.ToUInt32(elements[3]);
            }
            return ip;
        }

        public static string Int2IP(uint ipInt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((ipInt >> 24) & 0xFF).Append(".");
            sb.Append((ipInt >> 16) & 0xFF).Append(".");
            sb.Append((ipInt >> 8) & 0xFF).Append(".");
            sb.Append(ipInt & 0xFF);
            return sb.ToString();
        }
        #endregion

        public static void ReadFileToList(String path, ref HashSet<string> ips)
        {
            ips.Clear();
            FileStream fs_dir = null;
            StreamReader reader = null;
            try
            {
                fs_dir = new FileStream(path, FileMode.Open, FileAccess.Read);

                reader = new StreamReader(fs_dir);

                String lineStr;

                while ((lineStr = reader.ReadLine()) != null)
                {
                    if (!lineStr.Equals(""))
                    {
                        ips.Add(lineStr);
                    }
                }
            }
            catch (Exception e)
            {
                Writer.Failed("An exception occurred while reading the file list!" + e.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (fs_dir != null)
                {
                    fs_dir.Close();
                }
            }
        }
    }
}
