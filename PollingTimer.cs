public delegate void UnitProcessor(Dictionary<string, object> data);
public delegate bool PollingStoper(Dictionary<string, object> data);

public class PollingTimer
{
    #region Fields

    private Timer timer;
    private Dictionary<string, object> data;
    private bool done = false;
    private AutoResetEvent autoEvent;

    private UnitProcessor PrePolling;
    private UnitProcessor AfterPolling;
    private PollingStoper OnPolling;

    #endregion

    #region Constructors

    public PollingTimer(UnitProcessor prePolling, UnitProcessor afterPolling, PollingStoper polling, Dictionary<string, object> data)
    {
        this.PrePolling = prePolling;
        this.AfterPolling = afterPolling;
        this.OnPolling = polling;
        this.data = data;

        if(this.data == null)
        {
            this.data = new Dictionary<string, object>();
        }

        if(this.PrePolling == null 
            ||
           this.AfterPolling == null
            ||
           this.OnPolling == null)
        {
            throw new ArgumentException("The polling event can not be null.");
        }
    }

    #endregion

    #region Type Specific Methods

    public void Start()
    {
        this.PrePolling(this.data);

        this.autoEvent = new AutoResetEvent(true);

        this.timer = new Timer(this.TimeoutCallback, this, 0, 1000);
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

        timer.autoEvent.WaitOne();

        if (!timer.done)
        {
            if (timer.OnPolling(timer.data))
            {
                timer.Stop();

                timer.AfterPolling(timer.data);
            }

            timer.autoEvent.Set();
        }
    }

    #endregion
}