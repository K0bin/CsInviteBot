using System;
using System.Collections.Generic;
using System.Text;

namespace CsInvite.Bot
{
    public class Job
    {
        public TimeSpan Duration { get; set; }
        public Action<Steam> Action { private get; set; }
        private DateTime lastExecutionDate = new DateTime();

        public bool ExecuteIfNecessary(Steam steam)
        {
            if ((DateTime.Now - lastExecutionDate) > Duration)
            {
                Action(steam);
                lastExecutionDate = DateTime.Now;
                return true;
            }
            return false;
        }
    }
}
