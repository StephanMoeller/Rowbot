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
            var sourceBufferPool = new BlockingCollection<object[]>();
            var targetBufferPool = new BlockingCollection<object[]>();
            for (var i = 0; i < maxQueueLength; i++)
            {
                sourceBufferPool.Add(new object[columnNames.Length]);
            }

            Exception readerException = null;
            var readerThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    while (!sourceBufferPool.IsAddingCompleted)
                    {
                        // freeBuffers == FILL => filledBuffers
                        var buffer = sourceBufferPool.Take();
                        if (source.ReadRow(buffer))
                        {
                            targetBufferPool.Add(buffer);
                        }
                        else
                        {
                            targetBufferPool.CompleteAdding();
                        }
                    }
                }
                catch (Exception ex)
                {
                    readerException = ex;
                    targetBufferPool.CompleteAdding();
                }

            }));
            readerThread.Start();


            try
            {
                while (!targetBufferPool.IsAddingCompleted)
                {
                    // filledBuffers == PROCESS => freeBuffers
                    var buffer = targetBufferPool.Take();
                    target.WriteRow(buffer);
                    sourceBufferPool.Add(buffer);
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
