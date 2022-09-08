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
                foreach (var argument in args)
                {
                    idx = argument.IndexOf('=');
                    if (idx > 0)
                    {
                        arguments[argument.Substring(0, idx)] = argument.Substring(idx + 1);
                        continue;
                    }
                    idx = argument.IndexOf(' ');
                    if (idx > 0)
                    {
                        arguments[argument.Substring(0, idx)] = argument.Substring(idx + 1);
                        continue;
                    }
                    arguments[argument] = string.Empty;
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
