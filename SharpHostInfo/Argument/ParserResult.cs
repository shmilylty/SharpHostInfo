using System.Collections.Generic;

namespace SharpHostInfo.Argument
{
    public class ParserResult
    {
        public bool ParsedOk { get; }
        public ParserContent Arguments { get; }

        private ParserResult(bool parsedOk, Dictionary<string, string> arguments)
        {
            ParsedOk = parsedOk;
            Arguments = new ParserContent(arguments);
        }
        public static ParserResult Success(Dictionary<string, string> arguments)
            => new ParserResult(true, arguments);

        public static ParserResult Failure()
            => new ParserResult(false, null);
    }
}
