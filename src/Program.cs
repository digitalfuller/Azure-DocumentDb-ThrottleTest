namespace Azure.DocumentDb.ThrottleTest
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading.Tasks;
    using Nito.AsyncEx;

    internal class Program
    {
        private static readonly int MaxPending = Convert.ToInt32(ConfigurationManager.AppSettings["MaxPendingTasks"]);

        private static bool ShouldRun => !Console.KeyAvailable;

        private static void LogInsert(int n)
        {
            Logger.Log($"insert #{n}");
        }

        private static void LogProgress(int completed, int cancelled, int faulted, int pending)
        {
            Logger.Log(
                $"insert tasks: pending:{pending}; removed:{{completed={completed}; cancelled={cancelled}; faulted={faulted};}}");
        }

        private static void Main(string[] args)
        {
            AsyncContext.Run(() =>
            {
                try
                {
                    using (var docCli =
                        new MyDocumentClient(ConfigurationManager.AppSettings["DatabaseCollectionName"]))
                    {
                        var tasks = new List<Task>();
                        var n = 0;
                        while (ShouldRun || tasks.Count > 0)
                        {
                            var completed = tasks.RemoveAll(t => t.IsCompleted);
                            var cancelled = tasks.RemoveAll(t => t.IsCanceled);
                            var faulted = tasks.RemoveAll(t => t.IsFaulted);
                            var pending = tasks.Count;
                            LogProgress(completed, cancelled, faulted, pending);

                            if (!ShouldRun || tasks.Count >= MaxPending)
                                Task.WaitAny(tasks.ToArray());

                            if (!ShouldRun) continue;

                            var bigDoc = Doc.BigDoc(++n, 19);
                            LogInsert(n);
                            tasks.Add(docCli.Insert(bigDoc));
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Exception("unhandled exception", e);
                }
                Pause();
            });
        }

        private static void Pause()
        {
            Console.WriteLine("press any key...");
            Console.ReadKey();
        }
    }
}