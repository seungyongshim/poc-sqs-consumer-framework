using System.Collections.Concurrent;

namespace WebApplication1;

public sealed class SingleThreadTaskScheduler : TaskScheduler, IDisposable
{
    public Thread Thread { get; }
    public CancellationTokenSource CancellationTokenSource { get; } = new();
    public BlockingCollection<Task> Tasks { get; } = new();

    public SingleThreadTaskScheduler(ApartmentState apartmentState = ApartmentState.STA) : this(null, apartmentState)
    {
    }

    public SingleThreadTaskScheduler(Action initAction, ApartmentState apartmentState = ApartmentState.STA)
    {
        if (apartmentState is not ApartmentState.MTA and not ApartmentState.STA)
        {
            throw new ArgumentException("apartementState");
        }

        Thread = new Thread(ThreadStart)
        {
            IsBackground = true
        };
        Thread.TrySetApartmentState(apartmentState);
        Thread.Start();
    }


    public void Wait()
    {
        if (CancellationTokenSource.IsCancellationRequested)
        {
            throw new TaskSchedulerException("Cannot wait after disposal.");
        }

        Tasks.CompleteAdding();
        Thread.Join();

        CancellationTokenSource.Cancel();
    }

    public void Dispose()
    {
        if (CancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        Tasks.CompleteAdding();
        CancellationTokenSource.Cancel();
    }

    protected override void QueueTask(Task task)
    {
        VerifyNotDisposed();

        Tasks.Add(task, CancellationTokenSource.Token);
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        VerifyNotDisposed();

        if (Thread != Thread.CurrentThread)
        {
            return false;
        }

        if (CancellationTokenSource.Token.IsCancellationRequested)
        {
            return false;
        }

        TryExecuteTask(task);
        return true;
    }

    protected override IEnumerable<Task> GetScheduledTasks()
    {
        VerifyNotDisposed();

        return Tasks.ToArray();
    }

    private void ThreadStart()
    {
        try
        {
            var token = CancellationTokenSource.Token;

            foreach (var task in Tasks.GetConsumingEnumerable(token))
            {
                TryExecuteTask(task);
            }
        }
        finally
        {
            Tasks.Dispose();
        }
    }

    private void VerifyNotDisposed()
    {
        if (CancellationTokenSource.IsCancellationRequested)
        {
            throw new ObjectDisposedException(typeof(SingleThreadTaskScheduler).Name);
        }
    }
}
