namespace Azure.DocumentDb.ThrottleTest
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;

    internal class Logger
    {
        public static void Log(string msg)
        {
            Console.WriteLine("LOG:" + msg);
        }

        public static void Error(string msg)
        {
            Console.WriteLine("ERROR:" + msg);
        }

        public static void Exception(string msg, Exception e)
        {
            while (true)
            {
                if (e == null)
                {
                    Error(msg + ": null");
                    return;
                }

                Error(msg + ":" + e.Message);
                var dce = e as DocumentClientException;
                if (dce != null)
                    Exception(
                        $"!!!DOCUMENTCLIENTEXCEPTION!!! StatusCode={dce.StatusCode}; RetryAfter={dce.RetryAfter}; RequestCharge={dce.RequestCharge};",
                        dce.InnerException);

                var tce = e as TaskCanceledException;
                if (tce != null)
                    Exception("TaskCancelledException.InnerException", e.InnerException);

                var ae = e as AggregateException;
                if (ae != null)
                {
                    msg = "AggregateException.InnerException";
                    e = ae.InnerException;
                    continue;
                }
                break;
            }
        }
    }
}