using System;
using System.Linq;
using System.Net.Sockets;

namespace SharpHostInfo.Lib
{
    public class TimeoutSocket
    {
        public static byte[] Send(string ip, int port, int mtime, string type)
        {
            byte[] buffer = new byte[2048];
            byte[] response = new byte[] { };

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            /*socket.SendTimeout = mtime;
            socket.ReceiveTimeout = mtime;*/
            try
            {
                /*socket.Connect(ip, port);*/
                int length;
                IAsyncResult result = socket.BeginConnect(ip, port, null, null);

                bool success = result.AsyncWaitHandle.WaitOne(mtime, true);

                if (socket.Connected)
                {
                    switch (type)
                    {
                        case "smb1":
                            socket.Send(NTLMSSPBuffer.smb_buffer_v1_1);
                            /*发送后必须要接收*/
                            socket.Receive(buffer);
                            socket.Send(NTLMSSPBuffer.smb_buffer_v1_2);
                            break;
                        case "smb2":
                            socket.Send(NTLMSSPBuffer.smb_buffer_v2_1);
                            socket.Receive(buffer);
                            socket.Send(NTLMSSPBuffer.smb_buffer_v2_2);
                            socket.Receive(buffer);
                            socket.Send(NTLMSSPBuffer.smb_buffer_v2_3);
                            break;
                        case "wmi0":
                            socket.Send(NTLMSSPBuffer.dcerpc_buffer_v2);
                            break;
                        case "wmi1":
                            socket.Send(NTLMSSPBuffer.dcerpc_buffer_v1);
                            break;
                    }
                    length = socket.Receive(buffer);
                    response = buffer.Take(length).ToArray();
                    socket.Close();
                }
                else
                {
                    throw new TimeoutException("TimeOut Exception");
                }
            }
            catch
            {
                /*todo 添加调试异常输出*/
                socket.Close();
            }

            return response;
        }
    }
}