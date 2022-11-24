using SharpHostInfo.Lib;
using System;

namespace SharpHostInfo.Services
{
    public class WMI
    {
        public static string CommandName => "wmi";

        private static int ParsingNDR64Syntax(byte[] responseBuffer)
        {
            if (responseBuffer.Length == 0) return 0;
            var NDR64SyntaxStr = BitConverter.ToString(responseBuffer).Replace("-", "");
            return NDR64SyntaxStr.Contains("33057171BABE37498319B5DBEF9CCC36") ? 64 : 86;
        }

        internal bool Execute(string host, int port, int mtime)
        {
            var _SSPKey = new SSPKey();
            _SSPKey.Target = host;
            _SSPKey.Port = port;
            _SSPKey.Type = "wmi";

            var response = TimeoutSocket.Send(host, port, mtime, "wmi0");
            _SSPKey.NDR64Syntax = ParsingNDR64Syntax(response);

            response = TimeoutSocket.Send(host, port, mtime, "wmi1");
            if (response.Length == 0) return false;

            NTLMSSPExtract.ParsingSocketStremResponse(ref response, ref _SSPKey);
            Helpers.SSPKeyOutput.Print(_SSPKey);
            return true;
        }
    }
}
