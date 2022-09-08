using System.Collections.Generic;

namespace SharpHostInfo.Argument
{
    public class ParserContent
    {
        public string Service { get; }
        public string Target { get; }
        public string Thread { get; }
        public string Timeout { get; }
        /*public string Port { get; }*/

        public ParserContent(Dictionary<string, string> arguments)
        {
            Service = ArgumentParser(arguments, "-s");
            if (string.IsNullOrEmpty(Service))
            {
                Service = ArgumentParser(arguments, "--service");
            }
            if (string.IsNullOrEmpty(Service))
            {
                Service = "nbns,smb";
            }
            Target = ArgumentParser(arguments, "-i");
            if (string.IsNullOrEmpty(Target))
            {
                Target = ArgumentParser(arguments, "--target");
            }
            if (string.IsNullOrEmpty(Target))
            {
                Target = ArgumentParser(arguments, "--ip");
            }
            Thread = ArgumentParser(arguments, "-t");
            if (string.IsNullOrEmpty(Thread))
            {
                Thread = ArgumentParser(arguments, "--thread");
            }
            if (string.IsNullOrEmpty(Thread))
            {
                Thread = "100";
            }
            Timeout = ArgumentParser(arguments, "-m");
            if (string.IsNullOrEmpty(Timeout))
            {
                Timeout = ArgumentParser(arguments, "--timeout");
            }
            if (string.IsNullOrEmpty(Timeout))
            {
                Timeout = "500";
            }
            /*Port = ArgumentParser(arguments, "--port");*/

        }

        private string ArgumentParser(Dictionary<string, string> arguments, string flag)
        {
            if (arguments.ContainsKey(flag) && !string.IsNullOrEmpty(arguments[flag]))
            {
                return arguments[flag];
            }
            return null;
        }
    }
}
