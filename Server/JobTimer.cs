using ServerCore;

namespace Server;

public struct JobTimerElem : IComparable<JobTimerElem>
{
    public int    ExecTick;
    public Action Action;
    
    public int CompareTo(JobTimerElem other)
    {
        return other.ExecTick - ExecTick;
    }
}

public class JobTimer
{
    private PriorityQueue<JobTimerElem> _pq = new();
    private object _lock = new();

    public static JobTimer Instance { get; } = new();

    public void Push(Action action, int tickAfter = 0)
    {
        JobTimerElem job;
        job.ExecTick = System.Environment.TickCount + tickAfter;
        job.Action = action;

        lock (_lock)
        {
            _pq.Push(job);
        }
    }

    public void Flush()
    {
        while (true)
        {
            int now = System.Environment.TickCount;

            JobTimerElem job;

            lock (_lock)
            {
                if (_pq.Count == 0)
                    break;

                job = _pq.Peek();
                if (job.ExecTick > now)
                    break;

                _pq.Pop();
            }
            
            job.Action.Invoke();
        }
    }
}