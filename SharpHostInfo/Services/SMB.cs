using SharpHostInfo.Lib;
using System;
using System.Text;

namespace SharpHostInfo.Services
{
    public class SMB
    {
        internal void Execute(string ip, int port, int mtime)
        {
            var _SSPKey = new SSPKey();
            _SSPKey.Target = ip;
            _SSPKey.Port = port;
            _SSPKey.Type = "smb";
            string flag = "smb1";
            var response = TimeoutSocket.Send(ip, port, mtime, "smb1");
            if (response.Length == 0)
            {
                flag = "smb2";
                response = TimeoutSocket.Send(ip, port, mtime, "smb2");
            }
            if (response.Length == 0) return;

            NTLMSSPExtract.ParsingSocketStremResponse(ref response, ref _SSPKey);

            if (flag.Equals("smb1"))
            {
                var veraw = Encoding.Default.GetString(response).Split(new String[] { "\0\0\0" }, StringSplitOptions.RemoveEmptyEntries);
                if (veraw.Length == 2)
                {
                    _SSPKey.NativeOs = veraw[0].Replace("\0", "");
                    _SSPKey.NativeLanManager = veraw[1].Replace("\0", "");
                }
            }
            Helpers.SSPKeyOutput.Print(_SSPKey);
        }
    }
}
