using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PollingTimer
{
    public delegate void UnitProcessor(Dictionary<string, object> data);
    public delegate bool PollingStoper(Dictionary<string, object> data);

    public class PollingTimer
    {
        #region Fields

        private Timer timer;
        private Dictionary<string, object> data;
        private bool done = false;
        private long dueTime = 0;
        private long period = 1000;

        private UnitProcessor PrePolling;
        private UnitProcessor AfterPolling;
        private PollingStoper OnPolling;

        #endregion

        #region Constructors

        public PollingTimer(UnitProcessor prePolling, PollingStoper polling, UnitProcessor afterPolling, Dictionary<string, object> data)
        {
            this.PrePolling = prePolling == null ? _data => { } : prePolling;
            this.AfterPolling = afterPolling == null ? _data => { } : afterPolling;
            this.OnPolling = polling;
            this.data = data == null ? new Dictionary<string, object>() : data;

            if (this.OnPolling == null)
            {
                throw new ArgumentException("The polling event can not be null.");
            }
        }

        public PollingTimer(UnitProcessor prePolling, PollingStoper polling, UnitProcessor afterPolling, Dictionary<string, object> data, long dueTime, long period)
            : this(prePolling, polling, afterPolling, data)
        {
            this.dueTime = dueTime;
            this.period = period;
        }

        #endregion

        #region Type Specific Methods

        public void Start()
        {
            this.PrePolling(this.data);

            //这里设置Timeout.Infinite只是为了timer只调用一次
            this.timer = new Timer(this.TimeoutCallback, this, this.dueTime, Timeout.Infinite);
        }

        public bool Done()
        {
            return this.done;
        }

        #endregion

        #region Helper

        private void Stop()
        {
            this.done = true;
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
            this.timer.Dispose();
        }

        private void TimeoutCallback(object state)
        {
            PollingTimer timer = state as PollingTimer;

            if (!timer.done)
            {
                if (timer.OnPolling(timer.data))
                {
                    timer.Stop();

                    timer.AfterPolling(timer.data);
                }
                else
                {
                    //由于事件仍需继续，这里改变timer，让timer在period时间之后再次触发一次。
                    //这里这样的做法是为了防止OnPolling执行的时间过长，导致timer已经到了再次触发的时间再次触发，最后导致多个线程池的线程同时执行TimeoutCallback这个回调方法。
                    timer.timer.Change(timer.period, Timeout.Infinite);
                }
            }
        }

        #endregion
    }
}