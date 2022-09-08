using SharpHostInfo.Argument;
using System;
using System.Threading;

namespace SharpHostInfo.Helpers
{
    public class Options
    {
        public static int SetMaxThreads(ParserContent arguments)
        {
            int maxThreads;
            int thread = Convert.ToInt32(arguments.Thread);
            ThreadPool.GetAvailableThreads(out int workers, out _);
            if (thread < workers)
                maxThreads = thread;
            else
                maxThreads = workers;
            return maxThreads;
        }
    }
}
