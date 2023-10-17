using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDap.Task6.Events;
using static VDap.Task6.CustomThreadPool;

namespace VDap.Task6.ThreadPoolElements
{
    public enum Priority
    {
        BelowNormal,
        Normal,
        AboveNormal
    }
    internal class UserTask
    {
        public WorkItem WorkItem;
        public object WorkData;
        public EventHandler<UserExceptionEventArgs> UserTaskException;
        public EventHandler<CompletedTaskEventArgs> CompletedTaskEvent;
        public Guid Token;
        public bool IsCancelled;
    }
}
