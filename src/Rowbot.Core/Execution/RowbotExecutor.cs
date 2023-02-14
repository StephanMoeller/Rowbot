using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Rowbot.Execution
{
    public static class RowbotExecutor
    {
        public static void Execute(RowSource source, RowTarget target)
        {
            // Columns
            var columnNames = source.InitAndGetColumns();
            target.Init(columns: columnNames);

            // Rows
            var valuesBuffer = new object[columnNames.Length];
            while (source.ReadRow(valuesBuffer))
            {
                target.WriteRow(valuesBuffer);
            }

            source.Complete();
            target.Complete();
        }

        public static void ExecuteDualThreaded(RowSource source, RowTarget target, int maxQueueLength)
        {
            // Columns
            var columnNames = source.InitAndGetColumns();
            target.Init(columns: columnNames);

            // Create buffer pool
            var freeBufferPool = new BlockingCollection<object[]>();
            var filledBufferPool = new BlockingCollection<object[]>();
            for (var i = 0; i < maxQueueLength; i++)
            {
                freeBufferPool.Add(new object[columnNames.Length]);
            }

            Exception readerException = null;
            var readerThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    while (!freeBufferPool.IsAddingCompleted)
                    {
                        // freeBuffers == FILL => filledBuffers
                        var buffer = freeBufferPool.Take();
                        if (source.ReadRow(buffer))
                        {
                            filledBufferPool.Add(buffer);
                        }
                        else
                        {
                            filledBufferPool.CompleteAdding();
                        }
                    }
                }
                catch (Exception ex)
                {
                    readerException = ex;
                    filledBufferPool.CompleteAdding();
                }

            }));
            readerThread.Start();


            try
            {
                while (!filledBufferPool.IsAddingCompleted)
                {
                    // filledBuffers == PROCESS => freeBuffers
                    var buffer = filledBufferPool.Take();
                    target.WriteRow(buffer);
                    freeBufferPool.Add(buffer);
                }
            }
            catch (InvalidOperationException) { }

            if (readerException != null)
            {
                throw readerException;
            }

            target.Complete();
        }


        public static Task ExecuteAsync(RowSource source, RowTarget target)
        {
            return RunAsAsync(() => Execute(source: source, target: target));
        }


        public static Task ExecuteDualThreadedAsync(RowSource source, RowTarget target, int maxQueueLength)
        {
            return RunAsAsync(() => ExecuteDualThreaded(source: source, target: target, maxQueueLength: maxQueueLength));
        }

        private static Task RunAsAsync(Action job)
        {
            var completionResult = new TaskCompletionSource<bool>();

            try
            {
                var thread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        job();
                        completionResult.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        completionResult.SetException(ex);// Error executing job
                    }
                }));
                thread.Start();
            }
            catch (Exception ex)
            {
                // Error starting thread
                completionResult.TrySetException(ex);
            }
            return completionResult.Task;
        }
    }
}
