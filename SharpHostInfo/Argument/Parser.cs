using SharpHostInfo.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpHostInfo.Argument
{
    public static class Parser
    {
        public static ParserResult Parse(IEnumerable<string> args)
        {
            var arguments = new Dictionary<string, string>();
            try
            {
                int idx = 0;
                string key = "";
                foreach (var arg in args)
                {
                    // 长参数情况 比如--target=192.168.1.1 --thread=100
                    if (arg.Contains("--"))
                    {
                        idx = arg.IndexOf("=");
                        if (idx > 0)
                        {
                            key = arg.Substring(0, idx);
                            arguments[key] = arg.Substring(idx + 1);
                            continue;
                        }
                        else 
                        { 
                            Writer.Error("Parameter error"); 
                        }
                    }

                    // 短参数情况 比如-i 192.168.1.1 -t 100
                    if (arg.Contains("-"))
                    {
                        key = arg;
                        arguments[key] = "";
                        continue;
                    }
                    if (!string.IsNullOrEmpty(key))
                    {
                        arguments[key] = arg;
                    }
                }

                return ParserResult.Success(arguments);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return ParserResult.Failure();
            }
        }
    }
}
