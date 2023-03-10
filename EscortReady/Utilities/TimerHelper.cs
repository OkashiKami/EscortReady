namespace EscortsReady
{
    public class TimerHelper : IDisposable
    {
        private Timer _timer;
        private readonly object _threadLock = new object();

        public event Action<Timer, object> TimerEvent;

        public void Start(TimeSpan timerInterval, bool triggerAtStart = false,
            object state = null)
        {
            Stop();
            _timer = new Timer(Timer_Elapsed, state,
                Timeout.Infinite, System.Threading.Timeout.Infinite);

            if (triggerAtStart)
            {
                _timer.Change(TimeSpan.FromTicks(0), timerInterval);
            }
            else
            {
                _timer.Change(timerInterval, timerInterval);
            }
        }

        public void Stop(TimeSpan timeout = default)
        {
            if (timeout == default)
                timeout = TimeSpan.FromMinutes(2);

            // Wait for timer queue to be emptied, before we continue
            // (Timer threads should have left the callback method given)
            // - http://woowaabob.blogspot.dk/2010/05/properly-disposing-systemthreadingtimer.html
            // - http://blogs.msdn.com/b/danielvl/archive/2011/02/18/disposing-system-threading-timer.aspx
            lock (_threadLock)
            {
                if (_timer != null)
                {
                    ManualResetEvent waitHandle = new ManualResetEvent(false);
                    if (_timer.Dispose(waitHandle))
                    {
                        // Timer has not been disposed by someone else
                        if (!waitHandle.WaitOne(timeout))
                            throw new TimeoutException("Timeout waiting for timer to stop");
                    }
                    waitHandle.Close();   // Only close if Dispose has completed succesful
                    _timer = null;
                }
            }
        }

        public void Dispose()
        {
            Stop();
            TimerEvent = null;
        }

        void Timer_Elapsed(object state)
        {
            // Ensure that we don't have multiple timers active at the same time
            // - Also prevents ObjectDisposedException when using Timer-object
            //   inside this method
            // - Maybe consider to use _timer.Change(interval, Timeout.Infinite)
            //   (AutoReset = false)
            if (Monitor.TryEnter(_threadLock))
            {
                try
                {
                    if (_timer == null)
                        return;

                    Action<Timer, object> timerEvent = TimerEvent;
                    if (timerEvent != null)
                    {
                        timerEvent(_timer, state);
                    }
                }
                finally
                {
                    Monitor.Exit(_threadLock);
                }
            }
        }
    }
}
