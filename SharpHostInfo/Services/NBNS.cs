using SharpHostInfo.Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SharpHostInfo.Services
{
    public class NBNS
    {
        static Dictionary<string, string> MACDict = new Dictionary<string, string>();

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

        public static void ResponseResolver(byte[] NameQueryResponse, IPAddress address)
        {
            nb_host_info HostInfo;
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
                Organization = NBNSResolver.MACParser(MAC, MACDict);
                #endregion

                Console.WriteLine(String.Format("{0,-15}", address) + String.Format("{0,-30}", GroupName + '\\' + ComputerName)
                    + String.Format("{0,-6}", ServiceName) + String.Format("{0,-20}", MAC) + String.Format(Organization));
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("{0,-15}", address) + e.Message);
            }
        }

        internal bool Execute(string ip, int port, int timeout, Dictionary<string, string> macdict)
        {
            MACDict = macdict;
            try
            {
                IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
               
                byte[] response = new byte[1024];
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
                    socket.Connect(remoteIPEndPoint);
                    socket.Send(NameQuery);
                    socket.Receive(response);
                }
                if (response.Length != 0)
                {
                    ResponseResolver(response, remoteIPEndPoint.Address);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine("[!] Error: {0} {1}", ip, ex.Message);
                return false;
            }
        }
    }
}
