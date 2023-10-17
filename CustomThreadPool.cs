using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDap.Task6.Events;
using VDap.Task6.ThreadPoolElements;

namespace VDap.Task6
{
    public class CustomThreadPool
    {
        public enum PoolStructure
        {
            Queue,
            Stack,
            PriorityQueue
        }
        private ThreadPoolStructure repoOfTask;
        public UserExceptionEventArgs EventArgs { get; set; } //This is for test purpose
        private static readonly object synLock = new object();
        private static readonly object cancelLock = new object();
        private static readonly object instanceLock = new object();
        private static readonly object addLock = new object();
        private AutoResetEvent continueEvent = new AutoResetEvent(false);
        private AutoResetEvent poolIsNotFull = new AutoResetEvent(false);
        private static CustomThreadPool instance;
        public delegate void WorkItem(object param = null);
        private CustomThreadPool(PoolStructure structure) // private constructor to prevent instantiating
        {
            InitializeStructure(structure);
            InitializePool();
            Thread manager = new Thread(() =>
            {
                PoolManager();
            });
            manager.IsBackground = true;
            manager.Start();
        }
        private struct settings
        {
            public static int MaxPoolSize = Environment.ProcessorCount;
            public static int MinPoolSize = 2;
        }
        private enum state
        {
            NotStarted,
            Completed,
            Running,
            Canceled
        }
        private class workerThread //Worker is here to be assigned by UserTask
        {
            public Thread thread;
            public state taskState;
            public DateTime LastProcess;
            public Guid Token;
        }
        private List<workerThread> pool;
        public static CustomThreadPool GetThreadPool(PoolStructure structure = PoolStructure.Queue)//Default structure is FIFO(QUEUE)
        {
            lock (instanceLock)
            {
                instance = instance ?? new CustomThreadPool(structure);
            }
            return instance;
        }
        private void InitializeStructure(PoolStructure structure)
        {
            repoOfTask = new ThreadPoolStructure(structure);
            return;
        }
        private void InitializePool()//Fill pool with minimum threads
        {
            pool = new List<workerThread>();
            for (int i = 0; i < settings.MinPoolSize; i++)
            {
                workerThread workerThread = new workerThread();
                workerThread.taskState = state.NotStarted;
                workerThread.LastProcess = DateTime.Now;
                lock (synLock)
                {
                    addToPool(workerThread);
                }
            }
            return;
        }
        public Guid AddTask(WorkItem work, object data = null,
                            Priority priority = Priority.Normal,
                            EventHandler<UserExceptionEventArgs> exception = null,
                            EventHandler<CompletedTaskEventArgs> completed = null)
        {
            UserTask task;
            lock (addLock)
            {
                task = new UserTask();
                task.WorkItem = work;
                task.WorkData = data;
                task.Token = Guid.NewGuid();
                task.CompletedTaskEvent += completed;
                task.UserTaskException += exception; 
            }
            repoOfTask.AddTask(task, priority);
            continueEvent.Set();
            return task.Token;
        }
        private void PoolManager()
        {
            while (true)
            {
                continueEvent.WaitOne();
                lock(synLock)
                {
                    poolIsNotFull.WaitOne(1000);
                    if (pool.Count(b => b.taskState == state.NotStarted || b.taskState == state.Completed) == 0)
                    {
                        workerThread newThread = new workerThread();
                        newThread.taskState = state.NotStarted;
                        addToPool(newThread);
                    }
                } 
            CleanPool();
            }
        }
        private void CleanPool()
        {
            lock (synLock)
            {
                if (pool.Count(b => (DateTime.Now - b.LastProcess).TotalSeconds
                    >= ((settings.MaxPoolSize + settings.MinPoolSize) * 5)) > 0)
                {
                    pool.RemoveAll(b => (DateTime.Now - b.LastProcess).TotalSeconds >=
                    (settings.MaxPoolSize + settings.MinPoolSize) * 5);
                }
            }
            return;
        }
        public void TryCancelTask(Guid token)
        {
            lock (cancelLock)
            {
                lock (synLock)
                {
                    var cancelledTask = pool.FirstOrDefault(b => b.taskState != state.Canceled && b.Token == token);
                    cancelledTask.taskState = state.Canceled;
                    repoOfTask.FindTask(token).IsCancelled = true;
                }
            }
            return;
        }
        public void CancelAll()
        {
            lock (cancelLock)
            {
                lock (synLock)
                {
                    foreach (workerThread workerThread in pool)
                    {
                        workerThread.taskState = state.Canceled;
                        workerThread.LastProcess = DateTime.Now;
                        //workerThread.thread.Abort();
                    }
                }
            }
            return;
        }
        private void addToPool(workerThread newThread)
        {
            newThread.thread = new Thread(() =>
            {
                while (true)
                {
                    UserTask task = repoOfTask.PopTask();
                    if (task == null || task.IsCancelled) { continue; }
                    newThread.Token = task.Token;
                    if (newThread.taskState == state.Canceled) { break; }
                    try
                    {
                        newThread.taskState = state.Running;
                        task.WorkItem.Invoke(task.WorkData);
                        newThread.taskState = state.Completed;
                        poolIsNotFull.Set();
                        OnCompletedTask(task.CompletedTaskEvent, new CompletedTaskEventArgs { Token = task.Token, UserData = task.WorkData });
                    }
                    catch (Exception ex)
                    {
                        newThread.taskState = state.NotStarted;
                        poolIsNotFull.Set();
                        OnTaskException(task.UserTaskException,new UserExceptionEventArgs { Exception = ex,Token=task.Token,UserData=task.WorkData });
                    }
                }
            });
            newThread.thread.Start();
            pool.Add(newThread);
            return;
        }
        protected virtual void OnTaskException(EventHandler<UserExceptionEventArgs> exceptionEvent, UserExceptionEventArgs args)
        {
            if (exceptionEvent != null)
                exceptionEvent(this, args);
        }
        protected virtual void OnCompletedTask(EventHandler<CompletedTaskEventArgs> completedEvent, CompletedTaskEventArgs args)
        {
            if (completedEvent != null)
                completedEvent(this, args);
        }
    }
}
