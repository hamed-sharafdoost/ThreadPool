using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDap.Task6.Events
{
    public class CompletedTaskEventArgs : EventArgs
    {
        public Guid Token { get; set; }
        public object UserData { get; set; }
    }
}
