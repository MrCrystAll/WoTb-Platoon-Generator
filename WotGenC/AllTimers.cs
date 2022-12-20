using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WotGenC
{
    public class AllTimers
    {
        public List<Timer> Timers { get; }= new List<Timer>(); 
        
        public event EventHandler<EventArgs> OnAddTimer;

        public void StartAll()
        {
            for(int i = 0; i < Timers.Count; i++)
            {
                Timers[i].Start();
            }
        }

        public AllTimers AddTimer(Timer timer)
        {
            Timers.Add(timer);
            OnAddTimer?.Invoke(this, EventArgs.Empty);
            return this;
        }

        public void PauseAll()
        {
            for (int i = 0; i < Timers.Count; i++)
            {
                Timers[i].Stop();
            }
        }

        public void RestartAll()
        {
            for (int i = 0; i < Timers.Count; i++)
            {
                Timers[i].Restart();
            }
        }

        public void Clear()
        {
            for (int i = 0; i < Timers.Count; i++)
            {
                Timers[i].Stop();
                Timers.RemoveAt(i);
            }
        }

        public void SwitchPauseUnpauseAll()
        {
            for (int i = 0; i < Timers.Count; i++)
            {
                Timers[i].SwitchPauseUnpause();
            }
        }
    }
}
