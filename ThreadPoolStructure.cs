using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDap.Task6.ThreadPoolElements;
using static VDap.Task6.CustomThreadPool;

namespace VDap.Task6
{
    internal class ThreadPoolStructure
    {
        private static readonly object taskLock = new object();
        private int poolStructure;
        private Queue<UserTask> queueTask;
        private Stack<UserTask> stackTask;
        private PriorityQueue<UserTask,Priority> priorityQueueTask;
        public ThreadPoolStructure(PoolStructure poolStructure)
        {
            this.poolStructure = (int)poolStructure;
            switch(poolStructure)
            {
                case PoolStructure.Queue:
                    queueTask = new Queue<UserTask>();
                    break;
                case PoolStructure.Stack:
                    stackTask = new Stack<UserTask>();
                    break;
                case PoolStructure.PriorityQueue:
                    priorityQueueTask = new PriorityQueue<UserTask,Priority>();
                    break;
                default:
                    this.poolStructure = 0;
                    break;
            }
        }
        public void AddTask(UserTask task,Priority priority = Priority.Normal)
        {
            lock(taskLock)
            {
                switch(poolStructure)
                {
                    case 0:
                        queueTask.Enqueue(task);
                        break;
                    case 1:
                        stackTask.Push(task);
                        break;
                    case 2:
                        priorityQueueTask.Add(task, priority);
                        break;
                }
            }
        }
        public UserTask PopTask()
        {
            lock(taskLock)
            {
                switch(poolStructure)
                {
                    case 0:
                        return GetNumberOfTasks() > 0 ? queueTask.Dequeue() : null;
                    case 1:
                        return GetNumberOfTasks() > 0 ? stackTask.Pop() : null;
                    case 2:
                        return  GetNumberOfTasks() > 0 ? priorityQueueTask.Dequeue() : null;
                    default://This section will never execute
                        return null;
                }
            }
        }
        public UserTask FindTask(Guid token)
        {
            lock (taskLock)
            {
                switch (poolStructure)
                {
                    case 0:
                        return queueTask.FirstOrDefault(b => b.Token == token);
                    case 1:
                        return stackTask.FirstOrDefault(b => b.Token == token);
                    default:
                        return null;
                }
            }
        }
        public int GetNumberOfTasks()
        {
            lock (taskLock)
            {
                switch (poolStructure)
                {
                    case 0:
                        return queueTask.Count;
                    case 1:
                        return stackTask.Count;
                    case 2:
                        return priorityQueueTask.Count();
                    default://This section will never execute
                        return 0;
                }
            }
        }
    }
}
