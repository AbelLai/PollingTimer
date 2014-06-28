using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PollingTimer.Async
{
    #region Processor Handler

    public delegate void AsyncProcessor(Dictionary<string, object> data);
    public delegate void AsyncPollingProcessor(Dictionary<string, object> data, bool isDone);
    public delegate void PreProcessor(Dictionary<string, object> data, AsyncProcessor asyncHandler);
    public delegate void PollingProcessor(Dictionary<string, object> data, AsyncPollingProcessor asyncHandler);
    public delegate void AfterProcessor(Dictionary<string, object> data);

    #endregion

    public class AsyncPollingTimer
    {
        #region Fields

        private Timer timer;
        private Dictionary<string, object> data;
        private bool done = false;
        private long dueTime = 0;
        private long period = 1000;

        private PreProcessor PrePolling;
        private AfterProcessor AfterPolling;
        private PollingProcessor OnPolling;

        #endregion

        #region Constructors

        public AsyncPollingTimer(PreProcessor prePolling, PollingProcessor polling, AfterProcessor afterPolling, Dictionary<string, object> data)
        {
            this.PrePolling = prePolling == null ? (_data, _asyncHandler) => { } : prePolling;
            this.AfterPolling = afterPolling == null ? _data => { } : afterPolling;
            this.OnPolling = polling;
            this.data = data == null ? new Dictionary<string, object>() : data;

            if (this.OnPolling == null)
            {
                throw new ArgumentException("The polling event can not be null.");
            }
        }

        public AsyncPollingTimer(PreProcessor prePolling, PollingProcessor polling, AfterProcessor afterPolling, Dictionary<string, object> data, long dueTime, long period)
            : this(prePolling, polling, afterPolling, data)
        {
            this.dueTime = dueTime;
            this.period = period;
        }

        #endregion

        #region Type Specific Methods

        public void Start()
        {
            this.PrePolling(this.data, _data => 
            {
                //这里设置Timeout.Infinite只是为了timer只调用一次
                this.timer = new Timer(this.TimeoutCallback, this, this.dueTime, Timeout.Infinite);
            });
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
            AsyncPollingTimer timer = state as AsyncPollingTimer;

            if (!timer.done)
            {
                timer.OnPolling(timer.data, (_data, _isDone) =>
                {
                    if(_isDone)
                    {
                        timer.Stop();

                        timer.AfterPolling(_data);
                    }
                    else
                    {
                        /*
                            由于事件仍需继续，这里改变timer，让timer在period时间之后再次触发一次。
                            这里这样的做法是为了防止OnPolling执行的时间过长，导致timer已经到了再次触发的时间再次触发，
                            最后导致多个线程池的线程同时执行TimeoutCallback这个回调方法。
                        */
                        timer.timer.Change(timer.period, Timeout.Infinite);
                    }
                });
            }
        }

        #endregion
    }
}