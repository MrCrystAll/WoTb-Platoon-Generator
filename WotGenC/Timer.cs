using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using System.Threading;
using Windows.UI.Xaml;

namespace WotGenC
{
    public class Timer : INotifyPropertyChanged
    {

        private System.Timers.Timer _timer;
        private int oldSec, oldMin;
        private Player Joueur;
        
        private int _seconds;
        public int Seconds
        {
            get => _seconds;
            set
            {
                _seconds = value;
                OnPropertyChanged(nameof(Seconds));
            }
        }
        private int _minutes;
        public int Minutes
        {
            get => _minutes;
            set
            {
                _minutes = value;
                OnPropertyChanged(nameof(Minutes));
            }
        }
        int hours;
        DateTime dateStarted;

        private int _waitingTime;
        bool paused;
        private bool isLaunched;        // True when timer launched

        public Timer(int minutes, int seconds, Player j, int waitingTime)
        {
            _waitingTime = waitingTime;
            Joueur = j;
            Seconds = seconds;
            _timer = new System.Timers.Timer();
            
            Minutes = minutes;
            if (Seconds == 60)
            {
                Minutes++;
                Seconds = 0;
            }

            oldSec = Seconds;
            oldMin = Minutes;
            hours = 0;
            isLaunched = false;
            paused = true;
        }

        public void Start()
        {
            isLaunched = true;
            paused = false;
            _timer.Interval = 1000;
            _timer.Enabled = true;
            _timer.Start();
            _timer.Elapsed += TimerOnTick;
        }

        private void TimerOnTick(object sender, object e)
        {

            if (Seconds == 0)
            {
                Minutes--;
                Seconds = 59;
            }
            else
            {
                Seconds--;
            }

            if (Minutes != -1) return;
            Minutes = 0;
            Seconds = 0;

            int min = oldMin;
            int sec = oldSec + _waitingTime;
            if (sec >= 60)
            {
                min++;
                sec %= 60;
            }

            new ToastContentBuilder().AddText($"Careful {Joueur.Pseudo} : Timer reached {6 - oldMin}:{60 - oldSec:D2} : Got to stop for {_waitingTime} seconds (Until {6 - min}:{60 - sec:D2})").Show();
            Stop();
        }

        public void Stop()
        {
            _timer.Stop();
            isLaunched = false;
            _timer.Close();
            paused = true;
        }

        public void SwitchPauseUnpause()
        {
            if(paused)
                Start();
            if(!paused)
                Stop();
        }

        public void Restart()
        {
            Stop();
            isLaunched = false;
            paused = false;
            Seconds = oldSec;
            Minutes = oldMin;
            Start();

        }

        public bool IsChronoLaunched
        {
            get  //get accessor method
            {
                return isLaunched;
            }
            
        }


        public override String ToString()
        {
   

            DateTime DateNow = DateTime.Now;
            TimeSpan Difference = DateNow - dateStarted;

            StringBuilder builder = new StringBuilder(11);
            builder.Append(Difference.Hours.ToString("d2"));
            builder.Append(":");
            builder.Append(Difference.Minutes.ToString("d2"));
            builder.Append(":");
            builder.Append(Difference.Seconds.ToString("d2"));
            builder.Append(":");
            builder.Append((Difference.Milliseconds.ToString("d2")).Substring(0,2));

            return builder.ToString();
        }

        public String Now()
        {
            DateTime dateNow = DateTime.Now;
            StringBuilder builder = new StringBuilder(8);
            builder.Append(dateNow.Hour.ToString("d2"));
            builder.Append(":");
            builder.Append(dateNow.Minute.ToString("d2"));
            builder.Append(":");
            builder.Append(dateNow.Second.ToString("d2"));

            return builder.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
