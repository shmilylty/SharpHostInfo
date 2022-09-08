using SharpHostInfo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SharpHostInfo.Services
{
    class NBNS
    {
        #region NetBIOS NameQuery
        static byte[] NameQuery =
        {
            0x00,0x00,0x00,0x00,0x00,0x01,0x00,0x00,
            0x00,0x00,0x00,0x00,0x20,0x43,0x4b,0x41,
            0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,
            0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,
            0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,
            0x41,0x41,0x41,0x41,0x41,0x00,0x00,0x21,
            0x00,0x01
        };
        #endregion

        #region  UDP Client Config Params
        static bool IsUdpcRecvStart = false;
        static UdpClient UdpClient = null;
        static IPEndPoint RemoteIPEndPoint = null;
        #endregion

        public static void StartReceive()
        {
            RemoteIPEndPoint = new IPEndPoint(IPAddress.Any, 137);
            UdpState udpState = new UdpState(UdpClient, RemoteIPEndPoint);
            IsUdpcRecvStart = true;
            UdpClient.BeginReceive(RecieveMessage, udpState);
        }

        public static void NameQueryResponseResolver(byte[] NameQueryResponse, IPAddress address)
        {
            nb_host_info HostInfo = new nb_host_info();
            try
            {
                HostInfo = NBNSResolver.NBNSParser(NameQueryResponse, NameQueryResponse.Length);

                #region identify  groupname\computername and service
                string ComputerName = "";
                string GroupName = "";
                bool IsComputerName = true;
                char chrService = '\xff';
                string ServiceName = "";
                for (int i = 0; i < HostInfo.header.number_of_names; i++)
                {
                    chrService = HostInfo.names[i].ascii_name.AsQueryable().Last();
                    ushort flag = HostInfo.names[i].rr_flags;
                    if (chrService == '\x00' && IsComputerName && ((flag & 0x8000) == 0))
                    {
                        ComputerName = new string(HostInfo.names[i].ascii_name).Replace('\0', ' ').Trim();
                        IsComputerName = false;
                    }
                    else if (chrService == '\x00')
                    {
                        GroupName = new string(HostInfo.names[i].ascii_name).Replace('\0', ' ').Trim();
                    }
                    else if (chrService == '\x1C')
                    {
                        ServiceName = "DC";
                    }

                }
                #endregion

                string MAC = BitConverter.ToString(HostInfo.footer.adapter_address);

                #region identify device via mac

                string Organization = "";
                Organization = NBNSResolver.MACParser(MAC);
                #endregion

                Console.WriteLine(String.Format("{0,-15}", address) + String.Format("{0,-30}", GroupName + '\\' + ComputerName)
                    + String.Format("{0,-6}", ServiceName) + String.Format("{0,-20}", MAC) + String.Format(Organization));
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("{0,-15}", address) + e.Message);
            }
        }

        public static void RecieveMessage(IAsyncResult asyncResult)
        {
            UdpState udpState = (UdpState)asyncResult.AsyncState;
            if (udpState != null)
            {
                UdpClient udpClient = udpState.UdpClient;
                IPEndPoint iPEndPoint = udpState.IP;
                if (IsUdpcRecvStart)
                {
                    byte[] NameQueryResponse = udpClient.EndReceive(asyncResult, ref iPEndPoint);
                    udpClient.BeginReceive(RecieveMessage, udpState);
                    if (NameQueryResponse.Length != 0)
                    {
                        NameQueryResponseResolver(NameQueryResponse, iPEndPoint.Address);
                    }
                }
            }
        }

        public class UdpState
        {
            private UdpClient udpclient = null;
            public UdpClient UdpClient
            {
                get { return udpclient; }
            }
            private IPEndPoint ip;
            public IPEndPoint IP
            {
                get { return ip; }
            }
            public UdpState(UdpClient udpclient, IPEndPoint ip)
            {
                this.udpclient = udpclient;
                this.ip = ip;
            }
        }


        internal void Execute(HashSet<string> ips, int port, int mtime)
        {
            try
            {
                UdpClient = new UdpClient(0);
                uint IOC_IN = 0x80000000;
                uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                UdpClient.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
                /*Console.FormatSSPKey("[*]Start udp client ...");*/
                StartReceive();

                foreach (string ip in ips)
                {
                    IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), 137);
                    UdpClient.Send(NameQuery, NameQuery.Length, remoteIPEndPoint);
                }

                // 睡眠3秒 等待关闭UdpClient
                Thread.Sleep(3000);
                IsUdpcRecvStart = false;
                UdpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[!] Error: {0}", ex.Message);
            }
        }
    }
}
