using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDap.Task6.Events
{
    public class CancelTaskEventArgs : EventArgs
    {
        public Guid CancellationToken { get; set; }
    }
}
