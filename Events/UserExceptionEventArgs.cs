using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDap.Task6.Events
{
    public class UserExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public object UserData { get; set; }
        public Guid Token { get; set; }
    }
}
