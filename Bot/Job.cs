using System;
using System.Collections.Generic;
using System.Text;

namespace CsInvite.Bot
{
    public class Job
    {
        public TimeSpan Duration { get; set; }
        public Action Action { private get; set; }
        private DateTime lastExecutionDate = new DateTime();

        public bool ExecuteIfNecessary()
        {
            if ((DateTime.Now - lastExecutionDate) > Duration)
            {
                Action();
                lastExecutionDate = DateTime.Now;
                return true;
            }
            return false;
        }
    }
}
